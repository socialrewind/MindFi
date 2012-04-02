using System;
using System.Collections;
using System.Data.SQLite;
using System.ComponentModel;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that contains methods to manage the asynchronous request queue
    /// </summary>

    public class AsyncReqQueue
    {
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
        public DateTime startDate;
        public DateTime endDate;

        public static string ProfilePhotoDestinationDir;
        public static string AlbumDestinationDir;
        public static bool GotOneProfilePic = false;
        public static bool GotFriendList = false;
        public static bool GotWall = false;
        public static bool GotInbox = false;
        public static bool GotEvent = false;
        public static bool GotFriendLists = false;
        public static bool FailedRequests = false;

        public string ErrorMessage;
        public static volatile int CurrentBackupState = 0;

        #region "Controlling what to backup"
        // TODO: REVIEW before closing beta, useful now for testing
        // Review CurrentBackupState about line 615 every time these are changed, also before CurrentBackupState is incremented about line 1100+
        // TODO: Remove from here, use the one from the Current Account (also to manage distinct social networks separately)
        public static bool BackupMyWall;
        public static bool BackupMyNews;
        public static bool BackupMyInbox;
        public static bool BackupMyEvents;
        public static bool BackupMyNotifications = false;
        public static bool BackupFriendsInfo = false;
        public static bool BackupFriendsFamily = false;
        public static bool BackupFriendsPic;
        public static bool BackupMyAlbums;
        public static bool BackupMyPhotos;
        public static bool BackupFriendsWall;
        public static bool BackupFriendsEvents;
        public static bool BackupFriendsAlbums;

        #endregion

        #endregion

        #region "Constants"
        public const int SIZETOGETPERPAGE = 200; // USE 1 for debugging issues that are not related to concurrency
        public const int CONCURRENTREQUESTLIMIT = 20;
        // AsyncReqQueue states
        public const int POTENTIAL = 0;
        public const int QUEUED = 1;
        public const int SENT = 2;
        public const int RETRY = 3;
        public const int RECEIVED = 4;
        public const int PARSED = 5;
        public const int PROCESSED = 6;
        public const int TODELETE = 7;
        public const int FAILED = 8;
        // Backup process states: Review CurrentBackupState about line 615 every time these are changed
        public const int NEWBACKUP = 0;
        public const int BACKUPMYWALL = 1;
        public const int BACKUPMYNEWS = 2;
        public const int BACKUPMYINBOX = 3;
        public const int BACKUPMYNOTIFICATIONS = 4;
        public const int BACKUPMYEVENTS = 5;
        public const int BACKUPFRIENDSINFO = 6;
        public const int BACKUPFRIENDSPROFPIC = 7;
        public const int BACKUPMYALBUMS = 8;
        public const int BACKUPMYPHOTOS = 9;
        public const int BACKUPALBUMCOVER = 10;
        public const int BACKUPFRIENDSWALLS = 11;
        public const int BACKUPFRIENDSEVENTS = 12;
        public const int BACKUPFRIENDSALBUMS = 13;
        #endregion

        #region "Statistics"
        public static volatile int nFriends = 0;
        public static volatile int nFriendsProcessed = 0;
        public static volatile int nFriendsPictures = 0;
        public static volatile int nFriendsWalls = 0;
        public static volatile int nFriendsEvents = 0;
        public static volatile int nFriendsAlbums = 0;
        public static volatile int nPosts = 0;
        public static volatile int nEvents = 0;
        public static volatile int nMessages = 0;
        public static volatile int nAlbums = 0;
        public static volatile int nPhotos = 0;
        public static volatile int nNotifications = 0;

        private static volatile int nNotParsedRequests = 0;
        private static volatile int nInParseRequests = 0;
        private static volatile int nInSaveRequests = 0;
        private static volatile int nFailedRequests = 0;
        private static volatile int[] CountPerState = null;
        private static volatile int minPriorityGlobal = 0;
        private static volatile int currentPriorityGlobal = 999;
        private static volatile int currentBackupNumber = 0;
        //private static volatile int nRequestsInTransit = 0;
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
        private static bool FirstTimeBasics = true;

        private static volatile int NextID = -1;
        private bool addToken = true;
        private bool addDateRange = false;
        private bool Saved = false;
        private string PostData = "";
        private static volatile Object obj = new Object();
        private static bool DatabaseInUse = false;
        private static bool newPeriod = false;
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
        public AsyncReqQueue(string type, string URL, int limit, CallBack resultCall, bool AddToken = true, bool AddDateRange = false, long? parentID = null, string parentSNID = "", string postData = "")
        {
            lock (obj)
            {
                DatabaseInUse = true;
                if (NextID == -1)
                {
                    // read from database
                    string errorMessage = "";
                    NextID = DBLayer.AvailableReqQueueSave(out errorMessage);
                    if (NextID == -1) NextID = 0;
                }
                ID = NextID++;
                DatabaseInUse = false;
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
            // TODO: Check defaults for dates
            startDate = ((SNAccount.CurrentProfile == null) ? DateTime.Now.AddMonths(-1) : SNAccount.CurrentProfile.CurrentPeriodStart);
            endDate = ((SNAccount.CurrentProfile == null) ? DateTime.Now.AddMonths(1) : SNAccount.CurrentProfile.CurrentPeriodEnd);
            PostData = postData;
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
                DatabaseInUse = true;
                if (NextID == -1)
                {
                    // read from database
                    string errorMessage = "";
                    NextID = DBLayer.AvailableReqQueueSave(out errorMessage);
                    if (NextID == -1) NextID = 0;
                }
                ID = NextID++;
                DatabaseInUse = false;
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
            // TODO: Check defaults for dates
            startDate = ((SNAccount.CurrentProfile == null) ? DateTime.Now.AddMonths(-1) : SNAccount.CurrentProfile.CurrentPeriodStart);
            endDate = ((SNAccount.CurrentProfile == null) ? DateTime.Now.AddMonths(1) : SNAccount.CurrentProfile.CurrentPeriodEnd);
            InitArray();
            Save();
        }

        /// <summary>
        /// Constructor that creates an AsyncReqQueue object from the RequestsQueue table in the database
        /// </summary>
        /// <param name="id">Identifier in the database to query the request</param>
        public AsyncReqQueue(int id)
        {
            string errorMessage;
            // Call layer to get data
            InitArray();
            if (DBLayer.GetAsyncReq(id, out ReqType, out Priority, out Parent, out ParentSNID,
                out Created, out Updated, 
                out startDate, out endDate,
                out ReqURL, out State, out FileName, out addToken, out addDateRange, out PostData,
                out errorMessage))
            {
                ID = id;
                SetCallBackFunction();
                Saved = true; // to make sure it asks for update later
                ErrorMessage = errorMessage;
                return;
            }
            else
            {
                ID = -1;
                ErrorMessage = errorMessage;
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
                case "FBNews":
                    methodToCall = ProcessNews;
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
                case "PostStatus":
                    methodToCall = ProcessStatus;
                    break;
                case "PostComment":
                    methodToCall = ProcessStatus;
                    break;
                case "PostLike":
                    methodToCall = ProcessNull;
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
                Save();
                return true;
            }
            //System.Windows.Forms.MessageBox.Show("ERROR: AsyncReq ID " + ID + " Not in the appropriate state for being Queued");
            return false;
        }

        /// <summary>
        /// Variant that specifies priority for the request and decides accordingly if it should be queued or not
        /// </summary>
        /// <param name="priority">Higher (using 999), the first to be consumed from the queue</param>
        /// <returns></returns>
        public bool Queue(int priority)
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
            if (FBLogin.loggedIn)
            {
                if (State == QUEUED || State == RETRY)
                {
                    lock (obj)
                    {
                        CountPerState[SENT]++;
                        CountPerState[QUEUED]--;
                        //nRequestsInTransit++;
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
                    switch ( ReqType )
                    {
                            // review state, should not be resending...
                        case "PostStatus":
                        case "PostComment":
                        case "PostLike":
                            result = FBAPI.CallGraphAPIPost(ReqURL, Limit, methodToCall, PostData, 0, Parent, ParentSNID, true, false);
                            break;
                        default:
                            FBAPI.SetTimeRange(startDate, endDate);
                            result = FBAPI.CallGraphAPI(ReqURL, Limit, methodToCall, ID, Parent, ParentSNID, addToken, addDateRange);
                            break;
                    }
                }
                else
                {
                    result = FBAPI.CallGraphAPI(ReqURL, FileName, methodToCall, ID, Parent, ParentSNID);
                }
                return result;
            }
            else
            {
                return false; // cannot send when disconnected or not loggedin
            }
        }

        /// <summary>
        /// Variant that combines Queue and Send, specifies priority for the request and decides accordingly if it should be queued and send or not
        /// </summary>
        /// <param name="priority">Higher (using 999), the first to be consumed from the queue</param>
        /// <returns></returns>
        public bool QueueAndSend(int priority)
        {
            bool success = true;
            Priority = priority;
            if (priority >= minPriorityGlobal)
            {
                success = Queue();
                if (success)
                {
                    // performance improvement: don't mark as failed when it was successfully queued, since it can be just disconnected - prevents double requests
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
            Saved = DBLayer.ReqQueueSave(ID, ReqType, Priority, Parent, ParentSNID, Created, Updated, ReqURL, State, FileName, addToken, addDateRange, PostData, 
                startDate, endDate, Saved, out ErrorMessage);
            if (!Saved)
            {
                // TODO: Error management
                //System.Windows.Forms.MessageBox.Show("Failed to save request: " + ID + " error: " + ErrorMessage);
            }
        }

        #endregion

        #region "Static externally used methods"

        #region "Basic data methods"
        public static void GetBasicData(DateTime currentPeriodStart, DateTime currentPeriodEnd, int ID, string SNID)
        {
            // clean information
            nFriends = 0;
            nPosts = 0;           
            FailedRequests = false;
            GotOneProfilePic = false;
            GotFriendList = false;
            GotWall = false;
            GotInbox = false;
            GotEvent = false;
            GotFriendLists = false;
            minPriorityGlobal = 999; // don't go on children requests

            FBAPI.SetTimeRange(currentPeriodStart, currentPeriodEnd);
            AsyncReqQueue apiReq;
            apiReq = FBAPI.ProfilePic(SNID,
                ProfilePhotoDestinationDir + SNID + ".jpg",
                ProcessFriendPic, ID, SNID);
            apiReq.QueueAndSend(999);
            string errorMessage;
            DBLayer.UpdatePictureRequest(ID, apiReq.ID, out errorMessage);
            apiReq = FBAPI.Friends("me", SIZETOGETPERPAGE, ProcessFriends, ID, SNID);
            apiReq.QueueAndSend(999);
            apiReq = FBAPI.FriendLists("me", SIZETOGETPERPAGE, ProcessFriendLists, FBLogin.Me.ID);
            apiReq.QueueAndSend(999);
            // TODO: FriendLists request ID

            if (BackupMyWall)
            {
                // Data that could use GetDataForCurrentState
                apiReq = FBAPI.Wall("me", SIZETOGETPERPAGE, ProcessWall, ID, SNID);
                apiReq.QueueAndSend(999);
                DBLayer.UpdateWallRequest(ID, apiReq.ID, out errorMessage);
            }
            if (BackupMyNews)
            {
                apiReq = FBAPI.News(SIZETOGETPERPAGE, ProcessNews, ID, SNID);
                apiReq.QueueAndSend(999);
                DBLayer.UpdateNewsRequest(ID, apiReq.ID, out errorMessage);
            }
            if (BackupMyInbox)
            {
                apiReq = FBAPI.Inbox("me", SIZETOGETPERPAGE, ProcessInbox);
                apiReq.QueueAndSend(999);
                DBLayer.UpdateInboxRequest(ID, apiReq.ID, out errorMessage);
            }
            if (BackupMyEvents)
            {
                // TODO: make sure "me" is always 1 and CurrentProfile is applicable
                apiReq = FBAPI.Events("me", SIZETOGETPERPAGE, ProcessEvents, 1, SNID);
                apiReq.QueueAndSend(999);
                DBLayer.UpdateEventRequest(ID, apiReq.ID, out errorMessage);
            }
            apiReq = FBAPI.Family(SNID, SIZETOGETPERPAGE, ProcessFamily);
            apiReq.Queue(400);
            DBLayer.UpdateFamilyRequest(ID, apiReq.ID, out errorMessage);
            if (BackupMyNews)
            {
                apiReq = FBAPI.Notifications("me", SIZETOGETPERPAGE, ProcessNotifications);
                apiReq.QueueAndSend(999);
            }
        }


        public static string PendingBasics()
        {
            string errorMessage ="";

            if (!DatabaseInUse)
            {
                ArrayList queueReq = DBLayer.GetRequests(CONCURRENTREQUESTLIMIT - FBAPI.InFlight, AsyncReqQueue.QUEUED, out errorMessage, minPriorityGlobal);
                if (queueReq.Count == 0 && FBAPI.InFlight==0 && !FirstTimeBasics )
                {
                    FailedRequests = true;
                }
                else
                {
                    foreach (int reqID in queueReq)
                    {
                        AsyncReqQueue apiReq = new AsyncReqQueue(reqID);
                        // special case: ID is not the requested, the request is not there any longer?
                        if (apiReq.ID != -1)
                        {
                            errorMessage += "ID: " + apiReq.ID + " Type: " + apiReq.ReqType + " Pri: " + apiReq.Priority + " URL: " + apiReq.ReqURL + "\n";
                            // DEBUG to confirm validation is redundant
                            //if (apiReq.Priority < minPriorityGlobal) MessageBox.Show("Unexpected low priority request, ID: " + apiReq.ID + " Type: " + apiReq.ReqType + " Pri: " + apiReq.Priority + " URL: " + apiReq.ReqURL + "\n");

                            apiReq.Send();
                        } // if
                    } // foreach
                    FirstTimeBasics = false;
                } // if queueReq.Count is 0
            } // if Database not in use
            return errorMessage;
        }

        #endregion

        /// <summary>
        /// Create the first, basic requests for a new backup
        /// </summary>
        public static void NewRequests(int MinPriority, int ID, string SNID)
        {
            minPriorityGlobal = MinPriority;
            string error;
            int[] stateArray;
            int nFriendsReceived, nPicsReceived;

            if (DBLayer.GetBackupStatistics(out nFriendsReceived, out nPicsReceived, out error))
            {
                nFriendsProcessed = nFriendsReceived;
                nFriendsPictures = nPicsReceived;
            }
                            
            if (DBLayer.GetNRequestsPerState(minPriorityGlobal, out stateArray, out error))
            {
                CountPerState = stateArray;
                // Update old requests for retry
                if (!DBLayer.QueueRetryUpdate(minPriorityGlobal))
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
                int currentBackup, currentState;
                DateTime currentPeriodStart, currentPeriodEnd, currentBackupStart, currentBackupEnd;
                bool isIncremental;

                DBLayer.StartBackup(SNAccount.CurrentProfile.BackupPeriodStart, SNAccount.CurrentProfile.BackupPeriodEnd,
                        out currentBackupStart, out currentBackupEnd,
                        out currentBackup, out currentPeriodStart, out currentPeriodEnd, 
                        out isIncremental, out currentState);
                SNAccount.CurrentProfile.CurrentPeriodStart = currentPeriodStart;
                SNAccount.CurrentProfile.CurrentPeriodEnd = currentPeriodEnd;
                SNAccount.CurrentProfile.BackupPeriodStart = currentBackupStart;
                SNAccount.CurrentProfile.BackupPeriodEnd = currentBackupEnd;
                FBAPI.SetTimeRange(SNAccount.CurrentProfile.CurrentPeriodStart, SNAccount.CurrentProfile.CurrentPeriodEnd);
                CurrentBackupState = currentState;
                currentBackupNumber = currentBackup;
                newPeriod = true;
                // Check: initial state, TODO improve / generalize
                CurrentBackupState = BACKUPMYWALL;

                if (CountPerState[QUEUED] + CountPerState[SENT] + CountPerState[RETRY] > 0)
                {
                    PendingRequests(MinPriority, ID, SNID, out error);
                }
                else
                {
                    if (currentBackup != 0)
                    {
                        GetDataForCurrentState(ID, SNID);
                    }
                    else
                    {
                        // TODO: Localize, return to interface
                        error = "Backup couldn't be created";
                    }
                }
            }
        }


        private static int GetDataForCurrentState(int ID, string SNID)
        {
            int nReqs = 0;
            AsyncReqQueue apiReq;
            switch (CurrentBackupState)
            {
                case BACKUPMYWALL:
                    if (BackupMyWall)
                    {
                        if (newPeriod)
                        {
                            apiReq = FBAPI.Wall("me", SIZETOGETPERPAGE, ProcessWall, ID, SNID);
                            apiReq.QueueAndSend(999);
                            nReqs = 1;
                            newPeriod = false;
                        }
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPMYNEWS:
                    if (BackupMyNews)
                    {
                        if (newPeriod)
                        {
                            apiReq = FBAPI.News(SIZETOGETPERPAGE, ProcessNews, ID, SNID);
                            apiReq.QueueAndSend(999);
                            nReqs = 1;
                            newPeriod = false;
                        }
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPMYINBOX:
                    if (BackupMyInbox)
                    {
                        nReqs = GetNextMessageData();
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPMYEVENTS:
                    if (BackupMyEvents)
                    {
                        nReqs = GetNextEventsData();
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPMYNOTIFICATIONS:
                    if (BackupMyNotifications)
                    {
                        if (newPeriod)
                        {
                            apiReq = FBAPI.Notifications("me", SIZETOGETPERPAGE, ProcessNotifications);
                            apiReq.QueueAndSend(500);
                            newPeriod = false;
                        }
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPFRIENDSINFO:
                    if (BackupFriendsInfo)
                    {
                        nReqs = GetNextFriendsData();
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPFRIENDSPROFPIC:
                    if (BackupFriendsPic)
                    {
                        nReqs = GetNextFriendsPics();
                    }
                    else
                    {
                        nReqs = 0;
                    }

                    break;
                case BACKUPMYALBUMS:
                    if (BackupMyAlbums)
                    {
                        if (newPeriod)
                        {
                            apiReq = FBAPI.PhotoAlbums("me", SIZETOGETPERPAGE, ProcessAlbums);
                            apiReq.QueueAndSend(500);
                            newPeriod = false;
                        }
                    }
                    else
                    {
                        nReqs = 0;
                    }

                    break;
                case BACKUPMYPHOTOS:
                    if (BackupMyPhotos)
                    {
                        nReqs = GetNextAlbumPics();
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPALBUMCOVER:
                    if (BackupMyAlbums || BackupFriendsAlbums)
                    {
                        nReqs = GetPendingAlbumCoverPics();
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPFRIENDSWALLS:
                    if (BackupFriendsWall)
                    {
                        if (newPeriod)
                        {
                            nReqs = GetNextFriendsWalls();
                        }
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPFRIENDSEVENTS:
                    if (BackupFriendsEvents)
                    {
                        if (newPeriod)
                        {
                            nReqs = GetNextFriendsEvents();
                        }
                        if (nReqs == 0)
                        {
                            nReqs = GetNextEventsDetail();
                        }
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                case BACKUPFRIENDSALBUMS:
                    if (BackupFriendsAlbums)
                    {
                        if (newPeriod)
                        {
                            nReqs = GetNextFriendsAlbums();
                        }
                    }
                    else
                    {
                        nReqs = 0;
                    }
                    break;
                default:
                    nReqs = 0;
                    break;
            }
            return nReqs;
        }

        #region "Methods to get missing data"

        private static int GetNextFriendsPics()
        {
            string errorMessage;
            // Go over friends, adding requests for them if not yet defined
            int[] FriendEntityID;
            string[] FriendSNID;

            int nRequests = DBLayer.GetNFriendsWithoutPicture(CONCURRENTREQUESTLIMIT, out FriendEntityID, out FriendSNID, out errorMessage);
            if (nRequests > 0)
            {
                int i = 0;
                foreach (int friendID in FriendEntityID)
                {
                    string SNID = FriendSNID[i];
                    if (SNID != null && SNID != "")
                    {
                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_GetFriendPic);
                        AsyncWorkArgs temp = new AsyncWorkArgs(friendID, SNID);
                        bw.RunWorkerAsync(temp);
                    }
                    i++;
                }
            }
            return nRequests;
        }

        private static int GetNextFriendsData()
        {
            string errorMessage;
            // Go over friends, adding requests for them if not yet defined
            int[] FriendEntityID;
            string[] FriendSNID;

            int nRequests = DBLayer.GetNFriendsWithoutData(CONCURRENTREQUESTLIMIT, out FriendEntityID, out FriendSNID, out errorMessage);
            if (nRequests > 0)
            {
                int i = 0;
                foreach (int friendID in FriendEntityID)
                {
                    string SNID = FriendSNID[i];

                    if (SNID != null && SNID != "")
                    {
                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_GetFriendData);
                        AsyncWorkArgs temp = new AsyncWorkArgs(friendID, SNID);
                        bw.RunWorkerAsync(temp);
                    }
                    i++;
                }
            }
            return nRequests;
        }

        private static int GetNextEventsData()
        {
            AsyncReqQueue apiReq;
            if (newPeriod)
            {
                // TODO: make sure "me" is always 1 and CurrentProfile is applicable
                apiReq = FBAPI.Events("me", SIZETOGETPERPAGE, ProcessEvents, 1, SNAccount.CurrentProfile.SNID );
                apiReq.QueueAndSend(500);
                newPeriod = false;
                return 1;
            }
            int nRequests = GetNextEventsDetail();
            return nRequests;
        }

        private static int GetNextEventsDetail()
        {
            // Find next
            string errorMessage;
            // Go over friends, adding requests for them if not yet defined
            int[] EventEntityID;
            string[] EventSNID;

            int nRequests = DBLayer.GetNEventsWithoutDetail(CONCURRENTREQUESTLIMIT, out EventEntityID, out EventSNID, out errorMessage);
            if (nRequests > 0)
            {
                int i = 0;
                foreach (int eventID in EventEntityID)
                {
                    string SNID = EventSNID[i];
                    if ( SNID != null )
                    {
                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_GetEvent);
                        AsyncWorkArgs temp = new AsyncWorkArgs(eventID, SNID);
                        bw.RunWorkerAsync(temp);
                    }
                    i++;
                }
            }
            return nRequests;
        }

        private static int GetNextMessageData()
        {
            AsyncReqQueue apiReq;
            if (newPeriod)
            {
                apiReq = FBAPI.Inbox("me", SIZETOGETPERPAGE, ProcessInbox);
                apiReq.QueueAndSend(999);
                //apiReq = FBAPI.Notes("me", SIZETOGETPERPAGE, ProcessNotes);
                //apiReq.QueueAndSend(999);                        

                newPeriod = false;
                return 1;
            }

            /* TODO: Verify usage of Threads

            // Find next
            string errorMessage;
            // Go over friends, adding requests for them if not yet defined
            int[] MessageEntityID;
            string[] MessageSNID;

            int nRequests = DBLayer.GetNMessagesWithoutThread(CONCURRENTREQUESTLIMIT, out MessageEntityID, out MessageSNID, out errorMessage);
            if (nRequests > 0)
            {
                int i = 0;
                foreach (int MessageID in MessageEntityID)
                {
                    string SNID = MessageSNID[i];
                    if (SNID != null)
                    {
                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_GetMessage);
                        AsyncWorkArgs temp = new AsyncWorkArgs(MessageID, SNID);
                        bw.RunWorkerAsync(temp);
                    }
                    i++;
                }
            }
            return nRequests;
             */
            return 0;
        }

        private static int GetPendingAlbumCoverPics()
        {
            string errorMessage;
            // Go over friends, adding requests for them if not yet defined
            string[] PhotoSource;
            int[] PhotoEntityID;
            string[] PhotoSNID;
            string[] AlbumSNID;

            int nRequests = DBLayer.GetCoverPicturesToDownload(CONCURRENTREQUESTLIMIT, out PhotoSource, out AlbumSNID, out PhotoEntityID, out PhotoSNID, out errorMessage);
            if (nRequests > 0)
            {
                int i = 0;
                foreach (int photoID in PhotoEntityID)
                {
                    string source = PhotoSource[i];
                    string parent = AlbumSNID[i];
                    string SNID = PhotoSNID[i];
                    if (source != null && SNID != null && source != "" && SNID != "")
                    {
                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_GetAlbumPic);
                        AsyncWorkArgs temp = new AsyncWorkArgs(photoID, SNID, source, parent);
                        bw.RunWorkerAsync(temp);
                    }
                    i++;
                }
            }
            return nRequests;
        }

        private static int GetNextAlbumPics()
        {
            string errorMessage;
            // Go over friends, adding requests for them if not yet defined
            string[] PhotoSource;
            int[] PhotoEntityID;
            string[] PhotoSNID;
            string[] AlbumSNID;

            int nRequests = DBLayer.GetNPhotosToDownload(CONCURRENTREQUESTLIMIT, out PhotoSource, out AlbumSNID, out PhotoEntityID, out PhotoSNID, out errorMessage);
            if (nRequests > 0)
            {
                int i = 0;
                foreach (int photoID in PhotoEntityID)
                {
                    string source = PhotoSource[i];
                    string parent = AlbumSNID[i];
                    string SNID = PhotoSNID[i];
                    if (source!=null && SNID != null && source!="" && SNID != "")
                    {
                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_GetAlbumPic);
                        AsyncWorkArgs temp = new AsyncWorkArgs(photoID, SNID, source, parent);
                        bw.RunWorkerAsync(temp);
                    }
                    i++;
                }
            }
            return nRequests;
        }

        private static int GetNextFriendsWalls()
        {
            string errorMessage;
            // Go over friends, adding requests for them if not yet defined
            int[] FriendEntityID;
            string[] FriendSNID;

            // Optimization: only get one friend wall instead of CONCURRENTREQUESTLIMIT
            int nRequests = DBLayer.GetNFriendsWithoutWall(1, out FriendEntityID, out FriendSNID, out errorMessage);
            if (nRequests > 0)
            {
                int i = 0;
                // TODO: Improve logic to manage time - depends on the friend...
                FBAPI.SetTimeRange(SNAccount.CurrentProfile.BackupPeriodStart, SNAccount.CurrentProfile.BackupPeriodEnd);
                foreach (int friendID in FriendEntityID)
                {
                    string SNID = FriendSNID[i];
                    if (SNID != null && SNID != "")
                    {
                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_GetFriendWall);
                        AsyncWorkArgs temp = new AsyncWorkArgs(friendID, SNID);
                        bw.RunWorkerAsync(temp);
                    }
                    i++;
                }
            }
            return nRequests;
        }

        private static int GetNextFriendsEvents()
        {
            string errorMessage;
            AsyncReqQueue apiReq;
            // Go over friends, adding requests for them if not yet defined
            int[] FriendEntityID;
            string[] FriendSNID;

            // Optimization: only get one friend wall instead of CONCURRENTREQUESTLIMIT
            int nRequests = DBLayer.GetNFriendsWithoutEvents(1, out FriendEntityID, out FriendSNID, out errorMessage);
            if (nRequests > 0)
            {
                int i = 0;
                foreach (int friendID in FriendEntityID)
                {
                    string SNID = FriendSNID[i];
                    if (SNID != null && SNID != "")
                    {
                        // TODO: Improve logic to manage time
                        // TODO: Make async
                        FBAPI.SetTimeRange(SNAccount.CurrentProfile.BackupPeriodStart, SNAccount.CurrentProfile.BackupPeriodEnd);
                        apiReq = FBAPI.Events(SNID, SIZETOGETPERPAGE, ProcessEvents, friendID, SNID);
                        apiReq.QueueAndSend(999);
                        DBLayer.UpdateEventRequest(friendID, apiReq.ID, out errorMessage);
                        nFriendsEvents++;
                    }
                    i++;
                }
            }
            return nRequests;
        }

        private static int GetNextFriendsAlbums()
        {
            string errorMessage;
            AsyncReqQueue apiReq;
            // Go over friends, adding requests for them if not yet defined
            int[] FriendEntityID;
            string[] FriendSNID;

            // Optimization: only get one by one friend album list instead of CONCURRENTREQUESTLIMIT
            int nRequests = DBLayer.GetNFriendsWithoutEvents(1, out FriendEntityID, out FriendSNID, out errorMessage);
            if (nRequests > 0)
            {
                int i = 0;
                foreach (int friendID in FriendEntityID)
                {
                    string SNID = FriendSNID[i];
                    if (SNID != null && SNID != "")
                    {
                        // TODO: Improve logic to manage time
                        // TODO: Make async
                        FBAPI.SetTimeRange(SNAccount.CurrentProfile.BackupPeriodStart, SNAccount.CurrentProfile.BackupPeriodEnd);
                        apiReq = FBAPI.PhotoAlbums(SNID, SIZETOGETPERPAGE, ProcessAlbums);
                        apiReq.QueueAndSend(999);
                        // TODO: Album requests
                        // DBLayer.UpdateAlbumRequest(friendID, apiReq.ID, out errorMessage);
                        nFriendsAlbums++;
                    }
                    i++;
                }
            }
            return nRequests;
        }

        #endregion

        #region "Background processing of methods to get missing data"

        private static void bw_GetFriendData(object sender, DoWorkEventArgs e)
        {
            string errorMessage;
            BackgroundWorker worker = sender as BackgroundWorker;
            AsyncWorkArgs temp = (AsyncWorkArgs)e.Argument;

            AsyncReqQueue apiReq = FBAPI.Profile(temp.SNID, ProcessOneFriend);
            apiReq.QueueAndSend(999);
            DBLayer.UpdateDataRequest(temp.ID, apiReq.ID, out errorMessage);
            // TODO: Separate process for missing family vs. missing data?
            apiReq = FBAPI.Family(temp.SNID, SIZETOGETPERPAGE, ProcessFamily);
            apiReq.Queue(400);
            DBLayer.UpdateFamilyRequest(temp.ID, apiReq.ID, out errorMessage);
        }

        private static void bw_GetFriendPic(object sender, DoWorkEventArgs e)
        {
            string errorMessage;
            BackgroundWorker worker = sender as BackgroundWorker;
            AsyncWorkArgs temp = (AsyncWorkArgs)e.Argument;

            AsyncReqQueue apiReq = FBAPI.ProfilePic(temp.SNID,
                            ProfilePhotoDestinationDir + temp.SNID + ".jpg",
                            ProcessFriendPic, temp.ID, temp.SNID);
            apiReq.QueueAndSend(999);
            DBLayer.UpdatePictureRequest(temp.ID, apiReq.ID, out errorMessage);
        }

        private static void bw_GetAlbumPic(object sender, DoWorkEventArgs e)
        {
            //string errorMessage;
            BackgroundWorker worker = sender as BackgroundWorker;
            AsyncWorkArgs temp = (AsyncWorkArgs)e.Argument;

            AsyncReqQueue apiReq = FBAPI.DownloadPhoto(temp.Source,
                AlbumDestinationDir + temp.parentSNID + "\\" + temp.SNID + ".jpg",
                ProcessPhoto, temp.ID, temp.SNID);
            apiReq.QueueAndSend(200);
            apiReq = FBAPI.Likes(temp.SNID, SIZETOGETPERPAGE, ProcessLikes, temp.ID);
            apiReq.QueueAndSend(150);
            // TODO: keep request ID for photos
            //DBLayer.UpdatePictureRequest(temp.ID, apiReq.ID, out errorMessage);
        }

        private static void bw_GetEvent(object sender, DoWorkEventArgs e)
        {
            string errorMessage;
            BackgroundWorker worker = sender as BackgroundWorker;
            AsyncWorkArgs temp = (AsyncWorkArgs)e.Argument;

            AsyncReqQueue apiReq = FBAPI.Event(temp.SNID, ProcessOneEvent);
            apiReq.QueueAndSend(200);
            DBLayer.UpdateEventDetailRequest(temp.ID, apiReq.ID, out errorMessage);
        }

        private static void bw_GetMessage(object sender, DoWorkEventArgs e)
        {
            string errorMessage;
            BackgroundWorker worker = sender as BackgroundWorker;
            AsyncWorkArgs temp = (AsyncWorkArgs)e.Argument;

            AsyncReqQueue apiReq = FBAPI.Thread(temp.SNID, ProcessInbox);
            apiReq.Queue(200);
            DBLayer.UpdateThreadRequest(temp.ID, apiReq.ID, out errorMessage);
        }

        private static void bw_GetFriendWall(object sender, DoWorkEventArgs e)
        {
            string errorMessage;
            BackgroundWorker worker = sender as BackgroundWorker;
            AsyncWorkArgs temp = (AsyncWorkArgs)e.Argument;

            AsyncReqQueue apiReq = FBAPI.Wall(temp.SNID, SIZETOGETPERPAGE, ProcessWall, temp.ID, temp.SNID);
            apiReq.QueueAndSend(999);
            DBLayer.UpdateWallRequest(temp.ID, apiReq.ID, out errorMessage);
            nFriendsWalls++;
        }

        #endregion


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
        public static bool PendingRequests(int MinPriority, int ID, string SNID, out string ErrorMessage)
        {
            ErrorMessage = ""; // backupCase;

            if (DBLayer.DatabaseBusy)
            {
                return true;
            }
            #region "Init"
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
            if (FBAPI.InFlight < ConcurrentRequestLimit && nInParseRequests <= 0 && nInSaveRequests <=0 )
            {
                ArrayList queueReq = DBLayer.GetRequests(ConcurrentRequestLimit - FBAPI.InFlight, AsyncReqQueue.QUEUED, out ErrorMessage, minPriorityGlobal);
                if (ErrorMessage != "")
                {
                    ExitPendingWithError("Error getting queued requests: " + ErrorMessage);
                    return false;
                }
                ErrorMessage = "Queued Requests to send: " + queueReq.Count + "\n";
                if (queueReq.Count == 0)
                {
                    queueReq = DBLayer.GetRequests(ConcurrentRequestLimit - FBAPI.InFlight, AsyncReqQueue.RETRY, out ErrorMessage, minPriorityGlobal);
                    if (ErrorMessage != "")
                    {
                        ExitPendingWithError("Error getting retry requests: " + ErrorMessage);
                        return false;
                    }

                    ErrorMessage = "Retry Requests to send: " + queueReq.Count + "\n";
                    if (queueReq.Count == 0)
                    {
                        int nReqs = 0;
                        // first check if there are more friends to get data from                            
                        if (DBLayer.DatabaseBusy)
                        {
                            nReqs = 1;
                        }
                        else
                        {
                            nReqs = GetDataForCurrentState(ID, SNID);
                        }

                        // if no pending data, then check backup time progress and infligh requests
                        if (nReqs == 0)
                        {
                            if (CurrentBackupState < BACKUPFRIENDSALBUMS )
                                {
                                CurrentBackupState++;
                                DBLayer.UpdateBackup(currentBackupNumber, SNAccount.CurrentProfile.CurrentPeriodStart, SNAccount.CurrentProfile.CurrentPeriodEnd, CurrentBackupState);
                                nReqs = 1;
                                newPeriod = true;
                            }
                        }

                        if (nReqs == 0)
                        {
                            
                            if ((SNAccount.CurrentProfile.CurrentPeriodStart <= SNAccount.CurrentProfile.BackupPeriodStart)
                                && nInParseRequests <= 0
                                && nInSaveRequests <= 0
                                && FBAPI.InFlight <= 0
                                )
                            {
                                DBLayer.EndBackup();
                                //int backedPosts;
                                // DBLayer.BackupStatistics(out backedPosts);
                                backupInProgress = false;
                            }
                            else
                            {
                                if ((SNAccount.CurrentProfile.CurrentPeriodStart > SNAccount.CurrentProfile.BackupPeriodStart))
                                {
                                    newPeriod = true;
                                    // Go to the previous week
                                    SNAccount.CurrentProfile.CurrentPeriodEnd = SNAccount.CurrentProfile.CurrentPeriodStart;
                                    SNAccount.CurrentProfile.CurrentPeriodStart = SNAccount.CurrentProfile.CurrentPeriodStart.AddDays(-30);
                                    if (SNAccount.CurrentProfile.CurrentPeriodStart < SNAccount.CurrentProfile.BackupPeriodStart)
                                    {
                                        SNAccount.CurrentProfile.CurrentPeriodStart = SNAccount.CurrentProfile.BackupPeriodStart;
                                    }
                                    FBAPI.SetTimeRange(SNAccount.CurrentProfile.CurrentPeriodStart, SNAccount.CurrentProfile.CurrentPeriodEnd);
                                    // return state to all data that is associated to time range
                                    //CurrentBackupState = BACKUPMYWALL;
                                    CurrentBackupState = BACKUPMYALBUMS;
                                    DBLayer.UpdateBackup(currentBackupNumber, SNAccount.CurrentProfile.CurrentPeriodStart, SNAccount.CurrentProfile.CurrentPeriodEnd, CurrentBackupState);
                                    nReqs = GetDataForCurrentState(ID, SNID);
                                }
                                else
                                {
                                    // TODO: Allow to show this information, probably a different state
                                    // TODO: Localize
                                    ErrorMessage += "only waiting for last requests in flight to complete backup";
                                }
                            }
                        }
                    }
                }
                if (backupInProgress)
                {
                    /*
                    ErrorMessage += "\nCurrent Backup " + currentBackupNumber + ", currently working on priority " + currentPriorityGlobal + ", limit " + minPriorityGlobal 
                        + " from " +SNAccount.CurrentProfile.CurrentPeriodStart 
                        + " to " +SNAccount.CurrentProfile.CurrentPeriodEnd
                        + "\nRequests in flight: " + FBAPI.InFlight
                        + " requests in save: " + nInSaveRequests
                        + " requests in parse: " + nInParseRequests
                        + "\n";
                     * */
                            
                    foreach (int reqID in queueReq)
                    {
                        AsyncReqQueue apiReq = new AsyncReqQueue(reqID);
                        // special case: ID is not the requested, the request is not there any longer?
                        if (apiReq.ID != -1)
                        {
                            //ErrorMessage += "ID: " + apiReq.ID + " Type: " + apiReq.ReqType + " Pri: " + apiReq.Priority + " URL: " + apiReq.ReqURL + "\n";
                            apiReq.Send();
                        } // if
                    } // foreach
                }
                else
                {
                    ErrorMessage += "\nCurrent Backup " + currentBackupNumber + " done\n";
                }
            } // if not too many requests pending...
            else
            {
                /*
                ErrorMessage += "\nCurrent Backup " + currentBackupNumber + ", currently working on priority " + currentPriorityGlobal + ", limit " + minPriorityGlobal
                    + " from " + SNAccount.CurrentProfile.CurrentPeriodStart
                    + " to " + SNAccount.CurrentProfile.CurrentPeriodEnd
                    + "\nRequests in flight: " + FBAPI.InFlight
                    + " requests in save: " + nInSaveRequests
                    + " requests in parse: " + nInParseRequests
                    + "\n";
                ErrorMessage += "\nWaiting for " + FBAPI.InFlight + " requests to be processed";
                 */
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
            //nRequestsInTransit--;
            nNotParsedRequests++;
            CountPerState[SENT]--;

            if (result)
            {
                GotOneProfilePic = true;
                nFriendsPictures++;
                CountPerState[RECEIVED]++;
                if (parent != null)
                {
                    if (parent != null && response.IndexOf("jpg") > 0)
                    {
                        DateTime perfCheck = DateTime.Now;
                        DBLayer.UpdateProfilePic((long)parent, response, out errorData);
                        TimeSpan temp = DateTime.Now.Subtract(perfCheck);
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
            //nRequestsInTransit--;
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

            //nRequestsInTransit--;
            FBCollection friends = null;
            if (result)
            {
                friends = new FBCollection(response, "FBPerson", parent, parentSNID);
                friends.Distance = 1;
                nInParseRequests++;
                friends.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                CountPerState[RECEIVED]--;
                nFriends += friends.items.Count;

                if (friends.Next != null)
                {
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBFriends", friends.Next, SIZETOGETPERPAGE, ProcessFriends);
                    apiReq.Queue(999);
                }
                else
                {
                    // only when done
                    GotFriendList = true;
                }

                // save all children...
                string error;
                nInSaveRequests++;
                friends.Save(out error);
                nInSaveRequests--;
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
            //nRequestsInTransit--;
            GotWall = true;
            FBCollection wall = null;
            // TODO: Moving parse to a different async method
            if (result)
            {
                wall = new FBCollection(response, "FBPost", parent, parentSNID);
                wall.PersonDataField = "WallRequestID";
                nInParseRequests++;
                wall.Parse();
                foreach (FBPost item in wall.items)
                {
                    if (item.ToID == null)
                    {
                        item.ToID = parentSNID;
                        if (item.ToID == item.FromID)
                        {
                            item.ToName = item.FromName;
                        }
                    }
                }
                nInParseRequests--;
                CountPerState[PARSED]++;
                CountPerState[RECEIVED]--;

                string error;
                nInSaveRequests++;
                wall.Save(out error);
                nInSaveRequests--;
                nPosts += wall.CurrentNumber;

                // TODO: Review to make sure it does not go further than the appropriate timeframe
                if (wall.Next != null)
                {
                    AsyncReqQueue apiReq = FBAPI.MoreData("FBWall", wall.Next, SIZETOGETPERPAGE, ProcessWall);
                    apiReq.Queue(minPriorityGlobal);
                }
            }
            return GenericProcess(hwnd, result, response, wall, true);
        }

        public static bool ProcessNews(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            FBCollection wall = null;
            // Moving parse to a different async method
            if (result)
            {
                wall = new FBCollection(response, "FBPost", parent, parentSNID);
                wall.PersonDataField = "NewsRequestID";
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

                //if (wall.Next != null)
                //{
                //    AsyncReqQueue apiReq = FBAPI.MoreData("FBNews", wall.Next, SIZETOGETPERPAGE, ProcessNews);
                //    apiReq.Queue(minPriorityGlobal);
                //}
            }
            return GenericProcess(hwnd, result, response, wall, true);
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
            //nRequestsInTransit--;
            FBCollection inbox = null;
            if (result)
            {
                inbox = new FBCollection(response, "FBMessage", parent, parentSNID);
                inbox.PersonDataField = "InboxRequestID";
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

                //if (processChildren)
                //{
                //    foreach (FBMessage message in inbox.items)
                //    {
                //        AsyncReqQueue apiReq = FBAPI.Thread(message.SNID, ProcessInbox);
                //        apiReq.Queue(minPriorityGlobal);
                //    }

                //    if (inbox.Next != null)
                //    {
                //        AsyncReqQueue apiReq = FBAPI.MoreData("FBInbox", inbox.Next, SIZETOGETPERPAGE, ProcessInbox);
                //        apiReq.Queue(minPriorityGlobal);
                //    }
                //}
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
            GotInbox = true;
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
            //nRequestsInTransit--;
            FBCollection notes = null;
            if (result)
            {
                notes = new FBCollection(response, "FBNote", parent, parentSNID);
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
            //nRequestsInTransit--;
            FBCollection albums = null;
            if (result)
            {
                albums = new FBCollection(response, "FBAlbum", parent, parentSNID);
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

                //if (albums.CurrentNumber > 0 && albums.Next != null)
                //{
                //    AsyncReqQueue apiReq = FBAPI.MoreData("FBAlbums", albums.Next, SIZETOGETPERPAGE, ProcessAlbums);
                //    apiReq.Queue(minPriorityGlobal);
                //}
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
            //nRequestsInTransit--;
            string errorData = "";

            GotEvent = true;
            if (result)
            {
                FBCollection events = new FBCollection(response, "FBEvent", parent, parentSNID);
                events.PersonDataField = "EventRequestID";
                nInParseRequests++;
                events.Parse();
                nInParseRequests--;
                CountPerState[PARSED]++;
                nInSaveRequests++;
                events.Save(out errorData);
                nEvents += events.CurrentNumber;
                nInSaveRequests--;

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
            GotFriendLists = true;
            //nRequestsInTransit--;
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
                    apiReq.Queue(999);
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
            //nRequestsInTransit--;
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
            //nRequestsInTransit--;
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
            //nRequestsInTransit--;
            CountPerState[SENT]--;
            string errorData = "";

            if (result)
            {
                FBCollection notifications = new FBCollection(response, "FBNotification", parent, parentSNID);
                nInParseRequests++;                
                notifications.Parse();
                nInParseRequests--;
                nNotifications = notifications.items.Count;
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
            //nRequestsInTransit--;
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
            //nRequestsInTransit--;
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
            //nRequestsInTransit--;
            CountPerState[SENT]--;
            if (result)
            {
                nFriendsProcessed++;
                return true;
            }
            nFailedRequests++;
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
            //nRequestsInTransit--;
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

                // TODO: Accept other parents / owners, not only the primary account...
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
            //nRequestsInTransit--;
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

        /// <summary>
        /// process update status response
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public bool ProcessStatus(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (result)
            {
                FBPost statusObj = new FBPost(response);
                statusObj.Parse();
                // TODO: Find the parentID / parentSNID in the database, once it is saved
                bool Saved;
                string error;
                DBLayer.PostDataUpdateSNID(parent.Value, statusObj.SNID, out Saved, out error);
                return Saved;
            }
            return false;
        }

        public bool ProcessNull(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (result)
            {
                // TODO: Find the parentSNID in the database, once it is saved, just to update SNID to the one generated by the SN
                FBPost statusObj = new FBPost(response);
                statusObj.Parse();
                return true;
            }
            return false;
        }

        #endregion

        #endregion

    }

    public class AsyncWorkArgs
    {
        public int ID;
        public string SNID;
        public string Source;
        public string parentSNID;

        public AsyncWorkArgs(int id, string snid)
        {
            ID = id;
            SNID = snid;
            Source = "";
        }

        public AsyncWorkArgs(int id, string snid, string source, string parent)
        {
            ID = id;
            SNID = snid;
            Source = source;
            parentSNID = parent;
        }
    }
}
