using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents a Facebook Photo
    /// It contains the parsing for importing data from a JSON response representing a Photo object taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/photo/
    /// It contains the logic to save to the MyBackup database (Photos table)
    /// </summary>
    public class FBPhoto : FBObject
    {
        #region "Properties"
        /// <summary>
        /// Social network ID of the user who published the photo
        /// </summary>
        public string FromID { get; set; }
        /// <summary>
        /// Name of the user who who published the photo
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// URL of the Photo as icon
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// URL of the Photo
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Image height
        /// </summary>
        public int? Height { get; set; }
        /// <summary>
        /// Image width
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// URL of the Photo as shown in Facebook
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Path of the Photo when downloaded to disk
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Tags associated to this photo
        /// </summary>
        public ArrayList Tags
        {
            get { lock (LockObj) { return m_tags; } }
        }
        /// <summary>
        /// List of comments associated to the album
        /// </summary>
        public ArrayList Comments
        {
            get { lock (LockObj) { return m_comments; } }
        }
        ///// <summary>
        ///// Link to more comments
        ///// </summary>
        //public string Next { get; set; }
        ///// <summary>
        ///// Link to more comments
        ///// </summary>
        //public string Previous { get; set; }
        #endregion

        private ArrayList m_comments;
        private ArrayList m_tags;
        //private FBAlbum parent;

        #region "Methods"
        /*
        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        public FBPhoto(string response, FBAlbum album)
            : base(response)
        {
        MyDataTable = "PhotoData";
        AddParser("comments", "FBPost", ref m_comments);
        AddParser("tags", "FBTag", ref m_tags);
        //parent = album;
        }
*/

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        //public FBPhoto(JSONScanner scanner, JSONParser album)
        public FBPhoto(JSONScanner scanner, long? ParentID, string ParentSNID)
            : base(scanner)
        {
            MyDataTable = "PhotoData";
            AddParser("comments", "FBPost", ref m_comments);
            AddParser("tags", "FBTag", ref m_tags);
            //parent = album as FBAlbum;
            parentID = ParentID;
            parentSNID = ParentSNID;
        }

        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            base.Save(out ErrorMessage);
            if (Saved)
            {
                Saved = false;
                //System.Windows.Forms.MessageBox.Show("saving photo: " + MyPartitionDate + " ID:" + MyPartitionID + " Link: " + Link);
                DBLayer.PhotoDataSave(MyPartitionDate, MyPartitionID,
                FromID, FromName, Icon, Source, Link, Height, Width,
                Created, Updated, Path, parentID, parentSNID,
                out Saved, out ErrorMessage);
                lock (LockObj)
                {
                    if (m_comments != null && m_comments.Count > 0)
                    {
                        // System.Windows.Forms.MessageBox.Show("saving comments for album: " + m_comments.Count );

                        foreach (FBPost post in m_comments)
                        {
                            string error;
                            post.Save(out error);
                            ErrorMessage += error;
                            if (error != "")
                            {
                                //System.Windows.Forms.MessageBox.Show("error saving comment: " + error );
                            }
                        }
                    }
                    if (m_tags != null && m_tags.Count > 0)
                    {
                        // System.Windows.Forms.MessageBox.Show("saving comments for album: " + m_comments.Count );

                        foreach (FBTag tag in m_tags)
                        {
                            string error;
                            tag.Save(out error);
                            ErrorMessage += error;
                            if (error != "")
                            {
                                //System.Windows.Forms.MessageBox.Show("error saving comment: " + error );
                            }
                        }
                    }
                }
                // TODO: Change parser to generate likes as user list, then save corresponding relationship	
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("didnt save album " + ID + " because of\n" + ErrorMessage);
            }
        }

        protected override void AssignValue(string name, string value)
        {
            switch (name)
            {
                case "id":
                    switch (parentName)
                    {
                        case null:
                        case "":
                            SNID = value;
                            break;
                        case "from":
                            FromID = value;
                            break;
                        default:
                            lastError += "Unknown name ignored: " + parentName + name + "\n";
                            break;
                    }
                    break;
                case "name":
                    switch (parentName)
                    {
                        case null:
                        case "":
                            Name = value;
                            break;
                        case "from":
                            FromName = value;
                            break;
                        default:
                            lastError += "Unknown name ignored: " + parentName + name + "\n";
                            break;
                    }
                    break;
                case "icon":
                    Icon = value;
                    break;
                case "source":
                    Source = value;
                    break;
                case "link":
                    Link = value;
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
                case "height":
                    Height = intValue;
                    break;
                case "width":
                    Width = intValue;
                    break;
                default:
                    base.AssignNumericValue(name, intValue);
                    break;
            }
        }

        #endregion

    }
}