using System;
using System.Collections;

namespace MyBackup
{
    /// <summary>
    /// Class that represents a Facebook Note
    /// It contains the parsing for importing data from a JSON response representing a Note object taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/note/
    /// It contains the logic to save to the MyBackup database (NoteData table), extending MessageData...
    /// </summary>
    public class FBNote : FBMessage
    {
        #region "Properties"

        #region "Standard FB Properties"
        /// <summary>
        /// Note icon
        /// </summary>
        public string Icon { get; set; }
        #endregion
        #endregion

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBNote(string response)
            : base(response)
        {
            MyDataTable = "NoteData";
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Message elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBNote(JSONScanner scanner)
            : base(scanner)
        {
            MyDataTable = "NoteData";
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        /// <param name="Parent">Post which contains this post / comment</param>
        public FBNote(JSONScanner scanner, JSONParser Parent)
            : base(scanner)
        {
            parent = Parent;
            MyDataTable = "NoteData";
        }

        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            base.Save(out ErrorMessage);
            if (Saved)
            {
                Saved = false;
                //System.Windows.Forms.MessageBox.Show("Message Date: " + MyPartitionDate + " ID:" + MyPartitionID);
                string ParentID = null;
                FBObject fbParent = parent as FBObject;
                if (fbParent != null)
                {
                    ParentID = fbParent.SNID;
                }
                DBLayer.NoteDataSave(MyPartitionDate, MyPartitionID,
                    FromID, FromName, FromEmail, ToID, ToName, ToEmail,
                    Message, Subject, Icon,
                    Created, Updated, ParentID, MyDataTable,
                    out Saved, out ErrorMessage);
                if (!Saved)
                {
                    System.Windows.Forms.MessageBox.Show("didnt save note " + ID + "-" + MyPartitionDate + "-" + MyPartitionID + " because of error\n" + ErrorMessage);
                }
                lock (LockObj)
                {
                    if (m_comments != null && m_comments.Count > 0)
                    {
                        // System.Windows.Forms.MessageBox.Show("saving comments for message: " + m_comments.Count );
                        foreach (FBMessage post in m_comments)
                        {
                            string error;
                            post.Save(out error);
                            ErrorMessage += error;
                        }
                    }
                    if (m_to != null && m_to.Count > 0)
                    {
                        //System.Windows.Forms.MessageBox.Show("saving to for message: " + m_to.Count );
                        foreach (FBPerson dest in m_to)
                        {
                            dest.Distance = 2; // TODO: don't update if it was already lower
                            //System.Windows.Forms.MessageBox.Show("person: " + dest.SNID + " email: " + dest.EMail + " name: " + dest.Name);
                            string error;
                            // save likes relationship
                            DBLayer.ActionDataSave(dest.SNID, SNID, Verb.SENTTO, out Saved, out error);
                            ErrorMessage += error;
                            dest.Save(out error);
                            ErrorMessage += error;
                        }
                    }
                }
                // TODO: Change parser to generate likes as user list, then save corresponding relationship	
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("didnt save message " + ID + " because of\n" + ErrorMessage);
            }
        }

        protected override void AssignValue(string name, string value)
        {
            //System.Windows.Forms.MessageBox.Show("message assign value: " + name + " value: " + value );
            switch (name)
            {
                case "icon":
                    Icon = value;
                    break;
                default:
                    base.AssignValue(name, value);
                    break;
            }
        }
    }
}