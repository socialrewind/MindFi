using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;

namespace MBBetaAPI.AgentAPI
{
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
            //bool inFlightwasDecremented = false;

            FileResultCallback state = (FileResultCallback)result.AsyncState;
            // get response
            try
            {
                HttpWebResponse response = (HttpWebResponse)state.request.EndGetResponse(result);
                // Check to Remove request to the list, moved Dec at the same time

                // only decrement if passed EndGetResponse, so to prevent double decrement on timeout
                // Remove request to the list
                FBAPI.requestList.Remove(state.request.RequestUri.ToString()); // TODO: access through method
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
                // Check to Remove request to the list, moved Dec at the same time

                //System.Windows.Forms.MessageBox.Show("No state information");
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("Error2 on file callblack for:\n" + state.ID + "Error:\n" + ex.ToString());
                // Not just a timeout; example case was to update profile pictures when they are in use
                FBAPI.requestList.Remove(state.request.RequestUri.ToString()); // TODO: access through method
                DBLayer.RespQueueSave(state.ID, ex.ToString(), AsyncReqQueue.FAILED);
                // prevent probable double call when failure is in the specific function, and not really an error in fileresponseprocess...
                if (!tryOne)
                {
                    state.resultCall(0, false, "\n" + ex.ToString() + "\nRequest: " + state.request.RequestUri.ToString(), state.parentID, state.parentSNID);
                }
            }
        }
    }
}
