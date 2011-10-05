using System;
using System.Collections;
namespace MyBackup
{
    /// <summary>
    /// Class that represents a Facebook contact, including users themselves
    /// It contains the parsing for importing data from a JSON response representing a User object taken using the FB Graph API
    ///     https://developers.facebook.com/docs/reference/api/user/
    /// It contains the logic to save to the MyBackup database (PersonIdentity, PersonData tables)
    /// </summary>
    public class JSONCollection:JSONParser
    {
        #region "Properties"
        /// <summary>
        /// list of <type>
        /// </summary>
        public ArrayList collectionElements { get; set; }
	/// <summary>
        /// Number of elements in the collection
        /// </summary>
        public int CurrentNumber
        {
            get
            {
                lock (collectionElements)
                {
                    if (collectionElements == null)
                        return 0;
                    return collectionElements.Count;
                }
            }
        }

        public int CurrentlySaved { get; set; }


        #endregion

        #region "Methods"
        /// <summary>
        /// Default constructor, with no source
        /// </summary>
        public JSONCollection():base("")
        {
	    lock ( collectionElements  )
	    {
		collectionElements = new ArrayList();
	    }
        }

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public JSONCollection(string response)
            : base(response)
        {
        }


        #region "Thread safe methods"
        /// <summary>
        /// takes a friend out from the queue
        /// </summary>
        /// <returns>First object parser in the queue</returns>
        public JSONParser Dequeue()
        {
            JSONParser temp;

            lock (collectionElements )
            {
                if (collectionElements == null || collectionElements.Count == 0)
                    return null;

                temp = (JSONParser) collectionElements[0];
                collectionElements.Remove(temp);
            }
            return temp;
        }

        /// <summary>
        /// gets a friend into the queue
        /// 
        /// </summary>
        /// <param name="parser">Parser object to queue</param>
        public void Queue(JSONParser parser)
        {
            lock (collectionElements)
            {
                if (collectionElements == null )
                    collectionElements = new ArrayList();

                collectionElements.Add(parser);
            }
        }


/* TODO: CHECK HOW TO MAKE THIS ONE WORK
        /// <summary>
        /// Removes an element from the queue, without returning it, by ID reference
        /// </summary>
        /// <param name="ID">Unique ID of the object</param>
        public void Remove(string ID)
        {
            lock (collectionElements)
            {
                if (collectionElements == null) 
                    return;
                foreach ( JSONParser current in collectionElements)
                {
                    if ( current.ID == ID )
                    {
                        collectionElements.Remove(current);
                        return;
                    }
                }
            }
        }
*/

        #endregion

        #endregion

    }
}