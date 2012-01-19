using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that keeps a list of pending HttpRequests
    /// </summary>
    public class HttpRequestList
    {
        private ArrayList items = new ArrayList();

        /// <summary>
        /// Default constructor
        /// </summary>
        public HttpRequestList()
        {
        }

        public int Size
        {
            get
            {
                lock (items)
                {
                    if (items == null)
                        return 0;
                    return items.Count;
                }
            }
        }

        #region "Thread safe methods"
        /// <summary>
        /// takes an element out from the queue
        /// </summary>
        /// <returns>First FBObject in the queue</returns>
        public HttpWebRequest Dequeue()
        {
            HttpWebRequest temp;

            lock (items)
            {
                if (items == null || items.Count == 0)
                    return null;

                temp = (HttpWebRequest)items[0];
                items.Remove(temp);
            }
            return temp;
        }

        /// <summary>
        /// gets an object into the queue
        /// 
        /// </summary>
        /// <param name="item">FBobject to queue</param>
        public void Queue(HttpWebRequest item)
        {
            lock (items)
            {
                if (items == null)
                    items = new ArrayList();

                items.Add(item);
            }
        }

        /// <summary>
        /// Removes an item from the queue, without returning it, by ID reference
        /// </summary>
        /// <param name="SNID">Unique ID of the FBObject in the social network</param>
        public void Remove(string RequestURI)
        {
            lock (items)
            {
                if (items == null)
                    return;
                foreach (HttpWebRequest current in items)
                {
                    if (current.RequestUri.ToString() == RequestURI)
                    {
                        items.Remove(current);
                        return;
                    }
                }
            }
        }

        #endregion

    }
}
