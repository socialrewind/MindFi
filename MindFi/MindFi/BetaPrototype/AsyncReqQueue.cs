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

	public const int POTENTIAL	= 0 ;
	public const int QUEUED		= 1 ;
	public const int SENT		= 2 ;
	public const int RETRY		= 3 ;
	public const int RECEIVED	= 4 ;
	public const int PARSED		= 5 ;
	public const int PROCESSED	= 6 ;
	public const int TODELETE	= 7 ;

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
	public static volatile int nPhotos= 0;

	public static volatile int nQueuedRequests = 0;
	public static volatile int nSentRequests = 0;
	public static volatile int nRetryRequests = 0;
	public static volatile int nReceivedRequests = 0;
	public static volatile int nParsedRequests = 0;
	public static volatile int nNotParsedRequests = 0;
	public static volatile int nFailedRequests = 0;

	#endregion
	private static volatile Object obj = new Object();

	public static int QueuedReqs
	{
	    get { return nQueuedRequests; }
	}

	public static int SentReqs
	{
	    get { return nSentRequests; }
	}

	#region "Constructors"

	public AsyncReqQueue(string type, string URL, int limit, CallBack resultCall, bool AddToken=true, long? parentID=null, string parentSNID="" )
	{
		lock ( obj )
		{
		    if ( NextID == -1 )
		    {
			// read from database
			string errorMessage = "";
			NextID = DBLayer.AvailableReqQueueSave(out errorMessage);
			if ( NextID == -1 ) NextID = 0;
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
		Save();
	}

	public AsyncReqQueue(string type, string URL, string filename, int limit, CallBack resultCall, long? parentID, string parentSNID )
	{
		lock ( obj )
		{
		    if ( NextID == -1 )
		    {
			// read from database
			string errorMessage = "";
			NextID = DBLayer.AvailableReqQueueSave(out errorMessage);
			if ( NextID == -1 ) NextID = 0;
		    }
		    ID = NextID++;
		}

		ReqType = type;
		ReqURL = URL;
		Created = DateTime.Now;
		FileName  = filename;
		Priority = 0;
		Limit = limit;
		State = POTENTIAL;
		methodToCall = resultCall;
		Parent = parentID;
		ParentSNID = parentSNID;
		Save();
	}

	/// <summary>
	/// Constructor that creates an AsyncReqQueue object from the RequestsQueue table in the database
	/// </summary>
	public AsyncReqQueue(int id)
	{
	    string ErrorMessage;
	    // Call layer to get data
	    if ( DBLayer.GetAsyncReq(id, out ReqType, out Priority, out Parent, out ParentSNID,
		out Created, out Updated, out ReqURL, out State, out FileName, out addToken, out ErrorMessage) )
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

	#endregion

	#region "Per request methods"
	public bool Queue()
	{
	    if ( State == POTENTIAL )
	    {
		lock ( obj )
		{
		    nQueuedRequests ++;
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
	    if ( State == QUEUED || State == RETRY )
	    {
		lock ( obj )
		{
		    nSentRequests ++;
		    nQueuedRequests --;
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
	    if ( FileName == "" )
	    {
		// DEBUG
		if ( !addToken )
		{
			//System.Windows.Forms.MessageBox.Show("Ready to call graph api: " + ReqURL + "\nID: " +ID);
		}

		result = FBAPI.CallGraphAPI(ReqURL, Limit, methodToCall, ID, Parent, ParentSNID, addToken);
	    }
	    else
	    {
		// DEBUG
		if ( !addToken )
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
	    if ( !Saved )
	    {
		System.Windows.Forms.MessageBox.Show("Failed to save request: " + ID + " error: " + ErrorMessage);
	    }
	}
	#endregion

	#region "Static externally used methods"
	/// <summary>
	/// Create the first, basic requests for the first time logged in
	/// </summary>
        public static void InitialRequests()
	{
	    // TODO: Check if there are pending requests, if so, don't send new initial requests!!!!
	    if ( DBLayer.QueueRetryUpdate() )
	    // DEBUG
	    //if ( !DBLayer.QueueRetryUpdate() )
	    {
	    AsyncReqQueue apiReq = FBAPI.ProfilePic(FBLogin.Me.SNID,
		ProfilePhotoDestinationDir + FBLogin.Me.ID + ".jpg", 
		ProcessFriendPic, FBLogin.Me.ID, FBLogin.Me.SNID);
	    apiReq.Priority = 100;
	    apiReq.Queue();
	    apiReq.Send();
            apiReq = FBAPI.Friends(SIZETOGETPERPAGE, ProcessFriends);
	    apiReq.Priority = 100;
	    apiReq.Queue();
	    apiReq.Send();
	    apiReq = FBAPI.Wall("me", SIZETOGETPERPAGE, ProcessWall);
	    apiReq.Priority = 100;
	    apiReq.Queue();
	    apiReq.Send();
	    apiReq = FBAPI.Events("me", SIZETOGETPERPAGE, ProcessEvents);
	    apiReq.Priority = 100;
	    apiReq.Queue();
	    apiReq.Send();
	    apiReq = FBAPI.Inbox("me", SIZETOGETPERPAGE, ProcessInbox);
	    //apiReq = FBAPI.Inbox("me", 8, ProcessInbox);
	    apiReq.Priority = 100;
	    apiReq.Queue();
	    apiReq.Send();
	    apiReq = FBAPI.PhotoAlbums("me", SIZETOGETPERPAGE, ProcessAlbums);
	    apiReq.Priority = 100;
	    apiReq.Queue();
	    apiReq.Send();
	    }
	}

	/// <summary>
	/// Follows up on pending requests
	/// </summary>
        public static void PendingRequests(out string ErrorMessage)
	{
	    ErrorMessage = "";
	    // Get top queued requests
	    int Limit = 10;
	    if ( nSentRequests - nReceivedRequests - nFailedRequests< Limit)
	    {
		Limit -= nSentRequests - nReceivedRequests -nFailedRequests;
		ArrayList queueReq = DBLayer.GetRequests(Limit, AsyncReqQueue.QUEUED, out ErrorMessage);
		// Assigning callback appropriately
		if ( ErrorMessage != "" )
		{
		    System.Windows.Forms.MessageBox.Show("Error getting queued requests: " + ErrorMessage);
		    return;
		}
		ErrorMessage = "Queued Requests to send: " + queueReq.Count +"\n";
		if ( queueReq.Count == 0 )
		{
		    queueReq = DBLayer.GetRequests(Limit, AsyncReqQueue.RETRY, out ErrorMessage);
		    if ( ErrorMessage != "" )
		    {
			System.Windows.Forms.MessageBox.Show("Error getting retry requests: " + ErrorMessage);
			return;
		    }
		
		    ErrorMessage = "Retry Requests to send: " + queueReq.Count +"\n";
		}
		foreach (int reqID in queueReq)
		{
		    //System.Windows.Forms.MessageBox.Show("id" + reqID);
		    AsyncReqQueue apiReq = new AsyncReqQueue(reqID);
		    // special case: ID is not the requested, the request is not there any longer?
		    if ( apiReq.ID != -1)
		    {
			switch ( apiReq.ReqType )
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
			    case "FBFriends":
				apiReq.methodToCall = ProcessFriends;
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
			apiReq.Send();
		    } // if
		} // foreach
	    } // if not too many requests pending...
	    else
	    {
		ErrorMessage = "Waiting for " + (nSentRequests-nReceivedRequests-nFailedRequests) + " to be responded before sending more...";
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
	    if ( result )
	    {
	    	nReceivedRequests ++;	 
	    }
	    else
	    {
	    	nFailedRequests ++;	 
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
	    nNotParsedRequests ++;
//System.Windows.Forms.MessageBox.Show("Processing Pic: " + result );

            if (result)
            {
		nFriendsPictures ++;
		nReceivedRequests ++;
                if (parent != null)
                {
                    if (parent != null && response.IndexOf("jpg") > 0)
                    {
		        DBLayer.UpdateProfilePic((long) parent, response, out errorData);
			if ( errorData == "" ) return true;
                    }
                }
            }
            // fails to process the pic, probably because of an exception
	    nFailedRequests ++;
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
        public static bool ProcessFriends(int hwnd, bool result, string response, long? parent = null, string parentSNID = "" )
        {
if ( !result)
{
	// System.Windows.Forms.MessageBox.Show("Processing friends (false): " + response );
}

            FBCollection friends = null;
            if (result)
            {
                friends = new FBCollection(response, "FBPerson");
		friends.Distance = 1;
                friends.Parse();
nParsedRequests ++;

                if (friends.Next != null)
		{
//System.Windows.Forms.MessageBox.Show("friends next: " + friends.Next);
		    AsyncReqQueue apiReq = FBAPI.MoreData("FBFriends", friends.Next, SIZETOGETPERPAGE, ProcessFriends);
	    	    apiReq.Priority = 100;
		    apiReq.Queue();
//System.Windows.Forms.MessageBox.Show("apiReq URL: " + apiReq.ReqURL);
		}

		// save all children...
		string error;
		friends.Save(out error);
		foreach (FBPerson item in friends.items)
		{
nFriends ++;
		    AsyncReqQueue apiReq;
		    // TODO: conditional depending on backup profile
		    apiReq = FBAPI.Profile(item.SNID, ProcessOneFriend);
		    apiReq.Priority = 50;
		    apiReq.Queue();
		    apiReq = FBAPI.ProfilePic(item.SNID,
			ProfilePhotoDestinationDir + item.ID + ".jpg", 
			ProcessFriendPic, item.ID, item.SNID);
		    apiReq.Priority = 50;
		    apiReq.Queue();
/*
		    // TODO: conditional depending on backup profile
		    apiReq = FBAPI.Wall(item.SNID, SIZETOGETPERPAGE, ProcessWall);
		    apiReq.Priority = 50;
		    apiReq.Queue();
		    // TODO: conditional depending on backup profile
		    apiReq = FBAPI.PhotoAlbums(item.SNID, SIZETOGETPERPAGE, ProcessAlbums);
		    apiReq.Priority = 20;
		    apiReq.Queue();
*/
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
        public static bool ProcessWall(int hwnd, bool result, string response, long? parent = null, string parentSNID = "" )
        {
//System.Windows.Forms.MessageBox.Show("Processing wall: " + response );
if ( !result)
{
//System.Windows.Forms.MessageBox.Show("Processing wall (false): " + response );
}
            FBCollection wall = null;
            if (result)
            {
                wall = new FBCollection(response, "FBPost");
                wall.Parse();
nParsedRequests++;

                string error;
                wall.Save(out error);
nPosts += wall.CurrentNumber;

		if ( error != "" )
		{
		    //System.Windows.Forms.MessageBox.Show ( "Error saving wall for parent " + parent + "\n" + error );
		}

		foreach ( FBPost post in wall.items )
		{
		    AsyncReqQueue apiReq = FBAPI.Likes(post.SNID, SIZETOGETPERPAGE, ProcessLikes, post.ID);
		    apiReq.Priority = 50;
		    apiReq.Queue();
		}


                if (wall.Next != null)
		{
//System.Windows.Forms.MessageBox.Show("Wall next: " + wall.Next);
		    AsyncReqQueue apiReq = FBAPI.MoreData("FBWall", wall.Next, SIZETOGETPERPAGE, ProcessWall);
	    	    apiReq.Priority = 50;
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
        public static bool ProcessOneFriend(int hwnd, bool result, string response, long? parent = null, string parentSNID = "" )
        {
	    if ( result )
	    {
	    	nReceivedRequests ++;	 
	    	nFriendsProcessed ++;
		FBPerson currentFriend = new FBPerson(response, 1);
		currentFriend.Parse();
		nParsedRequests ++;

		string errorData;
		currentFriend.Save(out errorData);
		if ( errorData != "" )
		{
		    //System.Windows.Forms.MessageBox.Show("Error saving friend: " + errorData);
		    return false;
		}
		return true;
	    }
	    else
	    {
	    	nFailedRequests ++;	 
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
        public static bool ProcessInbox(int hwnd, bool result, string response, long? parent = null, string parentSNID = "" )
        {
//System.Windows.Forms.MessageBox.Show("Processing inbox: " + response );
if ( !result)
{
//System.Windows.Forms.MessageBox.Show("Processing inbox (false): " + response );
}
            FBCollection inbox = null;
            if (result)
            {
                inbox = new FBCollection(response, "FBMessage");
//System.Windows.Forms.MessageBox.Show("about to parse Inbox");
                inbox.Parse();
//System.Windows.Forms.MessageBox.Show("Inbox parsed");
nParsedRequests++;

                string error;
//System.Windows.Forms.MessageBox.Show("about to save inbox");
                inbox.Save(out error);
//System.Windows.Forms.MessageBox.Show("saved inbox: " + inbox.CurrentNumber);
nMessages += inbox.CurrentNumber;

	    if ( error != "" )
	    {
		//System.Windows.Forms.MessageBox.Show("Error saving inbox " + error );
		return false;
	    }

                if (inbox.Next != null)
		{
//System.Windows.Forms.MessageBox.Show("Inbox next: " + inbox.Next);
		    //AsyncReqQueue apiReq = FBAPI.MoreData("FBInbox", inbox.Next, SIZETOGETPERPAGE, ProcessInbox);
		    AsyncReqQueue apiReq = FBAPI.MoreData("FBInbox", inbox.Next, 2, ProcessInbox);
	    	    apiReq.Priority = 101;
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
        public static bool ProcessAlbums(int hwnd, bool result, string response, long? parent = null, string parentSNID = "" )
        {
	    if ( !result)
	    {
		//System.Windows.Forms.MessageBox.Show("Processing albums (false): " + response );
	    }
            FBCollection albums = null;
            if (result)
            {
		//System.Windows.Forms.MessageBox.Show("about to process album");
                albums = new FBCollection(response, "FBAlbum");
                albums.Parse();
		//System.Windows.Forms.MessageBox.Show("Number of albums: " + albums.CurrentNumber );
		nParsedRequests++;
		nAlbums += albums.CurrentNumber;

		string error;
		albums.Save(out error);
		if ( error != "" )
		{
		    //System.Windows.Forms.MessageBox.Show("Error saving albums " + error );
		    return false;
		}

		foreach ( FBAlbum album in albums.items )
		{
		    if (!System.IO.Directory.Exists(AlbumDestinationDir + "\\" + album.ID))
		    {
			System.IO.Directory.CreateDirectory(AlbumDestinationDir + "\\" + album.ID);
		    }

		    //System.Windows.Forms.MessageBox.Show("sending req for photos in album " + album.SNID + " ID: " + album.ID );
		    AsyncReqQueue apiReq = FBAPI.PhotosInAlbum(album.SNID, SIZETOGETPERPAGE, ProcessPhotosInAlbum, album.ID);
		    apiReq.Priority = 50;
		    apiReq.Queue();
		    apiReq = FBAPI.Likes(album.SNID, SIZETOGETPERPAGE, ProcessLikes, album.ID);
		    apiReq.Priority = 50;
		    apiReq.Queue();
		}

		if (albums.CurrentNumber >0 && albums.Next != null)
		{
		    //System.Windows.Forms.MessageBox.Show("Albums next: " + albums.Next);
		    AsyncReqQueue apiReq = FBAPI.MoreData("FBAlbums", albums.Next, SIZETOGETPERPAGE, ProcessAlbums);
	    	    apiReq.Priority = 50;
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
        public static bool ProcessPhotosInAlbum(int hwnd, bool result, string response, long? parent = null, string parentSNID = "" )
        {
	    if ( !result )
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
		nParsedRequests++;
		nPhotos += photos.CurrentNumber;

		string error;
		photos.Save(out error);
		if ( error != "" )
		{
		    System.Windows.Forms.MessageBox.Show("Error saving photos " + error );
		    return false;
		}

		foreach ( FBPhoto photo in photos.items )
		{
		    // System.Windows.Forms.MessageBox.Show("will download photo: " + photo.ID);
		    AsyncReqQueue apiReq = FBAPI.DownloadPhoto(photo.Source, 
			AlbumDestinationDir + parent + "\\" + photo.ID + ".jpg", 
			ProcessPhoto, photo.ID, photo.SNID);
		    apiReq.Priority = 50;
		    apiReq.Queue();
		    apiReq = FBAPI.Likes(photo.SNID, SIZETOGETPERPAGE, ProcessLikes, photo.ID);
		    apiReq.Priority = 50;
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
        public static bool ProcessPhoto(int hwnd, bool result, string response, long? parent = null, string parentSNID = "" )
        {
	    string errorData = "";
	    nNotParsedRequests ++;
	    // System.Windows.Forms.MessageBox.Show("Processing Photo parentID: " + parent + "\n" + result );

            if (result)
            {
		nReceivedRequests ++;
                if (parent != null)
                {
                    if (parent != null && response.IndexOf("jpg") > 0)
                    {
		        DBLayer.UpdatePhoto((long) parent, response, out errorData);
			if ( errorData == "" ) return true;
                    }
                }
            }
            // fails to process the pic, probably because of an exception
	    nFailedRequests ++;
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
        public static bool ProcessLikes(int hwnd, bool result, string response, long? parent = null, string parentSNID = "" )
        {
	    string errorData = "";
	    nNotParsedRequests ++;
	    //System.Windows.Forms.MessageBox.Show("Processing likes result: " + parent + "\n" + response );

            if (result)
            {
		nReceivedRequests ++;
                if (parent != null)
                {
			// create FBLikes object
			FBLikes likes = new FBLikes(response, parent, parentSNID);
			likes.Parse();
			likes.Save(out errorData);
			if ( errorData == "" ) return true;
                    
                }
            }
	    nFailedRequests ++;
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
        public static bool ProcessEvents(int hwnd, bool result, string response, long? parent = null, string parentSNID = "" )
        {
	    string errorData = "";
	    nNotParsedRequests ++;
	    //System.Windows.Forms.MessageBox.Show("Processing events result: " + result + "\n" + response );

            if (result)
            {
		nReceivedRequests ++;
		FBCollection events = new FBCollection(response, "FBEvent", parent, parentSNID);
		events.Parse();
		events.Save(out errorData);
                if ( errorData == "" ) return true;
            }
	    nFailedRequests ++;
	    return false;
        }

	#endregion
			
    }
}
