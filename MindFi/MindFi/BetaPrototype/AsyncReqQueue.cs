using System;
using System.Collections;
using System.Data.SQLite;

namespace MyBackup
{
    /// <summary>
    /// Class that contains methods to manage the asynchronous request queue
    /// </summary>

    public class AsyncReqQueue
    {
        // USE 1 for debugging issues that are not related to concurrency
        const int SIZETOGETPERPAGE = 200;
        //const int SIZETOGETPERPAGE = 1; // debug

        #region "Properties"
        public long ID;
        public string ReqType;
        public string ReqURL;
        public string FileName;
        public string Response;
        public DateTime Created;
        public DateTime Updated;
        public int Limit;
        public int Priority;
        public int State; // TODO: substitute by enum
        public long? Parent;
        public string ParentSNID;
        public CallBack methodToCall;

        public static string ProfilePhotoDestinationDir;
        public static string AlbumDestinationDir;
        #endregion

        public const int POTENTIAL = 0;
        public const int QUEUED = 1;
        public const int SENT = 2;
        public const int RETRY = 3;
        public const int RECEIVED = 4;
        public const int PARSED = 5;
        public const int PROCESSED = 6;
        public const int TODELETE = 7;

        private static volatile int NextID = -1;
        public bool addToken = true;
        public bool Saved = false;
        public string ErrorMessage;

        #region "Statistics"
        public static volatile int nFriends = 0;
        public static volatile int nFriendsProcessed = 0;
        public static volatile int nFriendsPictures = 0;
        public static volatile int nPosts = 0;
        public static volatile int nMessages = 0;
        public static volatile int nAlbums = 0;
        public static volatile int nPhotos = 0;

        private static volatile int nNotParsedRequests = 0;
        private static volatile int nFailedRequests = 0;
        private static volatile int[] CountPerState = null;

        #endregion
        private static volatile Object obj = new Object();

        public static int QueuedReqs
        {
            get { if (CountPerState == null) { return 0; }  else return CountPerState[QUEUED]; }
        }

        public static int SentReqs
        {
            get { if (CountPerState == null) { return 0; } else return CountPerState[SENT]; }
        }

        public static int RetryReqs
        {
            get { if (CountPerState == null) { return 0; } else return CountPerState[RETRY]; }
        }

        public static int ReceivedReqs
        {
            get { if (CountPerState == null) { return 0; } else return CountPerState[RECEIVED]; }
        }

        public static int ParsedReqs
        {
            get { if (CountPerState == null) { return 0; } else return CountPerState[PARSED]; }
        }

        public static int NotParsedReqs
        {
            get { return nNotParsedRequests; }
        }

        public static int FailedReqs
        {
            get { return nFailedRequests; }
        }

        #region "Constructors"

        public AsyncReqQueue(string type, string URL, int limit, CallBack resultCall, bool AddToken = true, long? parentID = null, string parentSNID = "")
        {
            lock (obj)
            {
                if (NextID == -1)
                {
                    // read from database
                    string errorMessage = "";
                    NextID = DBLayer.AvailableReqQueueSave(out errorMessage);
                    if (NextID == -1) NextID = 0;
                }
                ID = NextID++;
            }

            ReqType = type;
            ReqURL = URL;
            Created = DateTime.Now;
            FileName = "";
            Priority = 0; // minimal
            Limit = limit;
            State = POTENTIAL;
            Parent = parentID;
            ParentSNID = parentSNID;
            methodToCall = resultCall;
            addToken = AddToken;
            InitArray();
            Save();
        }

