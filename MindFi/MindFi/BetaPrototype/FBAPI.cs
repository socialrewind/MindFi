using System;
using System.IO;
using System.Net;
using System.Threading;

namespace MyBackup
{
    /// <summary>
    /// Class that contains methods to call the Facebook Graph API
    /// </summary>
    public class FBAPI
    {
        private const string FBGraphAPIURL = "https://graph.facebook.com/";
        public const int DEFAULT_TIMEOUT = 30000; // 30 sec timeout

        #region "Methods"

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
        public static AsyncReqQueue Friends(string SNID, int Limit, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBFriends",
                FBGraphAPIURL + SNID + "/friends", Limit,
                resultCall);
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
        public static AsyncReqQueue Wall(string Who, int Limit, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBWall",
                FBGraphAPIURL + Who + "/feed",
                Limit, resultCall);
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
                FBGraphAPIURL + Who + "/notifications",
                Limit, resultCall);
            return me;
        }

        /// <summary>
        /// Gets multiple FB events, by user ID
        /// </summary>
        /// <param name="Who">Alias of the desired user</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Events(string Who, int Limit, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBEvents",
                FBGraphAPIURL + Who + "/events",
                Limit, resultCall);
            return me;
        }

        /// <summary>
        /// Gets multiple FB events, by user ID
        /// </summary>
        /// <param name="SNID">SNID of Event in Facebook</param>
        /// <param name="Limit">How many JSON records should be returned, max</param>
        /// <param name="resultCall">Function that is called once the wall records are parsed. Reference to the callback method that will process the response asynchronously, following Callback async prototype</param>
        /// <returns>Success/Failure</returns>
        public static AsyncReqQueue Event(string SNID, CallBack resultCall)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBEvent",
                FBGraphAPIURL + SNID,
                1, resultCall, true);
            return me;
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
                Limit, resultCall);
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
                Limit, resultCall);
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
                Limit, resultCall, true, Parent, Album);
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
                Limit, resultCall, true, Parent, SNID);
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
                Limit, resultCall, true, Parent, SNID);
            return me;
        }

        // TODO: Refactor and modularize all similar functions
        public static AsyncReqQueue Members(string SNID, int Limit, CallBack resultCall, int? Parent)
        {
            AsyncReqQueue me = new AsyncReqQueue("FBFriendList",
                FBGraphAPIURL + SNID + "/members",
                Limit, resultCall, true, Parent, SNID);
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
                Limit, resultCall, true, Parent, SNID);
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
                Limit, resultCall, true, Parent, SNID);
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
                Limit, resultCall, true, Parent, SNID);
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
                URL,
                Limit, resultCall, false);
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
        /// TODO: Private?
        public static bool CallGraphAPI(string GraphAPIURL, int Limit, CallBack resultCall, long AsyncID, long? parent, string parentSNID, bool addToken)
        {
            try
            {
                string URLToGet = GraphAPIURL;
                if (addToken)
                    URLToGet += "?access_token=" + FBLogin.token + "&limit=" + Limit.ToString();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URLToGet);

                JSONResultCallback state = new JSONResultCallback(request, resultCall, AsyncID, parent, parentSNID);
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
        /// TODO: Private?
        public static bool CallGraphAPI(string GraphAPIURL, string FileName, CallBack resultCall, long AsyncID, long? parent, string parentSNID)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GraphAPIURL);
                // Start the asynchronous operation.    
                FileResultCallback state = new FileResultCallback(request, FileName, resultCall, AsyncID, parent, parentSNID);
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
        /// TODO: Private?
        public static bool DownloadLink(string Link, string FileName, CallBack resultCall, long AsyncID, long? parent, string parentSNID)
        {
            try
            {
                // Asynchronous Get
                // Create a new HttpWebRequest object.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Link);
                // Start the asynchronous operation.    
                FileResultCallback state = new FileResultCallback(request, FileName, resultCall, AsyncID, parent, parentSNID);
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
                    request.Abort();
                }
            }
        }
        #endregion

    }


    #region "Callback classes"

    public delegate bool CallBack(int hwnd, bool result, string response, long? parent, string snid);

    public class JSONResultCallback
    {
        HttpWebRequest request;
        CallBack resultCall;
        long ID;
        long? parentID;
        string parentSNID;

        public JSONResultCallback(HttpWebRequest req, CallBack call, long AsyncID, long? parent, string snid)
        {
            request = req;
            resultCall = call;
            ID = AsyncID;
            parentID = parent;
            parentSNID = snid;
        }

        public static void JSONResponseProcess(IAsyncResult result)
        {
            bool tryOne = false;
            string responseString = "";
            JSONResultCallback state = (JSONResultCallback)result.AsyncState;
            // get response

            // Get the response.
            try
            {
                HttpWebResponse response = (HttpWebResponse)state.request.EndGetResponse(result);
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse);
                responseString = streamRead.ReadToEnd();
                // Close the stream object.
                streamResponse.Close();
                streamRead.Close();
                tryOne = true;
                DBLayer.RespQueueSave(state.ID, responseString);

                // call second level callback
                if (state.resultCall != null)
                {
                    state.resultCall(0, true, responseString, state.parentID, state.parentSNID);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("don't have a callblack for ID: " + state.ID + "\nParent: " + state.parentID + "\nResponse: " + responseString);
                }
            }
            catch (ArgumentNullException)
            {
                //System.Windows.Forms.MessageBox.Show("No state information");
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Error on callblack for:\n" + state.ID + "Error:\n" + ex.ToString());
                DBLayer.RespQueueSave(state.ID, ex.ToString(), AsyncReqQueue.RETRY);
                // prevent probable double call when failure is in the specific function, and not really an error in fileresponseprocess...
                if (!tryOne)
                {
                    if (state.resultCall != null)
                    {
                        state.resultCall(0, false, "\n" + ex.ToString() + "\nRequest: " + state.request.RequestUri.ToString(), state.parentID, state.parentSNID);
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("don't have a callblack for ID: " + state.ID + "\nParent: " + state.parentID + "\nResponse: " + responseString);
                }
            }
        }
    }

    public class FileResultCallback
    {
        HttpWebRequest request;
        CallBack resultCall;
        string filename;
        long ID;
        long? parentID;
        string parentSNID;

        public FileResultCallback(HttpWebRequest req, string Filename, CallBack call, long AsyncID, long? parent, string snid)
        {
            request = req;
            filename = Filename;
            resultCall = call;
            ID = AsyncID;
            parentID = parent;
            parentSNID = snid;
        }

        public static void FileResponseProcess(IAsyncResult result)
        {
            bool tryOne = false;

            FileResultCallback state = (FileResultCallback)result.AsyncState;
            // get response
            try
            {
                HttpWebResponse response = (HttpWebResponse)state.request.EndGetResponse(result);
                Stream streamResponse = response.GetResponseStream();
                FileStream output = new FileStream(state.filename, FileMode.Create);
                byte[] buffer = new byte[1000];
                int Length;
                do
                {
                    Length = streamResponse.Read(buffer, 0, 1000);
                    output.Write(buffer, 0, Length);
                } while (streamResponse.CanRead && Length != 0);
                output.Close();
                streamResponse.Close();
                tryOne = true;

                DBLayer.RespQueueSave(state.ID, state.filename);

                // call second level callback
                state.resultCall(0, true, state.filename, state.parentID, state.parentSNID);
            }
            catch (WebException wex)
            {
                // only get the link
                //System.Windows.Forms.MessageBox.Show("Error on file callblack for:\n" + state.ID + "Error:\n" + wex.ToString());
                DBLayer.RespQueueSave(state.ID, wex.ToString(), AsyncReqQueue.RETRY);
                if (!tryOne)
                {
                    state.resultCall(0, false, "\nFailed to get file: " + state.request.RequestUri.ToString(), state.parentID, state.parentSNID);
                }
            }
            catch (ArgumentNullException)
            {
                //System.Windows.Forms.MessageBox.Show("No state information");
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Error2 on file callblack for:\n" + state.ID + "Error:\n" + ex.ToString());
                //DBLayer.RespQueueSave(state.ID, ex.ToString(), AsyncReqQueue.RETRY);
                // prevent probable double call when failure is in the specific function, and not really an error in fileresponseprocess...
                if (!tryOne)
                {
                    state.resultCall(0, false, "\n" + ex.ToString() + "\nRequest: " + state.request.RequestUri.ToString(), state.parentID, state.parentSNID);
                }
            }
        }
    }

    #endregion

}