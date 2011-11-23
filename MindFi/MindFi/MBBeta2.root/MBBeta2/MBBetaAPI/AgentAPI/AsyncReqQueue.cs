using System;
using System.Collections;
using System.Data.SQLite;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that contains methods to manage the asynchronous request queue
    /// </summary>

    public class AsyncReqQueue
    {
        const int SIZETOGETPERPAGE = 50; // USE 1 for debugging issues that are not related to concurrency

        #region "Public Properties"
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

        public string ErrorMessage;
        #endregion

        #region "Constants"
        public const int POTENTIAL = 0;
        public const int QUEUED = 1;
        public const int SENT = 2;
        public const int RETRY = 3;
        public const int RECEIVED = 4;
        public const int PARSED = 5;
        public const int PROCESSED = 6;
        public const int TODELETE = 7;
        #endregion

        #region "Statistics"
        public static volatile int nFriends = 0;
        public static volatile int nFriendsProcessed = 0;
        public static volatile int nFriendsPictures = 0;
        public static volatile int nPosts = 0;
        public static volatile int nMessages = 0;
        public static volatile int nAlbums = 0;
        public static volatile int nPhotos = 0;

        private static volatile int nNotParsedRequests = 0;
        private static volatile int nInParseRequests = 0;
        private static volatile int nInSaveRequests = 0;
        private static volatile int nFailedRequests = 0;
        private static volatile int[] CountPerState = null;
        private static volatile int minPriorityGlobal = 0;
        private static volatile int currentPriorityGlobal = 999;
        private static volatile int currentBackupNumber = 0;
        private static volatile int nRequestsInTransit = 0;
        private static volatile bool RecoveringPendingReqs = false;

        #region "Statistic properties"
        public static int QueuedReqs
        {
            get { if (CountPerState == null) { return 0; } else return CountPerState[QUEUED]; }
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
        #endregion
        #endregion

        #region "Internal variables"
        private static volatile int NextID = -1;
        private bool addToken = true;
        private bool addDateRange = false;
        private bool Saved = false;
        private static volatile Object obj = new Object();
        #endregion

        #region "Constructors"

        /// <summary>
        /// Construct a JSON request based on most information
        /// </summary>
        /// <param name="type">Identifier for the expected return type</param>
        /// <param name="URL">Server URL that will be called to get the JSON result</param>
        /// <param name="limit">Number of JSON records to be returned</param>
        /// <param name="resultCall">Method to process/parse the response</param>
        /// <param name="AddToken">Indicates if the URL requires the authentication parameter</param>
        /// <param name="AddDateRange">Indicates if the URL requires the date limit parameter</param>
        /// <param name="parentID">Database ID for the parent object to which the result needs to be associated</param>
        /// <param name="parentSNID">Social Network ID for the parent object to which the result needs to be associated</param>
        public AsyncReqQueue(string type, string URL, int limit, CallBack resultCall, bool AddToken = true, bool AddDateRange = false, long? parentID = null, string parentSNID = "")
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
            addDateRange = AddDateRange;
            InitArray();
            Save();
        }

        /// <summary>
        /// Construct a file request based on appropriate information
        /// </summary>
        /// <param name="type">Identifier for the expected return type</param>
        /// <param name="URL">Server URL that will be called to get the file</param>
        /// <param name="filename">Name of the file to write the response to</param>
        /// <param name="limit">Number of JSON records to be returned</param>
        /// <param name="resultCall">Method to process/parse the response</param>
        /// <param name="parentID">Database ID for the parent object to which the result needs to be associated</param>
        /// <param name="parentSNID">Social Network ID for the parent object to which the result needs to be associated</param>
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
        /// <param name="id">Identifier in the database to query the request</param>
        public AsyncReqQueue(int id)
        {
            string ErrorMessage;
            // Call layer to get data
            InitArray();
            if (DBLayer.GetAsyncReq(id, out ReqType, out Priority, out Parent, out ParentSNID,
                out Created, out Updated, out ReqURL, out State, out FileName, out addToken, out addDateRange, out ErrorMessage))
            {
                ID = id;
                SetCallBackFunction();
                Saved = true; // to make sure it asks for update later
                return;
            }
            else
            {
                ID = -1;
                //System.Windows.Forms.MessageBox.Show("Couldn't find request " + id + ", error: " + ErrorMessage);
                return;
            }
        }

        /// <summary>
        /// Function used by the multiple constructors to cleanup the state count array
        /// </summary>
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

        /// <summary>
        /// Assigns the Callback on recovery
        /// </summary>
        private void SetCallBackFunction()
        {
            switch (ReqType)
            {
                case "FBPerson": // Friend info processing callback
                    methodToCall = ProcessOneFriend;
                    break;
                case "FBWall":
                    methodToCall = ProcessWall;
                    break;
                case "FBAlbums":
                    methodToCall = ProcessAlbums;
                    break;
                case "FBInbox":
                    methodToCall = ProcessInbox;
                    break;
                case "FBThread":
                    methodToCall = ProcessThread;
                    break;
                case "FBNotes":
                    methodToCall = ProcessNotes;
                    break;
                // TODO: Consolidate
                case "FBFriends":
                    methodToCall = ProcessFriends;
                    break;
                case "FBFamily":
                    methodToCall = ProcessFamily;
                    break;
                case "FBFriendList":
                    methodToCall = ProcessList;
                    break;
                // END TODO
                case "FBFriendLists":
                    methodToCall = ProcessFriendLists;
                    break;
                case "FBProfilePic":
                    methodToCall = ProcessFriendPic;
                    break;
                case "FBPhotos":
                    methodToCall = ProcessPhotosInAlbum;
                    break;
                case "FBPhoto":
                    methodToCall = ProcessPhoto;
                    break;
                case "FBLikes":
                    methodToCall = ProcessLikes;
                    break;
                case "FBAttending":
                    methodToCall = ProcessAttending;
                    break;
                case "FBMaybe":
                    methodToCall = ProcessMaybeAttending;
                    break;
                case "FBDeclined":
                    methodToCall = ProcessNotAttending;
                    break;
                case "FBEvents":
                    methodToCall = ProcessEvents;
                    break;
                case "FBEvent":
                    methodToCall = ProcessOneEvent;
                    break;
                case "FBPost":
                    methodToCall = ProcessOnePost;
                    break;
                case "FBNotifications":
                    methodToCall = ProcessNotifications;
                    break;
                default:
                    //System.Windows.Forms.MessageBox.Show("Unsupported request type: " + ReqType);
                    break;
            }
        }
        #endregion

        #region "Per request Queue management methods"
        /// <summary>
        /// Saves and moves the current object, which should be in the POTENTIAL state, to QUEUE state
        /// </summary>
        /// <returns>Success on queuing the object</returns>
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
            //System.Windows.Forms.MessageBox.Show("ERROR: AsyncReq ID " + ID + " Not in the appropriate state for being Queued");
            return false;
        }

        /// <summary>
        /// Internal variant that specifies priority for the request and decides accordingly if it should be queued or not
        /// </summary>
        /// <param name="priority">Higher (using 999), the first to be consumed from the queue</param>
        /// <returns></returns>
        private bool Queue(int priority)
        {
            bool success = true;
            Priority = priority;
            if (priority >= minPriorityGlobal)
            {
                success = Queue();
            }
            return success;
        }

        /// <summary>
        /// Saves and moves the current object, which should be in the QUEUE state, to SENT state, then submits the request to the server asynchronously
        /// </summary>
        /// <returns>Success on sending the request</returns>
        public bool Send()
        {
            if (State == QUEUED || State == RETRY)
            {
                lock (obj)
                {
                    CountPerState[SENT]++;
                    CountPerState[QUEUED]--;
                    nRequestsInTransit++;
                }
                State = SENT;
                Save();
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("ERROR: AsyncReq ID " + ID + " Not in the appropriate state for being Sent");
                return false;
            }
            bool result = false;
            if (FileName == "")
            {
                result = FBAPI.CallGraphAPI(ReqURL, Limit, methodToCall, ID, Parent, ParentSNID, addToken, addDateRange);
            }
            else
            {
                result = FBAPI.CallGraphAPI(ReqURL, FileName, methodToCall, ID, Parent, ParentSNID);
            }
            return result;
        }

        /// <summary>
        /// Internal variant that combines Queue and Send, specifies priority for the request and decides accordingly if it should be queued and send or not
        /// </summary>
        /// <param name="priority">Higher (using 999), the first to be consumed from the queue</param>
        /// <returns></returns>
        private bool QueueAndSend(int priority)
        {
            bool success = true;
            Priority = priority;
            if (priority >= minPriorityGlobal)
            {
                success = Queue();
                if (success)
                {
                    Send();
                }
                currentPriorityGlobal = priority;
            }
            return success;
        }

        /// <summary>
        /// Persists the request in the database
        /// </summary>
        public void Save()
        {
            Updated = DateTime.Now;
            // TODO: Save limit, update when response comes back...
            DateTime? start = ((SNAccount.CurrentProfile == null) ? (DateTime?)null : SNAccount.CurrentProfile.CurrentPeriodStart);
            DateTime? end = ((SNAccount.CurrentProfile == null) ? (DateTime?)null : SNAccount.CurrentProfile.CurrentPeriodEnd);
            Saved = DBLayer.ReqQueueSave(ID, ReqType, Priority, Parent, ParentSNID, Created, Updated, ReqURL, State, FileName, addToken, addDateRange, 
                start, end, Saved, out ErrorMessage);
            if (!Saved)
            {
                //System.Windows.Forms.MessageBox.Show("Failed to save request: " + ID + " error: " + ErrorMessage);
            }
        }

        #endregion

        #region "Static externally used methods"
        /// <summary>
        /// Create the first, basic requests for a new backup
        /// </summary>
        /// TODO: Possibly force a new full backup
        public static void InitialRequests(int MinPriority)
        {
            minPriorityGlobal = MinPriority;
            string error;
            int[] stateArray;
            if (DBLayer.GetNRequestsPerState(minPriorityGlobal, out stateArray, out error))
            {
                CountPerState = stateArray;
                // Update old requests for retry
                if (!DBLayer.QueueRetryUpdate())
                {
                    return;
                }
                else
                {
                    // verify this is working correctly
                    CountPerState[QUEUED] += CountPerState[RETRY];
                    CountPerState[RETRY] = 0;
                }
                // starts or finds existing backup to continue
                int currentBackup;
                DateTime currentPeriodStart, currentPeriodEnd;
                
                DBLayer.StartBackup(SNAccount.CurrentProfile.BackupPeriodStart, SNAccount.CurrentProfile.BackupPeriodEnd,
                        out currentBackup, out currentPeriodStart, out currentPeriodEnd);
                SNAccount.CurrentProfile.CurrentPeriodStart = currentPeriodStart;
                SNAccount.CurrentProfile.CurrentPeriodEnd = currentPeriodEnd;
                currentBackupNumber = currentBackup;

                if (CountPerState[QUEUED] + CountPerState[SENT] + CountPerState[RETRY] > 0)
                {
                    PendingRequests(MinPriority, out error);
                }
                else
                {

                    if (currentBackup != 0)
                    {
                        // Calculate dates for first iteration - TODO make sure they are recalculated until done
                        FBAPI.SetTimeRange(currentPeriodStart, currentPeriodEnd);
                        /*
                        SNAccount.CurrentProfile.CurrentPeriodStart = currentPeriodStart;
                        SNAccount.CurrentProfile.CurrentPeriodEnd = currentPeriodEnd;
                        currentBackupNumber = currentBackup;
                         */
                        AsyncReqQueue apiReq = FBAPI.ProfilePic(FBLogin.Me.SNID,
                            ProfilePhotoDestinationDir + FBLogin.Me.SNID + ".jpg",
                            ProcessFriendPic, FBLogin.Me.ID, FBLogin.Me.SNID);
                        apiReq.QueueAndSend(999);
                        apiReq = FBAPI.Friends("me", SIZETOGETPERPAGE, ProcessFriends);
                        apiReq.QueueAndSend(999);
                        apiReq = FBAPI.Family("me", SIZETOGETPERPAGE, ProcessFamily);
                        apiReq.QueueAndSend(999);
                        apiReq = FBAPI.Wall("me", SIZETOGETPERPAGE, ProcessWall);
                        apiReq.QueueAndSend(999);
                        apiReq = FBAPI.Inbox("me", SIZETOGETPERPAGE, ProcessInbox);
                        apiReq.QueueAndSend(999);
                        apiReq = FBAPI.Notes("me", SIZETOGETPERPAGE, ProcessNotes);
                        apiReq.QueueAndSend(999);
                        apiReq = FBAPI.Notifications("me", SIZETOGETPERPAGE, ProcessNotifications);
                        apiReq.QueueAndSend(999);
                        apiReq = FBAPI.Events("me", SIZETOGETPERPAGE, ProcessEvents);
                        apiReq.QueueAndSend(500);
                        apiReq = FBAPI.PhotoAlbums("me", SIZETOGETPERPAGE, ProcessAlbums);
                        apiReq.QueueAndSend(500);
                        apiReq = FBAPI.FriendLists("me", SIZETOGETPERPAGE, ProcessFriendLists, FBLogin.Me.ID);
                        apiReq.QueueAndSend(500);
                    }
                    else
                    {
                        // TODO: Localize, return to interface
                        error = "Backup couldn't be created";   
                    }
                }
            }
        }


        private static void ExitPendingWithError(string ErrorMessage)
        {
            //System.Windows.Forms.MessageBox.Show(ErrorMessage);
            lock (obj)
            {
                RecoveringPendingReqs = false;
            }
        }

        /// <summary>
        /// Follows up on pending requests, sending them by priority until exhausted to end the backup
        /// </summary>
        /// <param name="MinPriority"></param>
        /// <param name="ErrorMessage"></param>
        /// <returns></returns>
        public static bool PendingRequests(int MinPriority, out string ErrorMessage)
        {
            #region "Init"
            ErrorMessage = "";
            lock (obj)
            {
                if (RecoveringPendingReqs)
                    return true;
                RecoveringPendingReqs = true;
            }
            bool backupInProgress = true;

            minPriorityGlobal = MinPriority;
            #endregion

            // TODO: calculate how many requests were processed in the last time unit, and increase / decrease dynamically the number of requests to go
            int ConcurrentRequestLimit = 10;
            // TESTING: stop sending while Parse ( and save database ) is in progress
            if (nRequestsInTransit < ConcurrentRequestLimit && nInParseRequests <= 0 && nInSaveRequests <=0 )
            {
                ArrayList queueReq = DBLayer.GetRequests(ConcurrentRequestLimit - nRequestsInTransit, AsyncReqQueue.QUEUED, out ErrorMessage, minPriorityGlobal);
                if (ErrorMessage != "")
                {
                    ExitPendingWithError("Error getting queued requests: " + ErrorMessage);
                    return false;
                }
                ErrorMessage = "Queued Requests to send: " + queueReq.Count + "\n";
                if (queueReq.Count == 0)
                {
                    queueReq = DBLayer.GetRequests(ConcurrentRequestLimit - nRequestsInTransit, AsyncReqQueue.RETRY, out ErrorMessage, minPriorityGlobal);
                    if (ErrorMessage != "")
                    {
                        ExitPendingWithError("Error getting retry requests: " + ErrorMessage);
                        return false;
                    }

                    ErrorMessage = "Retry Requests to send: " + queueReq.Count + "\n";
                    if (queueReq.Count == 0)
                    {
                        if ( (SNAccount.CurrentProfile.CurrentPeriodStart <= SNAccount.CurrentProfile.BackupPeriodStart )
                            && nInParseRequests <= 0
                            && nInSaveRequests <= 0
                            && FBAPI.InFlight <= 0
                            )
                        {
                            // TODO: Make sure no pending threads before closing the backup
                            DBLayer.EndBackup();
                            backupInProgress = false;
                        }
                        else
                        {
                            // Go to the previous week
                            // TODO: Show which week is being processed
                            SNAccount.CurrentProfile.CurrentPeriodEnd = SNAccount.CurrentProfile.CurrentPeriodStart;
                            SNAccount.CurrentProfile.CurrentPeriodStart = SNAccount.CurrentProfile.CurrentPeriodStart.AddDays(-30);
                            FBAPI.SetTimeRange(SNAccount.CurrentProfile.CurrentPeriodStart, SNAccount.CurrentProfile.CurrentPeriodEnd);
                            // TODO: Verify which requests are really affected by periods
                            AsyncReqQueue apiReq = FBAPI.Wall("me", SIZETOGETPERPAGE, ProcessWall);
                            apiReq.QueueAndSend(999);
                            apiReq = FBAPI.Inbox("me", SIZETOGETPERPAGE, ProcessInbox);
                            apiReq.QueueAndSend(999);
                            apiReq = FBAPI.Notes("me", SIZETOGETPERPAGE, ProcessNotes);
                            apiReq.QueueAndSend(999);
                            apiReq = FBAPI.Notifications("me", SIZETOGETPERPAGE, ProcessNotifications);
                            apiReq.QueueAndSend(999);
                            apiReq = FBAPI.Events("me", SIZETOGETPERPAGE, ProcessEvents);
                            apiReq.QueueAndSend(500);
                            apiReq = FBAPI.PhotoAlbums("me", SIZETOGETPERPAGE, ProcessAlbums);
                            apiReq.QueueAndSend(500);
                        }
                    }
                }
                if (backupInProgress)
                {
                    ErrorMessage += "Current Backup " + currentBackupNumber + ", currently working on priority " + currentPriorityGlobal + ", limit " + minPriorityGlobal 
                        + " from " +SNAccount.CurrentProfile.CurrentPeriodStart 
                        + " to " +SNAccount.CurrentProfile.CurrentPeriodEnd
                        + "\nRequests in flight: " + FBAPI.InFlight
                        + " requests in save: " + nInSaveRequests
                        + " requests in parse: " + nInParseRequests
                        + "\n";
                            
                    foreach (int reqID in queueReq)
                    {
                        AsyncReqQueue apiReq = new AsyncReqQueue(reqID);
                        // special case: ID is not the requested, the request is not there any longer?
                        if (apiReq.ID != -1)
                        {
                            ErrorMessage += "ID: " + apiReq.ID + " Type: " + apiReq.ReqType + " Pri: " + apiReq.Priority + " URL: " + apiReq.ReqURL + "\n";
                            // DEBUG to confirm validation is redundant
                            //if (apiReq.Priority < minPriorityGlobal) MessageBox.Show("Unexpected low priority request, ID: " + apiReq.ID + " Type: " + apiReq.ReqType + " Pri: " + apiReq.Priority + " URL: " + apiReq.ReqURL + "\n");

                            apiReq.Send();
                        } // if
                    } // foreach
                }
                else
                {
                    ErrorMessage += "Current Backup " + currentBackupNumber + " done\n";
                }
            } // if not too many requests pending...
            else
            {
                ErrorMessage += "Current Backup " + currentBackupNumber + ", currently working on priority " + currentPriorityGlobal + ", limit " + minPriorityGlobal
                    + " from " + SNAccount.CurrentProfile.CurrentPeriodStart
                    + " to " + SNAccount.CurrentProfile.CurrentPeriodEnd
                    + "\nRequests in flight: " + FBAPI.InFlight
                    + " requests in save: " + nInSaveRequests
                    + " requests in parse: " + nInParseRequests
                    + "\n";
                ErrorMessage += "Waiting for " + nRequestsInTransit + " requests to be processed";
            }
            lock (obj)
            {
                RecoveringPendingReqs = false;
            }
            return backupInProgress;
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
                CountPerState[SENT]--;
            }
            else
            {
                nFailedRequests++;
                CountPerState[SENT]--;
            }
            if (result)
            {
                string error;
                if (save)
                {
                    nInSaveRequests++;
                    temp.Save(out error);
                    nInSaveRequests--;
                }
            }
            return true;
        }

        #region "File processing methods"
        /// <summary>
        /// Process a profile picture
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">Filename</param>
        /// <param name="parent">Reference to the friend database ID</param>
        /// <param name="parentSNID">Reference to the friend social network ID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessFriendPic(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            string errorData = "";
            nRequestsInTransit--;
            nNotParsedRequests++;
            CountPerState[SENT]--;

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
                // corrected bug: return an error without mark as failed
                return false;
            }
            // fails to process the pic, probably because of an exception
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process a photo from an album
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON photo data</param>
        /// <param name="parent">CHECK: Reference to the album ID</param>
        /// <param name="parentSNID">CHECK: Reference to the album SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessPhoto(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            string errorData = "";
            nNotParsedRequests++;
            if (result)
            {
                CountPerState[RECEIVED]++;
                if (parent != null)
                {
                    if (parent != null && response.IndexOf("jpg") > 0)
                    {
                        //  TODO: Check if can be consolidated with profile pic
                        DBLayer.UpdatePhoto((long)parent, response, out errorData);
                        if (errorData == "") return true;
                    }
                }
                // corrected bug: return an error without mark as failed
                return false;
            }
            // fails to process the pic, probably because of an exception
            nFailedRequests++;
            return false;
        }

        #endregion

        #region "Collection processing methods"
        /// <summary>
        /// Get friend list
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of person data</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessFriends(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            FBCollection friends = null;
            if (result)
            {
                friends = new FBCollection(response, "FBPerson");
                friends.Distance = 2;
                nInParseRequests++;
                friends.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                CountPerState[RECEIVED]--;

                if (friends.Next != null)
                {
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBFriends", friends.Next, SIZETOGETPERPAGE, ProcessFriends);
                    apiReq.Queue(999);
                }

                // save all children...
                string error;
                nInSaveRequests++;
                friends.Save(out error);
                nInSaveRequests--;
                foreach (FBPerson item in friends.items)
                {
                    nFriends++;
                    AsyncReqQueue apiReq;
                    apiReq = FBAPI.Profile(item.SNID, ProcessOneFriend);
                    apiReq.Queue(750);
                    apiReq = FBAPI.ProfilePic(item.SNID,
                        ProfilePhotoDestinationDir + item.SNID + ".jpg",
                        ProcessFriendPic, item.ID, item.SNID);
                    apiReq.Queue(750);
                    apiReq = FBAPI.Family(item.SNID, SIZETOGETPERPAGE, ProcessFamily);
                    apiReq.Queue(400);
                    apiReq = FBAPI.Wall(item.SNID, SIZETOGETPERPAGE, ProcessWall);
                    apiReq.Queue(400);
                    apiReq = FBAPI.Events(item.SNID, SIZETOGETPERPAGE, ProcessEvents);
                    apiReq.Queue(400);
                    apiReq = FBAPI.PhotoAlbums(item.SNID, SIZETOGETPERPAGE, ProcessAlbums);
                    apiReq.Queue(50);
                }
            }
            return GenericProcess(hwnd, result, response, friends, false);
        }

        /// <summary>
        /// process a round of posts
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of Posts</param>
        /// <param name="parent">Reference to the user database ID</param>
        /// <param name="parentSNID">Reference to the user social network ID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessWall(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            FBCollection wall = null;
            if (result)
            {
                wall = new FBCollection(response, "FBPost");
                nInParseRequests++;
                wall.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                CountPerState[RECEIVED]--;

                string error;
                nInSaveRequests++;
                wall.Save(out error);
                nInSaveRequests--;
                nPosts += wall.CurrentNumber;

                foreach (FBPost post in wall.items)
                {
                    if (post.LikesCount > 0)
                    {
                        AsyncReqQueue apiReq = FBAPI.Likes(post.SNID, SIZETOGETPERPAGE, ProcessLikes, post.ID);
                        apiReq.Queue(200);
                    }
                    if (post.CommentCount > post.Comments.Count)
                    {
                        AsyncReqQueue apiReq = FBAPI.Post(post.SNID, ProcessOnePost);
                        apiReq.Queue(minPriorityGlobal);
                    }
                }


                if (wall.Next != null)
                {
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBWall", wall.Next, SIZETOGETPERPAGE, ProcessWall);
                    apiReq.Queue(minPriorityGlobal);
                }
            }
            return GenericProcess(hwnd, result, response, wall, false);
        }

        /// <summary>
        /// process a round of messages
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of threads</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessThread(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            return ProcessThread(hwnd, result, response, parent, parentSNID, false);
        }

        public static bool ProcessThread(int hwnd, bool result, string response, long? parent = null, string parentSNID = "", bool processChildren = false)
        {
            nRequestsInTransit--;
            FBCollection inbox = null;
            if (result)
            {
                inbox = new FBCollection(response, "FBMessage");
                nInParseRequests++;
                inbox.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                CountPerState[RECEIVED]--;

                string error;
                nInSaveRequests++;
                inbox.Save(out error);
                nInSaveRequests--;
                nMessages += inbox.CurrentNumber;

                if (error != "")
                {
                    return false;
                }

                if (processChildren)
                {
                    foreach (FBMessage message in inbox.items)
                    {
                        AsyncReqQueue apiReq = FBAPI.Thread(message.SNID, ProcessInbox);
                        apiReq.Queue(minPriorityGlobal);
                    }

                    if (inbox.Next != null)
                    {
                        AsyncReqQueue apiReq = FBAPI.MoreData("FBInbox", inbox.Next, SIZETOGETPERPAGE, ProcessInbox);
                        apiReq.Queue(minPriorityGlobal);
                    }
                }
            }
            return GenericProcess(hwnd, result, response, inbox, false);
        }

        /// <summary>
        /// process a round of messages
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of threads</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessInbox(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            return ProcessThread(hwnd, result, response, parent, parentSNID, true);
        }

        /// <summary>
        /// process a round of notes
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of notes</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        // TODO: Consolidate with ProcessInbox and similar routines
        public static bool ProcessNotes(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            FBCollection notes = null;
            if (result)
            {
                notes = new FBCollection(response, "FBNote");
                nInParseRequests++;
                notes.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                CountPerState[RECEIVED]--;

                string error;
                nInSaveRequests++;
                notes.Save(out error);
                nInSaveRequests--;
                nMessages += notes.CurrentNumber;

                if (error != "")
                {
                    return false;
                }

                if (notes.Next != null)
                {
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBNotes", notes.Next, SIZETOGETPERPAGE, ProcessInbox);
                    apiReq.Queue(minPriorityGlobal);
                }

            }
            return GenericProcess(hwnd, result, response, notes, false);
        }

        /// <summary>
        /// process a round of albums
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of notes</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessAlbums(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            FBCollection albums = null;
            if (result)
            {
                albums = new FBCollection(response, "FBAlbum");
                nInParseRequests++;
                albums.Parse();
                nInParseRequests--;
                
                CountPerState[PARSED]++;
                CountPerState[RECEIVED]--;
                nAlbums += albums.CurrentNumber;

                string error;
                nInSaveRequests++;
                albums.Save(out error);
                nInSaveRequests--;
                if (error != "")
                {
                    return false;
                }

                foreach (FBAlbum album in albums.items)
                {
                    if (!System.IO.Directory.Exists(AlbumDestinationDir + "\\" + album.SNID))
                    {
                        System.IO.Directory.CreateDirectory(AlbumDestinationDir + "\\" + album.SNID);
                    }

                    //System.Windows.Forms.MessageBox.Show("sending req for photos in album " + album.SNID + " ID: " + album.ID );
                    AsyncReqQueue apiReq = FBAPI.PhotosInAlbum(album.SNID, SIZETOGETPERPAGE, ProcessPhotosInAlbum, album.ID);
                    apiReq.Queue(400);
                    apiReq = FBAPI.Likes(album.SNID, SIZETOGETPERPAGE, ProcessLikes, album.ID);
                    apiReq.Queue(400);
                }

                if (albums.CurrentNumber > 0 && albums.Next != null)
                {
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBAlbums", albums.Next, SIZETOGETPERPAGE, ProcessAlbums);
                    apiReq.Queue(minPriorityGlobal);
                }
            }
            return GenericProcess(hwnd, result, response, albums, false);
        }

        /// <summary>
        /// Process list of events
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of events</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessEvents(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            string errorData = "";

            if (result)
            {
                FBCollection events = new FBCollection(response, "FBEvent", parent, parentSNID);
                nInParseRequests++;
                events.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                nInSaveRequests++;
                events.Save(out errorData);
                nInSaveRequests--;

                foreach (FBEvent anEvent in events.items)
                {

                    AsyncReqQueue apiReq;
                    apiReq = FBAPI.Event(anEvent.SNID, ProcessOneEvent);
                    apiReq.Queue(400);
                }

                if (errorData == "") return true;
                // corrected bug: return an error without mark as failed
                return false;
            }
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process list of friendlists
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of friendlist IDs/titles</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessFriendLists(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            CountPerState[SENT]--;
            string errorData = "";

            if (result)
            {
                FBCollection lists = new FBCollection(response, "FBFriendList", parent, parentSNID);
                nInParseRequests++;
                lists.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                nInSaveRequests++;
                lists.Save(out errorData);
                nInSaveRequests--;

                foreach (FBFriendList list in lists.items)
                {
                    AsyncReqQueue apiReq = FBAPI.Members(list.SNID, SIZETOGETPERPAGE, ProcessList, list.ID);
                    apiReq.Queue(400);
                }

                if (errorData == "") return true;
                // corrected bug: return an error without mark as failed
                return false; 
            }
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process list of friends
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of notes</param>
        /// <param name="parent">CHECK: Reference to the friendlist ID</param>
        /// <param name="parentSNID">CHECK: Reference to the friendlist SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessList(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            CountPerState[SENT]--;
            // TODO: Consolidate with ProcessFriends, maybe with an empty name or with the default name All Friends
            string errorData = "";
            if (result)
            {
                FBCollection friends = new FBCollection(response, "FBPerson", parent, parentSNID);
                nInParseRequests++;                
                friends.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                nInSaveRequests++;
                friends.Save(out errorData);
                nInSaveRequests--;
                // save the list and association
                if (errorData == "") return true;
                // corrected bug: return an error without mark as failed
                return false;
            }
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process list of family
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON array of notes</param>
        /// <param name="parent">CHECK: Reference to the person ID</param>
        /// <param name="parentSNID">CHECK: Reference to the person SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessFamily(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            CountPerState[SENT]--;
            // TODO: Consolidate with ProcessFriends, maybe with an empty name or with the default name All Friends
            string errorData = "";

            if (result)
            {
                // should be null by default
                FBCollection family = new FBCollection(response, "FBRelative", parent, parentSNID);
                nInParseRequests++;                
                family.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                nInSaveRequests++;
                family.Save(out errorData);
                nInSaveRequests--;
                // TODO: Check if there are more data to process
                if (errorData == "") return true;
                // corrected bug: return an error without mark as failed
                return false;
            }
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process user Notifications
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>        
        public static bool ProcessNotifications(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            CountPerState[SENT]--;
            string errorData = "";

            if (result)
            {
                FBCollection notifications = new FBCollection(response, "FBNotification", parent, parentSNID);
                nInParseRequests++;                
                notifications.Parse();
                nInParseRequests--;
                
                CountPerState[PARSED]++;
                nInSaveRequests++;
                notifications.Save(out errorData);
                nInSaveRequests--;
                if (errorData == "") return true;
                // corrected bug: return an error without mark as failed
                return false;
            }
            nFailedRequests++;
            return false;
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
            nRequestsInTransit--;
            FBCollection photos = null;
            if (result)
            {
                photos = new FBCollection(response, "FBPhoto", parent, parentSNID);
                nInParseRequests++;
                photos.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                CountPerState[RECEIVED]--;
                nPhotos += photos.CurrentNumber;

                string error;
                nInSaveRequests++;
                photos.Save(out error);
                nInSaveRequests--;
                if (error != "")
                {
                    //System.Windows.Forms.MessageBox.Show("Error saving photos " + error);
                    return false;
                }

                foreach (FBPhoto photo in photos.items)
                {
                    AsyncReqQueue apiReq = FBAPI.DownloadPhoto(photo.Source,
                        AlbumDestinationDir + parentSNID + "\\" + photo.SNID + ".jpg",
                        ProcessPhoto, photo.ID, photo.SNID);
                    apiReq.Queue(200);
                    apiReq = FBAPI.Likes(photo.SNID, SIZETOGETPERPAGE, ProcessLikes, photo.ID);
                    apiReq.Queue(150);
                }

                // Next is not null, but it should since we get all accepted photos on the first round... 
                // TODO: Analyze how to use Next appropriately
                /*
                if (photos.Next != null)
                {
                    //System.Windows.Forms.MessageBox.Show("Photos next: " + photos.Next);
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBPhotos", photos.Next, SIZETOGETPERPAGE, ProcessPhotosInAlbum);
                    apiReq.Queue(50);
                }
                */
            }
            return GenericProcess(hwnd, result, response, photos, false);
        }

        /// <summary>
        /// Process a list of people associated with an action - TODO: Document and name better
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="verb">Action to be associated</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessActions(int hwnd, bool result, string response, int verb, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            CountPerState[SENT]--;
            string errorData = "";

            if (result)
            {
                CountPerState[RECEIVED]++;
                if (parent != null)
                {
                    // create FBLikes object
                    FBLikes likes = new FBLikes(response, parent, parentSNID);
                    likes.Action = verb;
                    nInParseRequests++;
                    likes.Parse();
                    nInParseRequests--;
                    CountPerState[PARSED]++;
                    nInSaveRequests++;
                    likes.Save(out errorData);
                    nInSaveRequests--;
                    if (errorData == "") return true;
                    // corrected bug: return an error without mark as failed
                    return false; 
                }
            }
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process likes
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the object ID that is liked</param>
        /// <param name="parentSNID">CHECK: Reference to the object SNID</param>
        public static bool ProcessLikes(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            return ProcessActions(hwnd, result, response, Verb.LIKE, parent, parentSNID);
        }
        #region "Processing event Attendance"
        /// <summary>
        /// Process Who is Attending Event
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the event ID</param>
        /// <param name="parentSNID">CHECK: Reference to the event SNID</param>
        public static bool ProcessAttending(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            return ProcessActions(hwnd, result, response, Verb.ATTENDING, parent, parentSNID);
        }
        /// <summary>
        /// Process Who is Maybe Attending Event
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the event ID</param>
        /// <param name="parentSNID">CHECK: Reference to the event SNID</param>
        public static bool ProcessMaybeAttending(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            return ProcessActions(hwnd, result, response, Verb.MAYBEATTENDING, parent, parentSNID);
        }
        /// <summary>
        /// Process Who is Not Attending Event
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the event ID</param>
        /// <param name="parentSNID">CHECK: Reference to the event SNID</param>
        public static bool ProcessNotAttending(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            return ProcessActions(hwnd, result, response, Verb.NOTATTENDING, parent, parentSNID);
        }
        #endregion

        #endregion

        #region "Single item processing methods"
        /// <summary>
        /// process one friend profile
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessOneFriend(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            CountPerState[SENT]--;
            if (result)
            {
                nFriendsProcessed++;
                FBPerson currentFriend = new FBPerson(response, 1, null);
                nInParseRequests++;
                currentFriend.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;

                string errorData;
                nInSaveRequests++;
                currentFriend.Save(out errorData);
                nInSaveRequests--;
                if (errorData != "")
                {
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
        /// Process details of a single event
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON event data</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessOneEvent(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            CountPerState[SENT]--;
            string errorData = "";

            if (result)
            {
                FBEvent anEvent = new FBEvent(response);
                nInParseRequests++;
                anEvent.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                nInSaveRequests++;
                anEvent.Save(out errorData);
                nInSaveRequests--;

                if (anEvent.OwnerID == SNAccount.CurrentProfile.SNID)
                {
                    AsyncReqQueue apiReq;
                    apiReq = FBAPI.AttendingEvent(anEvent.SNID, SIZETOGETPERPAGE, ProcessAttending, anEvent.ID);
                    apiReq.Queue(200);
                    apiReq = FBAPI.MaybeEvent(anEvent.SNID, SIZETOGETPERPAGE, ProcessMaybeAttending, anEvent.ID);
                    apiReq.Queue(200);
                    apiReq = FBAPI.DeclinedEvent(anEvent.SNID, SIZETOGETPERPAGE, ProcessNotAttending, anEvent.ID);
                    apiReq.Queue(200);
                }

                if (errorData == "") return true;
                // corrected bug: return an error without mark as failed
                return false;
            }
            nFailedRequests++;
            return false;
        }

        /// <summary>
        /// Process details of a single post
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON post data</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public static bool ProcessOnePost(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            nRequestsInTransit--;
            string errorData = "";

            if (result)
            {
                FBPost aPost = new FBPost(response);
                nInParseRequests++;
                aPost.Parse();
                nInParseRequests--;
                nInSaveRequests++;
                aPost.Save(out errorData);
                nInSaveRequests--;

                if (aPost.LikesCount > 0)
                {
                    AsyncReqQueue apiReq;
                    apiReq = FBAPI.Likes(aPost.SNID, SIZETOGETPERPAGE, ProcessLikes, aPost.ID);
                    apiReq.Queue(200);
                }

                if (errorData == "")
                {                    
                    CountPerState[PARSED]++;
                    return true;
                }
                // corrected bug: return an error without mark as failed
                return false; 
            }
            nFailedRequests++;
            return false;
        }

        #endregion

        #endregion

    }
}