        public AsyncReqQueue(string type, string URL, string filename, int limit, CallBack resultCall, long? parentID, string parentSNID)
        {
            lock (obj)
            {
                if (NextID == -1)
                {
                    // read from database
                    string errorMessage = "";
                    NextID = DBLayer.AvailableReqQueueSave(out errorMessage);
                    if (NextID == -1) NextID = 0;
                }
                ID = NextID++;
            }

            ReqType = type;
            ReqURL = URL;
            Created = DateTime.Now;
            FileName = filename;
            Priority = 0;
            Limit = limit;
            State = POTENTIAL;
            methodToCall = resultCall;
            Parent = parentID;
            ParentSNID = parentSNID;
            InitArray();
            Save();
        }

        /// <summary>
        /// Constructor that creates an AsyncReqQueue object from the RequestsQueue table in the database
        /// </summary>
        public AsyncReqQueue(int id)
        {
            string ErrorMessage;
            // Call layer to get data
            InitArray();
            if (DBLayer.GetAsyncReq(id, out ReqType, out Priority, out Parent, out ParentSNID,
            out Created, out Updated, out ReqURL, out State, out FileName, out addToken, out ErrorMessage))
            {
                ID = id;
                Saved = true; // to make sure it asks for update later
                return;
            }
            else
            {
                ID = -1;
                System.Windows.Forms.MessageBox.Show("Couldn't find request " + id + ", error: " + ErrorMessage);
                return;
            }
        }

        private void InitArray()
        {
            lock (obj)
            {
                if (CountPerState == null)
                {
                    CountPerState = new int[AsyncReqQueue.TODELETE + 1];
                    for (int i = 0; i < AsyncReqQueue.TODELETE + 1; i++) CountPerState[i] = 0;
                }
            }
        }

        #endregion

        #region "Per request methods"
        public bool Queue()
        {
            if (State == POTENTIAL)
            {
                lock (obj)
                {
                    CountPerState[QUEUED]++;
                }
                State = QUEUED;
                // TODO: separate status?
                Save();
                return true;
            }
            System.Windows.Forms.MessageBox.Show("ERROR: AsyncReq ID " + ID + " Not in the appropriate state for being Queued");
            return false;
        }

        public bool Send()
        {
            if (State == QUEUED || State == RETRY)
            {
                lock (obj)
                {
                    CountPerState[SENT]++;
                    CountPerState[QUEUED]--;
                }
                State = SENT;
                Save();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("ERROR: AsyncReq ID " + ID + " Not in the appropriate state for being Sent");
                return false;
            }
            bool result = false;
            // TODO: Evaluate appropriate data type, pass a limit that is not 1, paging
            if (FileName == "")
            {
                // DEBUG
                if (!addToken)
                {
                    //System.Windows.Forms.MessageBox.Show("Ready to call graph api: " + ReqURL + "\nID: " +ID);
                }

                result = FBAPI.CallGraphAPI(ReqURL, Limit, methodToCall, ID, Parent, ParentSNID, addToken);
            }
            else
            {
                // DEBUG
                if (!addToken)
                {
                    //System.Windows.Forms.MessageBox.Show("Ready to call graph api: " + ReqURL + "\nID: " +ID);
                }
                result = FBAPI.CallGraphAPI(ReqURL, FileName, methodToCall, ID, Parent, ParentSNID);
            }
            return result;
        }

        public void Save()
        {
            Updated = DateTime.Now;
            // TODO: Save limit, update when response comes back...
            Saved = DBLayer.ReqQueueSave(ID, ReqType, Priority, Parent, ParentSNID, Created, Updated, ReqURL, State, FileName, addToken, Saved, out ErrorMessage);
            if (!Saved)
            {
                System.Windows.Forms.MessageBox.Show("Failed to save request: " + ID + " error: " + ErrorMessage);
            }
        }
        #endregion

