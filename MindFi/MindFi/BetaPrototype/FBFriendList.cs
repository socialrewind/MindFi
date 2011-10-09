using System;
using System.Collections;

namespace MyBackup
{
    /// <summary>
    /// Class that represents a Facebook Friendlist
    /// It contains the parsing for importing data from a JSON response representing a Tag object taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/lists
    /// It contains the logic to save to the MyBackup database (Organizations table)
    /// </summary>
    public class FBFriendList : FBObject
    {
        #region "Properties"

        #region "Standard FB Properties"
        /*
        /// <summary>
        /// List of friends associated to the friendlist
        /// </summary>
        public ArrayList Friends
        {
            get { lock (LockObj) { return m_friends; } }
        }
        private ArrayList m_friends;
        */
        #endregion

        #endregion
        #region "Methods"
        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBFriendList(string response)
            : base(response)
        {
            MyDataTable = "OrganizationData";
            //AddParser("comments", "FBPost", ref m_comments);
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBFriendList(JSONScanner scanner)
            : base(scanner)
        {
            MyDataTable = "OrganizationData";
            // AddParser("comments", "FBPost", ref m_comments);
        }

        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            base.Save(out ErrorMessage);
        }

        #endregion
    }
}
