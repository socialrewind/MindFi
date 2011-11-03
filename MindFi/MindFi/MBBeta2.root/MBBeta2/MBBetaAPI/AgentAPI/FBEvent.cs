using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents a Facebook Event
    /// It contains the parsing for importing data from a JSON response representing a Post object taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/event/
    /// It contains the logic to save to the MyBackup database (EventData table)
    /// </summary>
    public class FBEvent : FBObject
    {
        #region "Properties"

        #region "Standard FB Properties"
        /// <summary>
        /// Owner of the event
        /// </summary>
        public string OwnerID { get; set; }
        /// <summary>
        /// Owner of the event
        /// </summary>
        public string OwnerName { get; set; }
        /// <summary>
        /// Description of the event
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Start time for the event
        /// </summary>
        public DateTime? StartTime { get { return m_StartTime; } }
        /// <summary>
        /// End time for the event
        /// </summary>
        public DateTime? EndTime { get { return m_EndTime; } }
        /// <summary>
        /// Location for the event
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Location for the event
        /// </summary>
        public string Privacy { get; set; }
        /// <summary>
        /// Venue for the event: street
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// Venue for the event: city
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// Venue for the event: state
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Venue for the event: zip code
        /// </summary>
        public string Zip { get; set; }
        /// <summary>
        /// Venue for the event: country
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Venue for the event: latitude
        /// </summary>
        public string Latitude { get; set; }
        /// <summary>
        /// Venue for the event: longitude
        /// </summary>
        public string Longitude { get; set; }
        /// <summary>
        /// Is the user attending the event?
        /// </summary>
        public string RSVPStatus { get; set; }
        /// <summary>
        /// List of comments associated to the post
        /// </summary>
        public ArrayList Comments
        {
            get { lock (LockObj) { return m_comments; } }
        }
        /// <summary>
        /// parent reference
        /// </summary>
        public JSONParser parent { get; set; }
        #endregion

        private ArrayList m_comments;
        private ArrayList m_to;
        /// <summary>
        /// Internal member for keeping dates
        /// </summary>
        protected DateTime? m_StartTime;
        /// <summary>
        /// Internal member for keeping dates
        /// </summary>
        protected DateTime? m_EndTime;
        #endregion

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBEvent(string response)
            : base(response)
        {
            MyDataTable = "EventData";
            AddParser("comments", "FBMessage", ref m_comments);
            AddParser("owner", "FBPerson", ref m_to);
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Message elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBEvent(JSONScanner scanner)
            : base(scanner)
        {
            MyDataTable = "EventData";
            AddParser("comments", "FBMessage", ref m_comments);
            AddParser("owner", "FBPerson", ref m_to);
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        /// <param name="Parent">Post which contains this post / comment</param>
        public FBEvent(JSONScanner scanner, JSONParser Parent)
            : base(scanner)
        {
            parent = Parent;
            MyDataTable = "EventData";
            AddParser("comments", "FBMessage", ref m_comments);
            AddParser("owner", "FBPerson", ref m_to);
        }

        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            base.Save(out ErrorMessage);
            if (Saved)
            {
                Saved = false;
                //System.Windows.Forms.MessageBox.Show("Event Date: " + MyPartitionDate + " ID:" + MyPartitionID);
                string ParentID = null;
                FBObject fbParent = parent as FBObject;
                if (fbParent != null)
                {
                    ParentID = fbParent.SNID;
                }
                DBLayer.EventDataSave(MyPartitionDate, MyPartitionID,
                Description, Location, StartTime, EndTime,
                Created, Updated, ParentID,
                out Saved, out ErrorMessage);
                if (!Saved)
                {
                    //System.Windows.Forms.MessageBox.Show("didnt save event " + ID + " because of\n" + ErrorMessage);
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
                    /*
                                        if (m_to != null && m_to.Count > 0 )
                                        {
                                //System.Windows.Forms.MessageBox.Show("saving to for message: " + m_to.Count );
                                foreach (FBPerson dest in m_to)
                                            {
                                    dest.Distance = 2; // TODO: don't update if it was already lower
                                    //System.Windows.Forms.MessageBox.Show("person: " + dest.SNID + " email: " + dest.EMail + " name: " + dest.Name);
                                                string error;
                                    // save likes relationship
                                    DBLayer.ActionDataSave( dest.SNID, SNID, Verb.SENTTO, out Saved, out error);
                                    ErrorMessage += error;
                                    dest.Save(out error);
                                    ErrorMessage += error;
                                            }
                                        }
                    */
                }
                // TODO: Change parser to generate likes as user list, then save corresponding relationship	
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("didnt save event " + ID + " because of\n" + ErrorMessage);
            }
        }

        protected override void AssignValue(string name, string value)
        {
            //System.Windows.Forms.MessageBox.Show("event assign value: " + name + " value: " + value );
            string error = "";
            switch (name)
            {
                case "id":
                    switch (parentName)
                    {
                        case "owner":
                            OwnerID = value;
                            break;
                        default:
                            base.AssignValue(name,value);
                            break;
                    }
                    break;
                case "name":
                    switch (parentName)
                    {
                        case "owner":
                            OwnerName = value;
                            break;
                        default:
                            base.AssignValue(name, value);
                            break;
                    }
                    break;
                case "description":
                    Description = value;
                    break;
                case "privacy":
                    Privacy = value;
                    break;
                case "location":
                    Location = value;
                    break;
                case "rsvp_status":
                    RSVPStatus = value;
                    break;
                case "start_time":
                    m_StartTime = DateTimeValue(value, out error);
                    lastError += error;
                    break;
                case "end_time":
                    m_EndTime = DateTimeValue(value, out error);
                    lastError += error;
                    break;
                // Venue fields
                case "street":
                    Street = value;
                    break;
                case "city":
                    City = value;
                    break;
                case "state":
                    State = value;
                    break;
                case "zip":
                    Zip = value;
                    break;
                case "country":
                    Country = value;
                    break;
                case "latitude":
                    Latitude = value;
                    break;
                case "longitude":
                    Longitude = value;
                    break;
                default:
                    base.AssignValue(name, value);
                    break;
            }
        }
    }
}