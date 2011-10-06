using System;
using System.Collections;
using System.Windows.Forms;

namespace MyBackup
{
    /// <summary>
    /// Class that represents a set of Facebook objects
    /// It contains the parsing for importing a list of some specific type
    /// </summary>
    public class FBCollection : JSONParser
    {
        #region "Properties"
        /// <summary>
        /// List of posts/persons/etc. contained in the set/wall
        /// </summary>
        public ArrayList items;
        /// <summary>
        /// Paging: link to previous set in FB to parse
        /// </summary>
        public string Previous { get; set; }
        /// <summary>
        /// Paging: link to next set in FB to parse
        /// </summary>
        public string Next { get; set; }
        /// <summary>
        /// Name of the children type, e.g. FBPerson, FBPost, FBMessage, ...
        /// </summary>
        public string itemType;
        /// <summary>
        /// Number of elements in the current collection
        /// </summary>
        public int CurrentNumber
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

        /// <summary>
        /// Number of elements that have been saved, useful for async progress when calling Save
        /// </summary>
        public int CurrentlySaved { get; set; }
        #endregion

        #region "Methods"
        /*
        /// <summary>
        /// Default constructor, with no source
        /// </summary>
        public FBCollection(string type):base("")
        {
            items = new ArrayList();
        itemType = type;
        AddParser("data", itemType, ref items);
        }
*/

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBCollection(string response, string type, long? ParentID = null, string ParentSNID = null)
            : base(response)
        {
            items = new ArrayList();
            itemType = type;
            parentID = ParentID;
            parentSNID = ParentSNID;
            AddParser("data", itemType, ref items);
        }

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBCollection(JSONScanner scanner, string type, long? ParentID = null, string ParentSNID = null)
            : base(scanner)
        {
            items = new ArrayList();
            itemType = type;
            parentID = ParentID;
            parentSNID = ParentSNID;
            AddParser("data", itemType, ref items);
        }

        public override void Save(out string ErrorMessage)
        {
            string error = "";
            CurrentlySaved = 0;

            // save array
            if (items != null)
            {
                try
                {
                    DBLayer.BeginTransaction();
                    foreach (FBObject item in items)
                    {
                        string error2;
                        item.Save(out error2);
                        CurrentlySaved++;
                        error += error2;
                    }
                    // TODO: save relationship to parent...

                    DBLayer.CommitTransaction();
                }
                catch (Exception ex)
                {
                    DBLayer.RollbackTransaction();
                    error += "Error saving to the database: " + ex.ToString() + "\n";
                }
            }
            ErrorMessage = error;
        }

        protected override void AssignValue(string name, string value)
        {
            switch (name)
            {
                case "previous":
                    // eliminate internal backslashes, not needed in URL for storage
                    value = value.Replace("\\", "");
                    Previous = value;
                    break;
                case "next":
                    // eliminate internal backslashes, not needed in URL for storage
                    value = value.Replace("\\", "");
                    Next = value;
                    break;
                default:
                    base.AssignValue(name, value);
                    break;
            }
        }
        #endregion

        #region "Thread safe methods"
        /// <summary>
        /// takes an element out from the queue
        /// </summary>
        /// <returns>First FBObject in the queue</returns>
        public FBObject Dequeue()
        {
            FBObject temp;

            lock (items)
            {
                if (items == null || items.Count == 0)
                    return null;

                temp = (FBObject)items[0];
                items.Remove(temp);
            }
            return temp;
        }

        /// <summary>
        /// gets an object into the queue
        /// 
        /// </summary>
        /// <param name="item">FBobject to queue</param>
        public void Queue(FBObject item)
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
        public void Remove(string SNID)
        {
            lock (items)
            {
                if (items == null)
                    return;
                foreach (FBObject current in items)
                {
                    if (current.SNID == SNID)
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