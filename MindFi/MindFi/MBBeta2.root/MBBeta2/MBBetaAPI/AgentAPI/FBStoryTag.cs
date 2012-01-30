using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents a Facebook Tag in a wall post
    /// It contains the parsing for importing data from a JSON response representing a story Tag object taken using the FB Graph API 
    /// It contains the logic to save to the MyBackup database (StoryTags table)
    /// </summary>
    public class FBStoryTag : FBObject
    {
        #region "Properties"
        /// <summary>
        /// Percentage horizontal for the tag center
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// Percentage vertical for the tag center
        /// </summary>
        public int Length { get; set; }

        private FBPost parent;

        #endregion

        #region "Methods"
        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBStoryTag(string response, FBPost post)
            : base(response)
        {
            MyDataTable = "StoryTagData";
            parent = post;
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Tag elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBStoryTag(JSONScanner scanner, JSONParser post)
            : base(scanner)
        {
            MyDataTable = "StoryTagData";
            parent = post as FBPost;
        }


        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            //System.Windows.Forms.MessageBox.Show("saving tag: person " + SNID + " PhotoID:" + parent.SNID);

            Saved = false;
            FBObject person = new FBObject(SNID, Name);
            person.Save(out ErrorMessage);
            if (person.Saved)
            {
                Saved = false;
                DBLayer.StoryTagDataSave(this.SNID, parent.SNID, Offset, Length,
                    Created, Updated,
                    out Saved, out ErrorMessage);
            }
        }

        protected override void AssignNumericValue(string name, int intValue)
        {
            switch (name)
            {
                case "offset":
                    Offset = intValue;
                    break;
                case "length":
                    Length = intValue;
                    break;
                default:
                    base.AssignNumericValue(name, intValue);
                    break;
            }
        }

        #endregion

    }
}