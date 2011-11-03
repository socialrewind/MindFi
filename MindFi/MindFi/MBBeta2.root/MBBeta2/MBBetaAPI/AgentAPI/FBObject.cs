using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents a Facebook entity, such as places (Hometown or Location) or
    /// unknown people, teams, etc.
    /// </summary>
    public class FBObject : JSONParser
    {
        #region "Properties"
        /// <summary>
        /// SNID for the object
        /// </summary>
        public string SNID { get; set; }
        /// <summary>
        /// Time the object was first created in the social network
        /// </summary>
        public DateTime? Created { get { return m_created; } }
        /// <summary>
        /// Time the object was last updated in the social network
        /// </summary>
        public DateTime? Updated { get { return m_updated; } }
        /// <summary>
        /// data table associated with the object
        /// </summary>
        protected string MyDataTable { get; set; }
        #endregion

        #region "internal members"
        /// <summary>
        /// YYYYMMDD, date for the version record
        /// </summary>
        protected decimal MyPartitionDate;
        /// <summary>
        /// consecutive for the version record
        /// </summary>
        protected int MyPartitionID;
        /// <summary>
        /// Object for locking access to internal members
        /// </summary>
        protected static volatile Object LockObj = new Object();
        /// <summary>
        /// Internal member for keeping dates
        /// </summary>
        protected DateTime? m_created;
        /// <summary>
        /// Internal member for keeping dates
        /// </summary>
        protected DateTime? m_updated;
        #endregion

        /// <summary>
        /// Default constructor, based on a scanner already in progress (parent object)
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        /// <param name="distance">indicates if the person is the user, a friend or other</param>
        public FBObject(JSONScanner scanner)
            : base(scanner)
        {
        }

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBObject(string response)
            : base(response)
        {
        }

        /// <summary>
        /// Specific constructor, based on properties
        /// </summary>
        public FBObject(string ID, string name)
            : base("")
        {
            SNID = ID;
            Name = name;
        }

        #region "Internal functions"
        /// <summary>
        /// Persist the object into disk
        /// </summary>
        /// <param name="ErrorMessage">Returns a description of any error during save</param>
        public override void Save(out string ErrorMessage)
        {
            if (SNID == null)
            {
                Saved = false;
                ErrorMessage = "Cannot save object that couldn't be parsed";
                return;
            }

            ErrorMessage = "";
            // new case: an FBObject with no additional attributes, doesn't require additional save
            if (MyDataTable != null)
            {
                // Check if data already exists, if not chained save
                int currentID;
                if (DBLayer.VerifyExists(SocialNetwork.FACEBOOK, SNID, MyDataTable, out currentID))
                {
                    ID = currentID;
                    Saved = true;
                }
                else
                {
                    base.Save(out ErrorMessage);
                }
            }
            else
            {
                base.Save(out ErrorMessage);
            }
            if (Saved && MyDataTable != null)
            {
                Saved = false;
                DBLayer.DataSave(ID, SocialNetwork.FACEBOOK, SNID, MyDataTable,
                out MyPartitionDate, out MyPartitionID, out Saved, out ErrorMessage);
            }
        }

        /// <summary>
        /// Parses a value to an internal field
        /// </summary>
        /// <param name="name">String that describes the JSON variable</param>
        /// <param name="name">String that describes the JSON value, after preprocessing</param>
        protected override void AssignValue(string name, string value)
        {
            string error;

            switch (name)
            {
                case "id":
                    SNID = value;
                    break;
                case "name":
                    Name = value;
                    break;
                case "created_time":
                    m_created = DateTimeValue(value, out error);
                    lastError += error;
                    break;
                case "updated_time":
                    m_updated = DateTimeValue(value, out error);
                    lastError += error;
                    break;
                default:
                    base.AssignValue(name, value);
                    break;
            }
        }

        /// <summary>
        /// Parses a value into a DateTime
        /// </summary>
        /// <param name="name">String that describes the JSON variable</param>
        /// <param name="name">String that describes the JSON value, after preprocessing</param>
        protected DateTime? DateTimeValue(string value, out string error)
        {
            DateTime? result = null;
            error = "";

            if (value.Length > 19)
            {
                value = value.Substring(0, 19);
            }
            DateTime temp;
            if (DateTime.TryParse(value, out temp))
            {
                result = temp;
            }
            else
            {
                error = "error converting Updated date: " + value + "\n";
            }
            return result;
        }

        #endregion
    }
}