        #region "Static externally used methods"
        /// <summary>
        /// Create the first, basic requests for the first time logged in
        /// </summary>
        public static void InitialRequests(int MinPriority)
        {
            // Update old requests for retry
            if (!DBLayer.QueueRetryUpdate())
            {
                return;
            }
            string error;
            int[] stateArray;
            if ( DBLayer.GetNRequestsPerState(out stateArray,  out error) )
            {
                CountPerState = stateArray;
                if (CountPerState[QUEUED] + CountPerState[SENT] + CountPerState[RETRY] > 0)
                {
                    // TODO: make sure received are parsed/processed by PendingRequests too
                    CountPerState[RECEIVED] = 0;
                    PendingRequests(MinPriority, out error);
                }
                else
                {
                    DBLayer.StartBackup();
                    // TODO: Modularize the priority/decide if send
                    AsyncReqQueue apiReq = FBAPI.ProfilePic(FBLogin.Me.SNID,
                        ProfilePhotoDestinationDir + FBLogin.Me.ID + ".jpg",
                        ProcessFriendPic, FBLogin.Me.ID, FBLogin.Me.SNID);
                    apiReq.Priority = 999;
                    apiReq.Queue();
                    if ( apiReq.Priority >= MinPriority )
                        apiReq.Send();
                    apiReq = FBAPI.Friends("me", SIZETOGETPERPAGE, ProcessFriends);
                    apiReq.Priority = 999;
                    apiReq.Queue();
                    apiReq.Send();
                    apiReq = FBAPI.Wall("me", SIZETOGETPERPAGE, ProcessWall);
                    apiReq.Priority = 999;
                    apiReq.Queue();
                    if (apiReq.Priority >= MinPriority)
                        apiReq.Send();
                    apiReq = FBAPI.Inbox("me", SIZETOGETPERPAGE, ProcessInbox);
                    apiReq.Priority = 999;
                    apiReq.Queue();
                    if (apiReq.Priority >= MinPriority)
                        apiReq.Send();
                    apiReq = FBAPI.Events("me", SIZETOGETPERPAGE, ProcessEvents);
                    apiReq.Priority = 500;
                    apiReq.Queue();
                    if (apiReq.Priority >= MinPriority)
                        apiReq.Send();
                    apiReq = FBAPI.PhotoAlbums("me", SIZETOGETPERPAGE, ProcessAlbums);
                    apiReq.Priority = 500;
                    apiReq.Queue();
                    if (apiReq.Priority >= MinPriority)
                        apiReq.Send();
                    apiReq = FBAPI.FriendLists("me", SIZETOGETPERPAGE, ProcessFriendLists, FBLogin.Me.ID);
                    apiReq.Priority = 500;
                    apiReq.Queue();
                    if (apiReq.Priority >= MinPriority)
                        apiReq.Send();
                }
            }
        }

