﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;
using MBBetaAPI.SRAPI;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that contains methods to call the Facebook Graph API
    /// </summary>
    public class FBAPI
    {
        private const string FBGraphAPIURL = "https://graph.facebook.com/";
        public const int DEFAULT_TIMEOUT = 30000; // 30 sec timeout
        public const int DEFAULT_LIMIT = 200;
        private static string InitialTime; // +0000 - needs to be encoded before adding the +
        private static string EndTime;
        public static volatile HttpRequestList requestList = new HttpRequestList();
        private static volatile Object obj = new Object();

        public static int InFlight
        {
            get { lock (obj) { return requestList.Size; } }
        }

        #region "Methods"
        private static string DateISO8601(DateTime date)
        {
            string temp = date.Year + "-";
            if (date.Month < 10)
                temp += 0;
            temp += date.Month + "-";
            if (date.Day < 10)
                temp += 0;
            // TODO: Add timezone and consider time in the process
            temp += date.Day + "T00:00:00";
            return temp;
        }

        public static void SetTimeRange(DateTime start, DateTime end)
        {
            lock (obj)
            {
                if ( start == null || end == null )
                {
                    throw new Exception("Time range should never be getting a null");
                }
                InitialTime = DateISO8601(start);
                // next day, to make sure today info is included
                EndTime = DateISO8601(end.AddDays(1));
            }
        }

        /// <summary>
        /// Gets the basic current logged in FB user information
        /// </summary>
        /// <param name="resultCall">Function that is called once the user data is parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Async Request record</returns>
        public static AsyncReqQueue Me(CallBack resultCall)
        {
            // limit 1, no need to specify in the URL
            AsyncReqQueue me = new AsyncReqQueue("FBPerson",
                FBGraphAPIURL + "me", 1,
                resultCall);
            return me;
        }

        /// <summary>
        /// Gets the friends list for the current logged in FB user
        /// </summary>
        /// <param name="resultCall">Function that is called once the friend list is parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Async Request record</returns>
        public static AsyncReqQueue Friends(string SNID, int Limit, CallBack resultCall, long parentID, string parentSNID = "")
        {
            if (parentSNID == "")
                parentSNID = SNID;
            AsyncReqQueue me = new AsyncReqQueue("FBFriends",
                FBGraphAPIURL + SNID + "/friends", Limit,
                resultCall, true, false, parentID, parentSNID);
            return me;
        }

        /// <summary>
        /// Gets the list of family members for the user
        /// </summary>
        /// <param name="resultCall">Function that is called once the friend list is parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Async Request record</returns>
        public static AsyncReqQueue Family(string SNID, int Limit, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBFamily",
                FBGraphAPIURL + SNID + "/family", Limit,
                resultCall, true, false, null, SNID);
            return me;
        }

        /// <summary>
        /// Gets FB user profile information, by alias
        /// </summary>
        /// <param name="Who">Alias of the desired user</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the user data is parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Profile(string Who, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBPerson",
                FBGraphAPIURL + Who, 1,
                resultCall);
            return me;
        }

        /// <summary>
        /// Gets FB user profile information, by SNID
        /// </summary>
        /// <param name="SNID">ID of the desired user</param>
        /// <param name="resultCall">Function that is called once the user data is parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Profile(Int64 SNID, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBPerson",
                FBGraphAPIURL + SNID, 1,
                resultCall);
            return me;
        }

        /// <summary>
        /// Downloads the profile Picture to the specified file
        /// </summary>
        /// <param name="Who">Alias or username in Facebook</param>
        /// <param name="fileName">Name of file to save into (including path)</param>
        /// <param name="resultCall">Function that is called once the picture is saved. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue ProfilePic(string Who, string fileName, CallBack resultCall, int? Parent, string ParentSNID)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBProfilePic",
                FBGraphAPIURL + Who + "/picture",
                fileName, 1, resultCall, Parent, ParentSNID);
            return me;
        }

        /// <summary>
        /// Gets FB user wall posts, by alias
        /// </summary>
        /// <param name="Who">Alias of the desired user</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Wall(string Who, int Limit, CallBack resultCall, long parentID, string parentSNID="")
        {
            if (parentSNID == "")
                parentSNID = Who;
            AsyncReqQueue me = new AsyncReqQueue("FBWall",
                FBGraphAPIURL + Who + "/feed",
                Limit, resultCall, true, true, parentID, parentSNID);
            return me;
        }

        /// <summary>
        /// Gets recent wall posts = news feed
        /// </summary>
        /// <param name="Who">Alias of the desired user</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue News(int Limit, CallBack resultCall, long parentID, string parentSNID = "")
        {
            if (parentSNID == "")
                parentSNID = "me";
            AsyncReqQueue me = new AsyncReqQueue("FBNews",
                FBGraphAPIURL + "me/home",
                Limit, resultCall, true, true, parentID, parentSNID);
            return me;
        }

        /// <summary>
        /// Gets notes, by alias
        /// </summary>
        /// <param name="Who">Alias of the desired user</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Notes(string Who, int Limit, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBNotes",
                FBGraphAPIURL + Who + "/notes",
                Limit, resultCall, true, true);
            return me;
        }

        /// <summary>
        /// Gets FB user notifications, by alias
        /// </summary>
        /// <param name="Who">Alias of the desired user</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Notifications(string Who, int Limit, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBNotifications",
                FBGraphAPIURL + Who + "/notifications?include_read=1",
                Limit, resultCall, true, true);
            return me;
        }

        /// <summary>
        /// Gets multiple FB events, by user ID
        /// </summary>
        /// <param name="Who">Alias of the desired user</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Events(string Who, int Limit, CallBack resultCall, long parentID, string parentSNID = "")
        {
            AsyncReqQueue me = new AsyncReqQueue("FBEvents",
                FBGraphAPIURL + Who + "/events",
                Limit, resultCall, true, true, parentID, parentSNID);
            return me;
        }

        /// <summary>
        /// Gets an specific object, by SNID
        /// </summary>
        /// <param name="SNID">SNID of Event in Facebook</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called for parsing the event. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        private static AsyncReqQueue FBObject(string SNID, string ObjType, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue(ObjType,
                FBGraphAPIURL + SNID,
                1, resultCall, true, false);
            return me;
        }

        /// <summary>
        /// Gets an specific FB event, by SNID
        /// </summary>
        /// <param name="SNID">SNID of Event in Facebook</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called for parsing the event. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Event(string SNID, CallBack resultCall)
        {
            return FBObject(SNID, "FBEvent", resultCall);
        }

        /// <summary>
        /// Gets an specific FB post, by SNID
        /// </summary>
        /// <param name="SNID">SNID of post in Facebook</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called for parsing the post. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Post(string SNID, CallBack resultCall)
        {
            return FBObject(SNID, "FBPost", resultCall);
        }

        /// <summary>
        /// Gets an specific FB message thread, by SNID
        /// </summary>
        /// <param name="SNID">SNID of thread in Facebook</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called for parsing the post. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Thread(string SNID, CallBack resultCall)
        {
            return FBObject(SNID, "FBThread", resultCall);
        }

        /// <summary>
        /// Gets FB messages inbox for the currently logged user
        /// </summary>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Inbox(string Who, int Limit, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBInbox",
                FBGraphAPIURL + Who + "/inbox",
                Limit, resultCall, true, true);
            return me;
        }

        /// <summary>
        /// Gets the data of the Photo Albums for the specified user
        /// </summary>
        /// <param name="Who">Alias or username in Facebook</param>
        /// <param name="Limit">Maximum number of JSON records</param>
        /// <param name="resultCall">Function that is called once the albums are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue PhotoAlbums(string Who, int Limit, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBAlbums",
                FBGraphAPIURL + Who + "/albums",
                Limit, resultCall, true, true);
            return me;
        }

        /// <summary>
        /// Gets the data of the Photos in an specified Album
        /// </summary>
        /// <param name="Album">SNID of Album in Facebook</param>
        /// <param name="resultCall">Function that is called once the albums are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue PhotosInAlbum(string Album, int Limit, CallBack resultCall, int? Parent)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBPhotos",
                FBGraphAPIURL + Album + "/photos",
                Limit, resultCall, true, false, Parent, Album);
            return me;
        }

        /// <summary>
        /// Gets the list of users that like an object
        /// </summary>
        /// <param name="SNID">SNID of Object in Facebook</param>
        /// <param name="resultCall">Function that is called once the albums are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Likes(string SNID, int Limit, CallBack resultCall, int? Parent)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBLikes",
                FBGraphAPIURL + SNID + "/likes",
                Limit, resultCall, true, false, Parent, SNID);
            return me;
        }

        /// <summary>
        /// Gets the friend lists for the current logged in FB user
        /// </summary>
        /// <param name="resultCall">Function that is called once the albums are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue FriendLists(string SNID, int Limit, CallBack resultCall, int? Parent)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBFriendLists",
                FBGraphAPIURL + SNID + "/friendlists",
                Limit, resultCall, true, false, Parent, SNID);
            return me;
        }

        // TODO: Refactor and modularize all similar functions
        public static AsyncReqQueue Members(string SNID, int Limit, CallBack resultCall, int? Parent)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBFriendList",
                FBGraphAPIURL + SNID + "/members",
                Limit, resultCall, true, false, Parent, SNID);
            return me;
        }

        /// <summary>
        /// Gets users attending an FB event
        /// </summary>
        /// <param name="SNID">Event ID</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue AttendingEvent(string SNID, int Limit, CallBack resultCall, int? Parent)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBAttending",
                FBGraphAPIURL + SNID + "/attending",
                Limit, resultCall, true, false, Parent, SNID);
            return me;
        }

        /// <summary>
        /// Gets users maybe attending an FB event
        /// </summary>
        /// <param name="SNID">Event ID</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue MaybeEvent(string SNID, int Limit, CallBack resultCall, int? Parent)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBMaybe",
                FBGraphAPIURL + SNID + "/maybe",
                Limit, resultCall, true, false, Parent, SNID);
            return me;
        }

        /// <summary>
        /// Gets users maybe attending an FB event
        /// </summary>
        /// <param name="SNID">Event ID</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue DeclinedEvent(string SNID, int Limit, CallBack resultCall, int? Parent)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBDeclined",
                FBGraphAPIURL + SNID + "/declined",
                Limit, resultCall, true, false, Parent, SNID);
            return me;
        }

        /// <summary>
        /// Downloads the Photo to the specified file
        /// </summary>
        /// <param name="SNID">ID of the photo in Facebook</param>
        /// <param name="fileName">Name of file to save into (including path)</param>
        /// <param name="resultCall">Function that is called once the picture is saved. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue DownloadPhoto(string URL, string fileName, CallBack resultCall, int? Parent, string ParentSNID)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBPhoto",
                URL, fileName, 1, resultCall, Parent, ParentSNID);
            return me;
        }

        /// <summary>
        /// Gets Next/Previous wall posts/inbox messages/etc, using direct link
        /// </summary>
        /// <param name="URL">Link to previous/next wall posts</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue MoreData(string dataType, string URL, int Limit, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue(dataType,
                URL, Limit, resultCall, false, true);
            return me;
        }

        #endregion

        #region "Publishing methods"
        /// <summary>
        /// Updates the status of the user - post in another user's wall
        /// </summary>
        /// <param name="Who">User to post to, use "me" to post in own wall</param>
        /// <param name="Status">Text to post</param>
        /// <param name="resultCall">Function that is called once the user data is parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Nothing - no Async Request record neeed</returns>
        public static AsyncReqQueue UpdateStatus(string Who, string Status, CallBack resultCall)
        {
            // TODO: Modularize better, reduce dependencies...
            if (SNAccount.CurrentProfile == null)
            {
                return null;
            }

            string error;
            AsyncReqQueue me = null;

            FBPost status = new FBPost("");
            status.FromID = status.ToID = SNAccount.CurrentProfile.SNID;
            status.FromName = status.ToName = SNAccount.CurrentProfile.Name;
            status.SNID = "Pending";
            status.Message = Status;
            // TODO: Review constants
            status.ApplicationID = FBLogin.APPID;
            status.ApplicationName = "Social Rewind";
            status.PostType = "status";
            status.Save(out error);
            if (error == "")
            {
                me = new AsyncReqQueue("PostStatus", FBGraphAPIURL + Who + "/feed", 1, resultCall, true, false, status.ID, status.SNID, Status);
                me.Priority = 999;
                me.Queue();
            }
            return me;
        }

        /// <summary>
        /// Adds a comment to an object
        /// </summary>
        /// <param name="SNID">Social Network ID for the object</param>
        /// <param name="Comment">Text to comment</param>
        /// <param name="resultCall">Function that is called once the user data is parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Nothing - no Async Request record neeed</returns>
        public static AsyncReqQueue AddComment(string SNID, string Comment, CallBack resultCall)
        {
            // TODO: Modularize better, reduce dependencies...
            if (SNAccount.CurrentProfile == null)
            {
                return null;
            }

            string error;
            AsyncReqQueue me = null;

            FBPost status = new FBPost("");
            status.FromID = status.ToID = SNAccount.CurrentProfile.SNID;
            status.FromName = status.ToName = SNAccount.CurrentProfile.Name;
            status.SNID = "Pending";
            status.Message = Comment;
            // TODO: Review constants
            status.ApplicationID = FBLogin.APPID;
            status.ApplicationName = "Social Rewind";
            status.PostType = "comment";
            status.Save(out error);
            if (error == "")
            {
                bool saved;
                DBLayer.PostDataUpdateParentSNID(status.ID, SNID, out saved, out error);
                if (saved)
                {
                    me = new AsyncReqQueue("PostComment", FBGraphAPIURL + SNID + "/comments", 1, resultCall, true, false, status.ID, SNID, Comment);
                    me.Priority = 999;
                    me.Queue();
                }
            }
            return me;
        }

        /// <summary>
        /// Likes an object
        /// </summary>
        /// <param name="SNID">Object to like</param>
        /// <param name="resultCall">Function that is called once the user data is parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Nothing - no Async Request record neeed</returns>
        public static AsyncReqQueue UpdateLike(string SNID, CallBack resultCall)
        {
            // TODO: Modularize better, reduce dependencies...
            if (SNAccount.CurrentProfile == null)
            {
                return null;
            }
            // TODO: Remove from the database if LIKE was already there...
            bool Saved;
            string error;
            DBLayer.ActionDataSave(SNAccount.CurrentProfile.SNID, SNID, Verb.LIKE, out Saved, out error);
            AsyncReqQueue me = null;
            if (Saved)
            {
                me = new AsyncReqQueue("PostLike", FBGraphAPIURL + SNID + "/likes", 1, resultCall, true, false, null, SNID, "");
                me.Priority = 999;
                me.Queue();
            }
            return me;
        }

        /// <summary>
        /// Unlikes an object
        /// </summary>
        /// <param name="SNID">Object to unlike</param>
        /// <param name="resultCall">Function that is called once the user data is parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Nothing - no Async Request record neeed</returns>
        public static AsyncReqQueue UpdateUnlike(string SNID, CallBack resultCall)
        {
            // TODO: Modularize better, reduce dependencies...
            if (SNAccount.CurrentProfile == null)
            {
                return null;
            }
            // TODO: Remove from the database if LIKE was already there...
            /*
            bool Saved;
            DBLayer.ActionDataSave(SNAccount.CurrentProfile.SNID, SNID, Verb.LIKE, out Saved, out error);
            if (Saved)
            {
             */
            string error;
            DBLayer.UnlikeDBUpdate(SNID, out error);
            AsyncReqQueue me = null;
            me = new AsyncReqQueue("PostUnlike", FBGraphAPIURL + SNID + "/likes", 1, resultCall, true, false, null, SNID, "");
            me.Priority = 999;
            me.Queue();
            /*
            }
             * */
            return me;
        }
        #endregion


        #region "Helper Methods"
        /// <summary>
        /// General method that makes the HTTP asynchronous request to start the Facebook Graph API call, when it returns JSON records
        /// </summary>
        /// <param name="GraphAPIURL">URL for the appropriate Graph API method</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success or failure</returns>
        public static bool CallGraphAPI(string GraphAPIURL, int Limit, CallBack resultCall, long AsyncID, long? parent, string parentSNID, bool addToken, bool addDateRange = false)
        {
            try
            {
                string URLToGet = GraphAPIURL;
                if (addToken)
                {
                    if (Limit == 0)
                    {
                        Limit = DEFAULT_LIMIT;
                    }
                    if (URLToGet.Contains("?"))
                    {
                        URLToGet += "&";
                    }
                    else
                    {
                        URLToGet += "?";
                    }
                    URLToGet += "access_token=" + FBLogin.token + "&limit=" + Limit.ToString();
                    // TODO: how to add in a more smart way the since / until
                    if (addDateRange)
                    {
                        if ( InitialTime == null || EndTime == null )
                        {
                            // TODO: Find why this is happening
                            //SetTimeRange(SNAccount.CurrentProfile.CurrentPeriodStart, SNAccount.CurrentProfile.CurrentPeriodEnd);
                            SetTimeRange(SRBackup.CurrentPeriodStart, SRBackup.CurrentPeriodEnd);
                        }
                        URLToGet += "&since=" + InitialTime + "&until=" + EndTime;
                    }
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URLToGet);

                JSONResultCallback state = new JSONResultCallback(request, resultCall, AsyncID, parent, parentSNID);
                // Add request to the list
                requestList.Queue(request);
                IAsyncResult res = request.BeginGetResponse(new AsyncCallback(JSONResultCallback.JSONResponseProcess), state);
                ThreadPool.RegisterWaitForSingleObject(res.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                    request, DEFAULT_TIMEOUT, true);
            }
            catch (Exception ex)
            {
                // TODO: Possibly improve Instrumentation
                System.Diagnostics.Debug.WriteLine("Exception during CallGraphAPI: " + ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// General method that makes the HTTP asynchronous request to start the Facebook Graph API call, when it returns a file to download
        /// </summary>
        /// <param name="GraphAPIURL">URL for the appropriate Graph API method</param>
        /// <param name="FileName">Path in which the file will be downloaded</param>
        /// <param name="resultCall">Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success or failure</returns>
        public static bool CallGraphAPI(string GraphAPIURL, string FileName, CallBack resultCall, long AsyncID, long? parent, string parentSNID)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GraphAPIURL);
                // Start the asynchronous operation.    
                FileResultCallback state = new FileResultCallback(request, FileName, resultCall, AsyncID, parent, parentSNID);
                // Add request to the list
                requestList.Queue(request);
                IAsyncResult res = request.BeginGetResponse(new AsyncCallback(FileResultCallback.FileResponseProcess), state);
                ThreadPool.RegisterWaitForSingleObject(res.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                    request, DEFAULT_TIMEOUT, true);
            }
            catch (Exception ex)
            {
                // TODO: Possibly improve Instrumentation
                System.Diagnostics.Debug.WriteLine("Exception during CallGraphAPI: " + ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// General method that makes the HTTP asynchronous request to get a file to download, with no Graph API involved
        /// </summary>
        /// <param name="Link">URL for the final file to save</param>
        /// <param name="FileName">Path in which the file will be downloaded</param>
        /// <param name="resultCall">Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success or failure</returns>
        public static bool DownloadLink(string Link, string FileName, CallBack resultCall, long AsyncID, long? parent, string parentSNID)
        {
            try
            {
                // Asynchronous Get
                // Create a new HttpWebRequest object.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Link);
                // Start the asynchronous operation.    
                FileResultCallback state = new FileResultCallback(request, FileName, resultCall, AsyncID, parent, parentSNID);
                // Add request to the list
                requestList.Queue(request);
                IAsyncResult res = request.BeginGetResponse(new AsyncCallback(FileResultCallback.FileResponseProcess), state);
                ThreadPool.RegisterWaitForSingleObject(res.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                    request, DEFAULT_TIMEOUT, true);
            }
            catch (Exception ex)
            {
                // TODO: Possibly improve Instrumentation
                System.Diagnostics.Debug.WriteLine("Exception during CallGraphAPI: " + ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// General method that makes the HTTP asynchronous request to start the Facebook Graph API call, when it returns JSON records, using Post method
        /// </summary>
        /// <param name="GraphAPIURL">URL for the appropriate Graph API method</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success or failure</returns>
        /// // TODO: check additional needed parameters
        public static bool CallGraphAPIPost(string GraphAPIURL, int Limit, CallBack resultCall, 
            string PostData,
            long AsyncID, long? parent, string parentSNID, 
            bool addToken, bool addDateRange = false)
        {
            try
            {
                string URLToGet = GraphAPIURL;

                // Preprocess postData to allow accents and special characters
                string EncodedPostData = Uri.EscapeDataString(PostData);                

                if (EncodedPostData != "")
                {
                    EncodedPostData = "message=" + EncodedPostData;
                }
                if (addToken)
                {
                    if (Limit == 0)
                    {
                        Limit = DEFAULT_LIMIT;
                    }
                    EncodedPostData += "&access_token=" + FBLogin.token + "&limit=" + Limit.ToString();
                    // TODO: how to add in a more smart way the since / until
                    if (addDateRange)
                    {
                        if (InitialTime == null || EndTime == null)
                        {
                            // TODO: Find why this is happening
                            SetTimeRange(SRBackup.CurrentPeriodStart, SRBackup.CurrentPeriodEnd);
                        }
                        EncodedPostData += "&since=" + InitialTime + "&until=" + EndTime;
                    }
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URLToGet);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                
                // normal encoding
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] byte1 = encoding.GetBytes(EncodedPostData); //postData);
                request.ContentLength = byte1.Length;

                Stream newStream = request.GetRequestStream();
                newStream.Write(byte1, 0, byte1.Length);
                newStream.Close();



                JSONResultCallback state = new JSONResultCallback(request, resultCall, AsyncID, parent, parentSNID);
                // Add request to the list
                requestList.Queue(request);
                IAsyncResult res = request.BeginGetResponse(new AsyncCallback(JSONResultCallback.JSONResponseProcess), state);
                ThreadPool.RegisterWaitForSingleObject(res.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                    request, DEFAULT_TIMEOUT, true);
            }
            catch (Exception ex)
            {
                // TODO: Possibly improve Instrumentation
                System.Diagnostics.Debug.WriteLine("Exception during CallGraphAPI Post: " + ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// General method that makes the HTTP asynchronous request to start the Facebook Graph API call, when it returns JSON records, using Post method
        /// </summary>
        /// <param name="GraphAPIURL">URL for the appropriate Graph API method</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success or failure</returns>
        /// // TODO: consolidate with post
        public static bool CallGraphAPIDelete(string GraphAPIURL, int Limit, CallBack resultCall,
            string PostData,
            long AsyncID, long? parent, string parentSNID,
            bool addToken, bool addDateRange = false)
        {
            try
            {
                string URLToGet = GraphAPIURL;

                // Preprocess postData to allow accents and special characters
                string EncodedPostData = Uri.EscapeDataString(PostData);

                if (EncodedPostData != "")
                {
                    EncodedPostData = "message=" + EncodedPostData;
                }
                if (addToken)
                {
                    if (Limit == 0)
                    {
                        Limit = DEFAULT_LIMIT;
                    }
                    EncodedPostData += "&access_token=" + FBLogin.token + "&limit=" + Limit.ToString();
                    // TODO: how to add in a more smart way the since / until
                    if (addDateRange)
                    {
                        if (InitialTime == null || EndTime == null)
                        {
                            // TODO: Find why this is happening
                            SetTimeRange(SRBackup.CurrentPeriodStart, SRBackup.CurrentPeriodEnd);
                        }
                        EncodedPostData += "&since=" + InitialTime + "&until=" + EndTime;
                    }
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URLToGet);
                request.Method = "DELETE";
                request.ContentType = "application/x-www-form-urlencoded";

                // normal encoding
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] byte1 = encoding.GetBytes(EncodedPostData); //postData);
                request.ContentLength = byte1.Length;

                Stream newStream = request.GetRequestStream();
                newStream.Write(byte1, 0, byte1.Length);
                newStream.Close();



                JSONResultCallback state = new JSONResultCallback(request, resultCall, AsyncID, parent, parentSNID);
                // Add request to the list
                requestList.Queue(request);
                IAsyncResult res = request.BeginGetResponse(new AsyncCallback(JSONResultCallback.JSONResponseProcess), state);
                ThreadPool.RegisterWaitForSingleObject(res.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback),
                    request, DEFAULT_TIMEOUT, true);
            }
            catch (Exception ex)
            {
                // TODO: Possibly improve Instrumentation
                System.Diagnostics.Debug.WriteLine("Exception during CallGraphAPI Post: " + ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// General method that implements HTTP timeout
        /// </summary>
        /// <param name="state">Async HTTP request</param>
        /// <param name="timedOut">Flag that indicates if timer was fired</param>
        public static void TimeoutCallback(object state, bool timedOut)
        {
            // TODO: call callback with appropriate failure
            if (timedOut)
            {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                {
                    // Remove request to the list, moved Dec at the same time
                    requestList.Remove(request.RequestUri.ToString());
                    request.Abort();
                }
            }
            }
        #endregion

    }

    public delegate bool CallBack(int hwnd, bool result, string response, long? parent, string snid);
}
