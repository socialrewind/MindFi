﻿using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
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
            // TODO: review relationship
            /*
            decimal RelPartitionDate;
            int RelPartitionID;
             */
            string error = "";
            FBCollection myparent = parent as FBCollection;
            if ( myparent != null)
            {
                // TODO: how to get the ID for the current User in a safer, less cohesive way
                DBLayer.ActionDataSave(this.SNID, myparent.ParentSNID, Verb.RELATIVEOF, out Saved, out error, Relationship);
            }
                /*
            else
            {
                // TODO: how to get the ID for the current User
                DBLayer.RelationSave(this.ID, Verb.RELATIVEOF, tempID, 
                    Relationship, null,
                    null, null,
                    out RelPartitionDate, out RelPartitionID,
                    out Saved, out error);
            }
                 * */
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