        /// <summary>
        /// Follows up on pending requests
        /// </summary>
        public static void PendingRequests(int MinPriority, out string ErrorMessage)
        {
            ErrorMessage = "";
            // Get top queued requests
            // TODO: Check formula now that received could stay for long
            int ConcurrentRequestLimit = 25;
            if (CountPerState[SENT] - CountPerState[RECEIVED] - nFailedRequests < ConcurrentRequestLimit)
            {
                ConcurrentRequestLimit -= CountPerState[SENT] - CountPerState[RECEIVED] - nFailedRequests;
                ArrayList queueReq = DBLayer.GetRequests(ConcurrentRequestLimit, AsyncReqQueue.QUEUED, out ErrorMessage, MinPriority);
                // Assigning callback appropriately
                if (ErrorMessage != "")
                {
                    System.Windows.Forms.MessageBox.Show("Error getting queued requests: " + ErrorMessage);
                    return;
                }
                ErrorMessage = "Queued Requests to send: " + queueReq.Count + "\n";
                if (queueReq.Count == 0)
                {
                    queueReq = DBLayer.GetRequests(ConcurrentRequestLimit, AsyncReqQueue.RETRY, out ErrorMessage, MinPriority);
                    if (ErrorMessage != "")
                    {
                        System.Windows.Forms.MessageBox.Show("Error getting retry requests: " + ErrorMessage);
                        return;
                    }

                    ErrorMessage = "Retry Requests to send: " + queueReq.Count + "\n";
                    if (queueReq.Count == 0)
                    {
                        DBLayer.EndBackup();
                    }
                }
                foreach (int reqID in queueReq)
                {
                    //System.Windows.Forms.MessageBox.Show("id" + reqID);
                    AsyncReqQueue apiReq = new AsyncReqQueue(reqID);
                    // special case: ID is not the requested, the request is not there any longer?
                    if (apiReq.ID != -1)
                    {
                        switch (apiReq.ReqType)
                        {
                            case "FBPerson": // Friend info processing callback
                                apiReq.methodToCall = ProcessOneFriend;
                                break;
                            case "FBWall":
                                apiReq.methodToCall = ProcessWall;
                                break;
                            case "FBAlbums":
                                apiReq.methodToCall = ProcessAlbums;
                                break;
                            case "FBInbox":
                                apiReq.methodToCall = ProcessInbox;
                                break;
                                // TODO: Consolidate
                            case "FBFriends":
                                apiReq.methodToCall = ProcessFriends;
                                break;
                            case "FBFriendList":
                                apiReq.methodToCall = ProcessList;
                                break;
                                // END TODO
                            case "FBFriendLists":
                                apiReq.methodToCall = ProcessFriendLists;
                                break;
                            case "FBProfilePic":
                                apiReq.methodToCall = ProcessFriendPic;
                                break;
                            case "FBPhotos":
                                apiReq.methodToCall = ProcessPhotosInAlbum;
                                break;
                            case "FBPhoto":
                                apiReq.methodToCall = ProcessPhoto;
                                break;
                            case "FBLikes":
                                apiReq.methodToCall = ProcessLikes;
                                break;
                            case "FBEvents":
                                apiReq.methodToCall = ProcessEvents;
                                break;
                            default:
                                System.Windows.Forms.MessageBox.Show("Unsupported request type: " + apiReq.ReqType);
                                break;
                        }
                        ErrorMessage += "ID: " + apiReq.ID + " Type: " + apiReq.ReqType + " Pri: " + apiReq.Priority + " URL: " + apiReq.ReqURL + "\n";
                        // TODO: verify, this should be redundant since the query already filters
                        if ( apiReq.Priority >= MinPriority )
                            apiReq.Send();
                    } // if
                } // foreach
            } // if not too many requests pending...
            else
            {
                ErrorMessage = "Waiting for " + (CountPerState[SENT] - CountPerState[RECEIVED] - nFailedRequests) + " to be responded before sending more...";
            }
        }

        #endregion

        #region "Response/Callback methods"

        /// <summary>
        /// General processing after the specific request processing
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static bool GenericProcess(int hwnd, bool result, string response, JSONParser temp, bool save = true, long? parent = null, string parentSNID = "")
        {
            if (result)
            {
                CountPerState[RECEIVED]++;
            }
            else
            {
                nFailedRequests++;
            }
            if (result)
            {
                string error;
                if (save)
                {
                    temp.Save(out error);
                }
            }
            return true;
        }

        /// <summary>
        /// Process a profile picture
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessFriendPic(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            string errorData = "";
            nNotParsedRequests++;
            //System.Windows.Forms.MessageBox.Show("Processing Pic: " + result );

            if (result)
            {
                nFriendsPictures++;
                CountPerState[RECEIVED]++;
                if (parent != null)
                {
                    if (parent != null && response.IndexOf("jpg") > 0)
                    {
                        DBLayer.UpdateProfilePic((long)parent, response, out errorData);
                        if (errorData == "") return true;
                    }
                }
            }
            // fails to process the pic, probably because of an exception
            nFailedRequests++;
            // TODO: check if changing requests to Retry
            return false;
        }

