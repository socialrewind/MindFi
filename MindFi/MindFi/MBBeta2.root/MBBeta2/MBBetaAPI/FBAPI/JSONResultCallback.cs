using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;

namespace MBBetaAPI.AgentAPI
{
    // TODO: Error message management

    /// <summary>
    /// Generic class that process a JSON response
    /// </summary>
    public class JSONResultCallback
    {
        #region "Properties"
        // Members of the request state
        /// <summary>
        /// Web request associated to the call
        /// </summary>
        private HttpWebRequest request;
        /// <summary>
        /// method that is called after the request is  received
        /// </summary>
        private CallBack resultCall;
        /// <summary>
        /// Unique ID in the database (corresponds to AsyncReqQueue)
        /// </summary>
        private long ID;
        /// <summary>
        /// Optional parent request ID
        /// </summary>
        private long? parentID;
        /// <summary>
        /// Optional Social Network ID associated to the request
        /// </summary>
        private string parentSNID;
        #endregion

        #region "Constructors"
        /// <summary>
        /// Creates an async state object for the JSON callback
        /// </summary>
        /// <param name="req">web request associated</param>
        /// <param name="call">method to be called after basic processing</param>
        /// <param name="AsyncID">unique ID in the AsyncReqQueue</param>
        /// <param name="parent">(optional) unique ID of the parent request</param>
        /// <param name="snid">(optional) Social Network ID of the parent object</param>
        public JSONResultCallback(HttpWebRequest req, CallBack call, long AsyncID, long? parent, string snid)
        {
            request = req;
            resultCall = call;
            ID = AsyncID;
            parentID = parent;
            parentSNID = snid;
        }
        #endregion

        /// <summary>
        /// Generic callback method
        /// </summary>
        /// <param name="result">Async state reference</param>
        public static void JSONResponseProcess(IAsyncResult result)
        {
            bool tryOne = false;
            string responseString = "";
            JSONResultCallback state = (JSONResultCallback)result.AsyncState;
            // Get the response.
            try
            {
                HttpWebResponse response = (HttpWebResponse)state.request.EndGetResponse(result);
                // Remove request to the list, moved Dec at the same time
                FBAPI.requestList.Remove(state.request.RequestUri.ToString()); // TODO: access through method
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
                    //System.Windows.Forms.MessageBox.Show("don't have a callblack for ID: " + state.ID + "\nParent: " + state.parentID + "\nResponse: " + responseString);
                }
            }
            catch (ArgumentNullException)
            {
                // Check to Remove request to the list, moved Dec at the same time
                FBAPI.requestList.Remove(state.request.RequestUri.ToString()); // TODO: access through method
                //System.Windows.Forms.MessageBox.Show("No state information");
            }
            catch (Exception ex)
            {
                // Check to Remove request to the list, moved Dec at the same time
                
                //System.Windows.Forms.MessageBox.Show("Error on callblack for:\n" + state.ID + "Error:\n" + ex.ToString());
                FBAPI.requestList.Remove(state.request.RequestUri.ToString()); // TODO: access through method
                DBLayer.RespQueueSave(state.ID, ex.ToString(), AsyncReqQueue.RETRY);
                // prevent probable double call when failure is in the specific function, and not really an error in fileresponseprocess...
                if (!tryOne)
                {
                    if (state.resultCall != null)
                    {
                        state.resultCall(0, false, "\n" + ex.ToString() + "\nRequest: " + state.request.RequestUri.ToString(), state.parentID, state.parentSNID);
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("Error when processing request (with no callback) " + state.ID + "\n" + ex.ToString());
                    }
                }
                else
                {
                    //System.Windows.Forms.MessageBox.Show("Error when processing request " + state.ID + "\n" + ex.ToString());
                }
            }
        }
    }
}
