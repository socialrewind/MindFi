using System;
using System.IO;
using System.Net;
using System.Threading;
using System.ComponentModel;

namespace MBBetaAPI.SRAPI
{
    public class SRConnection
    {
        const string CheckURL = "http://www.socialrewind.com/socialrewind/";
        // 60 seconds - no more than once a minute
        static TimeSpan CheckFrequency = new TimeSpan(600000000L);
        private volatile static bool connected = false;
        private static DateTime lastcheck = DateTime.Now.Subtract(CheckFrequency);

        public static bool CheckInternetConnection()
        {
            if (DateTime.Now.Subtract(lastcheck) > CheckFrequency)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_CheckConnection);
                bw.RunWorkerAsync();
                lastcheck = DateTime.Now;
            }
            return connected;
        }

        private static void bw_CheckConnection(object sender, DoWorkEventArgs e)
        {
            try
            {
                string URLToGet = CheckURL;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URLToGet);
                // 5 seconds timeout
                request.Timeout = 5000;
                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
                // TODO: Check something from the response...
            }
            catch (Exception ex)
            {
                // TODO: Possibly improve Instrumentation
                System.Diagnostics.Debug.WriteLine("Exception during CheckInternetConnection: " + ex.ToString());
                connected = false;
            }
            connected = true;
        }
    }
}