        /// <summary>
        /// Get friend list
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessFriends(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (!result)
            {
                // System.Windows.Forms.MessageBox.Show("Processing friends (false): " + response );
            }

            FBCollection friends = null;
            if (result)
            {
                friends = new FBCollection(response, "FBPerson");
                friends.Distance = 1;
                friends.Parse();
                CountPerState[PARSED]++;

                if (friends.Next != null)
                {
                    //System.Windows.Forms.MessageBox.Show("friends next: " + friends.Next);
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBFriends", friends.Next, SIZETOGETPERPAGE, ProcessFriends);
                    apiReq.Priority = 999;
                    apiReq.Queue();
                    //System.Windows.Forms.MessageBox.Show("apiReq URL: " + apiReq.ReqURL);
                }

                // save all children...
                string error;
                friends.Save(out error);
                foreach (FBPerson item in friends.items)
                {
                    nFriends++;
                    AsyncReqQueue apiReq;
                    apiReq = FBAPI.Profile(item.SNID, ProcessOneFriend);
                    apiReq.Priority = 400;
                    apiReq.Queue();
                    apiReq = FBAPI.ProfilePic(item.SNID,
                    ProfilePhotoDestinationDir + item.ID + ".jpg",
                    ProcessFriendPic, item.ID, item.SNID);
                    apiReq.Priority = 999;
                    apiReq.Queue();                    
                    apiReq = FBAPI.Wall(item.SNID, SIZETOGETPERPAGE, ProcessWall);
                    apiReq.Priority = 200;
                    apiReq.Queue();
                    apiReq = FBAPI.PhotoAlbums(item.SNID, SIZETOGETPERPAGE, ProcessAlbums);
                    apiReq.Priority = 50;
                    apiReq.Queue();
                }
            }
            //friendsNotParsed = friends;
            //NumFriends = friends.CurrentNumber;
            return GenericProcess(hwnd, result, response, friends, false);
        }

        /// <summary>
        /// process a round of posts
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessWall(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            //System.Windows.Forms.MessageBox.Show("Processing wall: " + response );
            if (!result)
            {
                //System.Windows.Forms.MessageBox.Show("Processing wall (false): " + response );
            }
            FBCollection wall = null;
            if (result)
            {
                wall = new FBCollection(response, "FBPost");
                wall.Parse();
                CountPerState[PARSED]++;

                string error;
                wall.Save(out error);
                nPosts += wall.CurrentNumber;

                if (error != "")
                {
                    //System.Windows.Forms.MessageBox.Show ( "Error saving wall for parent " + parent + "\n" + error );
                }

                foreach (FBPost post in wall.items)
                {
                    if (post.LikesCount > 0)
                    {
                        AsyncReqQueue apiReq = FBAPI.Likes(post.SNID, SIZETOGETPERPAGE, ProcessLikes, post.ID);
                        // TODO: Check priority depending on own wall or friend wall
                        apiReq.Priority = 200;
                        apiReq.Queue();
                    }
                }


                if (wall.Next != null)
                {
                    //System.Windows.Forms.MessageBox.Show("Wall next: " + wall.Next);
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBWall", wall.Next, SIZETOGETPERPAGE, ProcessWall);
                    // TODO: Check priority depending on own wall or friend wall
                    apiReq.Priority = 999;
                    apiReq.Queue();
                }
            }
            return GenericProcess(hwnd, result, response, wall, false);
        }

        /// <summary>
        /// process one friend profile
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessOneFriend(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (result)
            {
                CountPerState[RECEIVED]++;
                nFriendsProcessed++;
                FBPerson currentFriend = new FBPerson(response, 1, null);
                currentFriend.Parse();
                CountPerState[PARSED]++;

                string errorData;
                currentFriend.Save(out errorData);
                if (errorData != "")
                {
                    //System.Windows.Forms.MessageBox.Show("Error saving friend: " + errorData);
                    return false;
                }
                return true;
            }
            else
            {
                nFailedRequests++;
            }
            return false;
        }


