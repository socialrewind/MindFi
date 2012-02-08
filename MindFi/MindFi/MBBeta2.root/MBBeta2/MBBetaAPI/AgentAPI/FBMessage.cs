using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents a Facebook Message or comment
    /// It contains the parsing for importing data from a JSON response representing a Message object taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/Message/
    /// It contains the logic to save to the MyBackup database (Messages table)
    /// </summary>
    public class FBMessage : FBObject
    {
        #region "Properties"

        #region "Standard FB Properties"

        #region "Similar to FBNotification"
        /// <summary>
        /// Social network ID of the user who Messages
        /// </summary>
        public string FromID { get; set; }
        /// <summary>
        /// Name of the user who Messages
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// Email  of the user who Messages
        /// </summary>
        public string FromEmail { get; set; }
        /// <summary>
        /// Social network ID of the users to which the Message is sent
        /// </summary>
        public string ToID { get; set; }
        /// <summary>
        /// Name of the users to which the Message is sent
        /// </summary>
        public string ToName { get; set; }
        /// <summary>
        /// Email of the users to which the Message is sent
        /// </summary>
        public string ToEmail { get; set; }
        /// <summary>
        /// parent reference
        /// </summary>
        public JSONParser parent { get; set; }
        #endregion

        /// <summary>
        /// Main content of the Message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// List of comments associated to the post
        /// </summary>
        public ArrayList Comments
        {
            get { lock (LockObj) { return m_comments; } }
        }
        /// <summary>
        /// Subject of the Message
        /// </summary>
        public string Subject { get; set; }
        ///// <summary>
        ///// Thread: link to previous message
        ///// </summary>
        //public string Previous { get; set; }
        ///// <summary>
        ///// Thread: link to next message
        ///// </summary>
        //public string Next { get; set; }
        #endregion

        #region "Internal similar to FBNotification"
        protected ArrayList m_to;
        #endregion
        protected ArrayList m_comments;

        #endregion

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBMessage(string response)
            : base(response)
        {
            MyDataTable = "MessageData";
            AddParser("comments", "FBMessage", ref m_comments);
            AddParser("to", "FBPerson", ref m_to);
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Message elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBMessage(JSONScanner scanner)
            : base(scanner)
        {
            MyDataTable = "MessageData";
            AddParser("comments", "FBMessage", ref m_comments);
            AddParser("to", "FBPerson", ref m_to);
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        /// <param name="Parent">Post which contains this post / comment</param>
        public FBMessage(JSONScanner scanner, JSONParser Parent)
            : base(scanner)
        {
            parent = Parent;
            MyDataTable = "MessageData";
            AddParser("comments", "FBMessage", ref m_comments);
            AddParser("to", "FBPerson", ref m_to);
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
                DBLayer.MessageDataSave(MyPartitionDate, MyPartitionID,
                    FromID, FromName, FromEmail, ToID, ToName, ToEmail,
                    Message, Subject,
                    Created, Updated, ParentID,
                    out Saved, out ErrorMessage);
                if (!Saved)
                {
                    //System.Windows.Forms.MessageBox.Show("didnt save message " + "-" + MyPartitionDate + "-" + MyPartitionID + " because of error\n" + ErrorMessage);
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
                            dest.Distance = 2;
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
                //System.Windows.Forms.MessageBox.Show("didnt save message " + ID + " because of\n" + ErrorMessage);
            }
        }

        protected override void AssignValue(string name, string value)
        {
            //System.Windows.Forms.MessageBox.Show("message assign value: " + name + " value: " + value );
            switch (name)
            {
                case "id":
                    switch (parentName)
                    {
                        case "from":
                            FromID = value;
                            break;
                        default:
                            base.AssignValue(name, value);
                            break;
                    }
                    break;
                case "name":
                    switch (parentName)
                    {
                        case "from":
                            FromName = value;
                            break;
                        default:
                            base.AssignValue(name, value);
                            break;
                    }
                    break;
                case "email":
                    switch (parentName)
                    {
                        case "from":
                            FromEmail = value;
                            break;
                        default:
                            base.AssignValue(name, value);
                            break;
                    }
                    break;
                case "message":
                    Message = value;
                    break;
                case "subject":
                    Subject = value;
                    break;
                case "previous":
                    Previous = value;
                    break;
                case "next":
                    Next = value;
                    break;
                default:
                    base.AssignValue(name, value);
                    break;
            }
        }
    }
}