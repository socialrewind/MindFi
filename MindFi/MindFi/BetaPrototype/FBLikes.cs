using System;
using System.Collections;
using System.Windows.Forms;

namespace MyBackup
{
    /// <summary>
    /// Class that represents a set of Facebook objects
    /// It contains the parsing for a list of Likes
    /// </summary>
    public class FBLikes:FBCollection
    {

        #region "Methods"
        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBLikes(string response, long? ParentID=null, string ParentSNID=null)
            : base(response, "FBPerson", ParentID, ParentSNID)
        {
        }

        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
//System.Windows.Forms.MessageBox.Show ( "Ready to save likes object" );
		Saved = false;
		foreach ( FBPerson who in items )
		{
//System.Windows.Forms.MessageBox.Show ( "Saving " + who.SNID + " who likes " + parentSNID );
		    string error;
		    // save likes relationship
		    DBLayer.ActionDataSave( who.SNID, parentSNID, Verb.LIKE, out Saved, out error);
		    ErrorMessage += error;
		    who.Save(out error);
		    ErrorMessage += error;
		}
        }

        #endregion
    }

}