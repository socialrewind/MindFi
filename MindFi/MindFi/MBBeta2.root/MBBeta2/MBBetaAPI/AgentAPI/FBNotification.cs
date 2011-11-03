using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents a Facebook Notification
    /// It contains the parsing for importing data from a JSON response representing a Message object taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/Notification
    /// It contains the logic to save to the MyBackup database (MessageData table)
    /// </summary>
    public class FBNotification : FBObject
    {
        #region "Properties"

        #region "Similar to FBMessage"
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
        /// Read state for the notification
        /// </summary>
        public int Unread { get; set; }
        /// <summary>
        /// parent reference
        /// </summary>
        public JSONParser parent { get; set; }
        #endregion

        #region "Standard FB Properties"
        /// <summary>
        /// Description of the notification
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Notification link
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Application that produces the notification
        /// </summary>
        public string AppName { get; set; }
        #endregion

        #region "Internal similar to FBMessage"
        private ArrayList m_to;
        #endregion
        
        #endregion


        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBNotification(string response)
            : base(response)
        {
            MyDataTable = "NotificationData";
            AddParser("to", "FBPerson", ref m_to);
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Message elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBNotification (JSONScanner scanner)
            : base(scanner)
        {
            MyDataTable = "NotificationData";
            AddParser("to", "FBPerson", ref m_to);
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        /// <param name="Parent">Post which contains this post / comment</param>
        public FBNotification(JSONScanner scanner, JSONParser Parent)
            : base(scanner)
        {
            parent = Parent;
            MyDataTable = "NotificationData";
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
                DBLayer.NotificationDataSave(MyPartitionDate, MyPartitionID,
                    FromID, FromName, FromEmail, ToID, ToName, ToEmail,
                    Title, Link, AppName, Unread,
                    Created, Updated, ParentID,
                    out Saved, out ErrorMessage);
                if (!Saved)
                {
                    //System.Windows.Forms.MessageBox.Show("didnt save message " + ID + " because of\n" + ErrorMessage);
                }
                lock (LockObj)
                {
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
                //System.Windows.Forms.MessageBox.Show("didnt save notification " + ID + " because of\n" + ErrorMessage);
            }
        }

        protected override void AssignValue(string name, string value)
        {
            //System.Windows.Forms.MessageBox.Show("notification assign value: " + name + " value: " + value );
            switch (name)
            {
                case "title":
                    Title = value;
                    break;
                case "link":
                    Link = value;
                    break;
                case "application":
                    AppName = value;
                    break;
                default:
                    base.AssignValue(name, value);
                    break;
            }
        }

        protected override void AssignNumericValue(string name, int intValue)
        {
            switch (name)
            {
                case "unread":
                    Unread = intValue;
                    break;
                default:
                    base.AssignNumericValue(name, intValue);
                    break;
            }
        }
    }
}