        /// <summary>
        /// process a round of messages
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessInbox(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            //System.Windows.Forms.MessageBox.Show("Processing inbox: " + response );
            if (!result)
            {
                //System.Windows.Forms.MessageBox.Show("Processing inbox (false): " + response );
            }
            FBCollection inbox = null;
            if (result)
            {
                inbox = new FBCollection(response, "FBMessage");
                inbox.Parse();
                CountPerState[PARSED]++;

                string error;
                inbox.Save(out error);
                nMessages += inbox.CurrentNumber;

                if (error != "")
                {
                    //System.Windows.Forms.MessageBox.Show("Error saving inbox " + error );
                    return false;
                }

                if (inbox.Next != null)
                {
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBInbox", inbox.Next, SIZETOGETPERPAGE, ProcessInbox);
                    apiReq.Priority = 999;
                    apiReq.Queue();
                }

            }
            return GenericProcess(hwnd, result, response, inbox, false);
        }

        /// <summary>
        /// process a round of albums
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessAlbums(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (!result)
            {
                //System.Windows.Forms.MessageBox.Show("Processing albums (false): " + response );
            }
            FBCollection albums = null;
            if (result)
            {
                albums = new FBCollection(response, "FBAlbum");
                albums.Parse();
                //System.Windows.Forms.MessageBox.Show("Number of albums: " + albums.CurrentNumber );
                CountPerState[PARSED]++;
                nAlbums += albums.CurrentNumber;

                string error;
                albums.Save(out error);
                if (error != "")
                {
                    //System.Windows.Forms.MessageBox.Show("Error saving albums " + error );
                    return false;
                }

                foreach (FBAlbum album in albums.items)
                {
                    if (!System.IO.Directory.Exists(AlbumDestinationDir + "\\" + album.ID))
                    {
                        System.IO.Directory.CreateDirectory(AlbumDestinationDir + "\\" + album.ID);
                    }

                    //System.Windows.Forms.MessageBox.Show("sending req for photos in album " + album.SNID + " ID: " + album.ID );
                    AsyncReqQueue apiReq = FBAPI.PhotosInAlbum(album.SNID, SIZETOGETPERPAGE, ProcessPhotosInAlbum, album.ID);
                    apiReq.Priority = 400;
                    apiReq.Queue();
                    apiReq = FBAPI.Likes(album.SNID, SIZETOGETPERPAGE, ProcessLikes, album.ID);
                    apiReq.Priority = 200;
                    apiReq.Queue();
                }

                if (albums.CurrentNumber > 0 && albums.Next != null)
                {
                    //System.Windows.Forms.MessageBox.Show("Albums next: " + albums.Next);
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBAlbums", albums.Next, SIZETOGETPERPAGE, ProcessAlbums);
                    // TODO: Check if they are my albums or friend's
                    apiReq.Priority = 500;
                    apiReq.Queue();
                }
            }
            return GenericProcess(hwnd, result, response, albums, false);
        }

        /// <summary>
        /// process a set of photos
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessPhotosInAlbum(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (!result)
            {
                //System.Windows.Forms.MessageBox.Show("Processing photos (false): " + response );
            }
            FBCollection photos = null;
            if (result)
            {
                // TODO: Check how to pass parent SNID all around
                photos = new FBCollection(response, "FBPhoto", parent);
                photos.Parse();
                //System.Windows.Forms.MessageBox.Show("Number of photos: " + photos.CurrentNumber );
                CountPerState[PARSED]++;
                nPhotos += photos.CurrentNumber;

                string error;
                photos.Save(out error);
                if (error != "")
                {
                    System.Windows.Forms.MessageBox.Show("Error saving photos " + error);
                    return false;
                }

                foreach (FBPhoto photo in photos.items)
                {
                    // System.Windows.Forms.MessageBox.Show("will download photo: " + photo.ID);
                    AsyncReqQueue apiReq = FBAPI.DownloadPhoto(photo.Source,
                    AlbumDestinationDir + parent + "\\" + photo.ID + ".jpg",
                    ProcessPhoto, photo.ID, photo.SNID);
                    // TODO: Check if it is my photo or other's
                    apiReq.Priority = 200;
                    apiReq.Queue();
                    apiReq = FBAPI.Likes(photo.SNID, SIZETOGETPERPAGE, ProcessLikes, photo.ID);
                    apiReq.Priority = 150;
                    apiReq.Queue();
                }

                // Next is not null, but it should since we get all accepted photos on the first round... 
                // TODO: Analyze how to use Next appropriately
                /*
                if (photos.Next != null)
                {
                    System.Windows.Forms.MessageBox.Show("Photos next: " + photos.Next);
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBPhotos", photos.Next, SIZETOGETPERPAGE, ProcessPhotosInAlbum);
                    apiReq.Priority = 50;
                    apiReq.Queue();
                }
                */
            }
            return GenericProcess(hwnd, result, response, photos, false);
        }

