using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents a Facebook Tag
    /// It contains the parsing for importing data from a JSON response representing a Tag object taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/photo/ (tags)
    /// It contains the logic to save to the MyBackup database (Tags table)
    /// </summary>
    public class FBTag : FBObject
    {
        #region "Properties"
        /// <summary>
        /// Percentage horizontal for the tag center
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// Percentage vertical for the tag center
        /// </summary>
        public float Y { get; set; }

        private FBPhoto parent;

        #endregion

        #region "Methods"
        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBTag(string response, FBPhoto photo)
            : base(response)
        {
            MyDataTable = "TagData";
            parent = photo;
        }

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Tag elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBTag(JSONScanner scanner, JSONParser photo)
            : base(scanner)
        {
            MyDataTable = "TagData";
            parent = photo as FBPhoto;
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
                DBLayer.TagDataSave(this.SNID, parent.SNID, Name, X, Y,
                    Created, Updated,
                    out Saved, out ErrorMessage);
            }
        }

        protected override void AssignNumericValue(string name, float floatValue)
        {
            switch (name)
            {
                case "x":
                    X = floatValue;
                    break;
                case "y":
                    Y = floatValue;
                    break;
                default:
                    base.AssignNumericValue(name, floatValue);
                    break;
            }
        }

        #endregion

    }
}