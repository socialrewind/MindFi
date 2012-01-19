using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;

namespace MBBetaAPI.AgentAPI
{
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
                // only decrement if passed EndGetResponse, so to prevent double decrement on timeout
                // Remove request to the list, moved Dec at the same time
                FBAPI.requestList.Remove(state.request.RequestUri.ToString()); // TODO: access through method
                //FBAPI.DecInFlight();
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

                //System.Windows.Forms.MessageBox.Show("No state information");
            }
            catch (Exception ex)
            {
                // Check to Remove request to the list, moved Dec at the same time

                //System.Windows.Forms.MessageBox.Show("Error on callblack for:\n" + state.ID + "Error:\n" + ex.ToString());
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
