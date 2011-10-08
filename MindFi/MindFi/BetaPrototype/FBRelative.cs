using System;
using System.Collections;

namespace MyBackup
{
    public class FBRelative:FBObject
    {
        #region "Properties"
        #region "Standard FB Properties"
        /// <summary>
        /// Description of the notification
        /// </summary>
        public string Relationship { get; set; }
        #endregion
        /// <summary>
        /// parent reference
        /// </summary>
        public JSONParser parent { get; set; }
        #endregion

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBRelative(string response)
            : base(response)
        {
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Message elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBRelative(JSONScanner scanner)
            : base(scanner)
        {
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        /// <param name="Parent">Post which contains this post / comment</param>
        public FBRelative(JSONScanner scanner, JSONParser Parent)
            : base(scanner)
        {
            parent = Parent;
        }

        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            base.Save(out ErrorMessage);
            // TODO: save relationship
            decimal RelPartitionDate;
            int RelPartitionID;
            string error = "";
            int tempID = -1;
            if (parent != null)
                tempID = parent.ID;
            DBLayer.RelationSave(this.ID, Verb.RELATIVEOF, tempID, 
                Relationship, null,
                null, null,
                out RelPartitionDate, out RelPartitionID,
                out Saved, out error);
            ErrorMessage += error;
        }

        protected override void AssignValue(string name, string value)
        {
            //System.Windows.Forms.MessageBox.Show("notification assign value: " + name + " value: " + value );
            switch (name)
            {
                case "relationship":
                    Relationship = value;
                    break;
                default:
                    base.AssignValue(name, value);
                    break;
            }
        }

    }
}