        /// <summary>
        /// Process a photo
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessPhoto(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            string errorData = "";
            nNotParsedRequests++;
            // System.Windows.Forms.MessageBox.Show("Processing Photo parentID: " + parent + "\n" + result );

            if (result)
            {
                CountPerState[RECEIVED]++;
                if (parent != null)
                {
                    if (parent != null && response.IndexOf("jpg") > 0)
                    {
                        DBLayer.UpdatePhoto((long)parent, response, out errorData);
                        if (errorData == "") return true;
                    }
                }
            }
            // fails to process the pic, probably because of an exception
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process likes
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessLikes(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            string errorData = "";
            //System.Windows.Forms.MessageBox.Show("Processing likes result: " + parent + "\n" + response );

            if (result)
            {
                CountPerState[RECEIVED]++;
                if (parent != null)
                {
                    // create FBLikes object
                    FBLikes likes = new FBLikes(response, parent, parentSNID);
                    likes.Parse();
                    CountPerState[PARSED]++;
                    likes.Save(out errorData);
                    if (errorData == "") return true;

                }
            }
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process list of events
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessEvents(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            string errorData = "";
            //System.Windows.Forms.MessageBox.Show("Processing events result: " + result + "\n" + response );

            if (result)
            {
                CountPerState[RECEIVED]++;
                FBCollection events = new FBCollection(response, "FBEvent", parent, parentSNID);
                events.Parse();
                CountPerState[PARSED]++;
                events.Save(out errorData);
                if (errorData == "") return true;
            }
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process list of friendlists
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool ProcessFriendLists(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            string errorData = "";
            //System.Windows.Forms.MessageBox.Show("Processing friendlists result: " + result + "\n" + response );

            if (result)
            {
                CountPerState[RECEIVED]++;
                FBCollection lists = new FBCollection(response, "FBFriendList", parent, parentSNID);
                lists.Parse();
                CountPerState[PARSED]++;
                lists.Save(out errorData);

                foreach (FBFriendList list in lists.items)
                {
                    AsyncReqQueue apiReq = FBAPI.Members(list.SNID, SIZETOGETPERPAGE, ProcessList, list.ID);
                    apiReq.Priority = 400;
                    apiReq.Queue();
                }

                if (errorData == "") return true;

            }
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process list of friendlists
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>        
        public static bool ProcessList(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            // TODO: Consolidate with ProcessFriends, maybe with an empty name or with the default name All Friends
            string errorData = "";
            //System.Windows.Forms.MessageBox.Show("Processing friendlist result: " + result + "\n" + response);

            if (result)
            {
                CountPerState[RECEIVED]++;
                FBCollection friends = new FBCollection(response, "FBPerson", parent, parentSNID);
                friends.Parse();
                CountPerState[PARSED]++;
                friends.Save(out errorData);
                // save the list and association
                if (errorData == "") return true;
            }
            nFailedRequests++;
            return false;
        }
        #endregion

    }
}
