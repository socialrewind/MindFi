using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents a Facebook photo album
    /// It contains the parsing for importing data from a JSON response PhotoAlbum object taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/album/
    /// It contains the logic to save to the MyBackup database (AlbumData table)
    /// </summary>
    public class FBAlbum : FBObject
    {
        #region "Properties"

        #region "Standard FB Properties"
        /// <summary>
        /// Social network ID of the user who published the album
        /// </summary>
        public string FromID { get; set; }
        /// <summary>
        /// Name of the user who who published the album
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// Main description of the PhotoAlbum
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Location where album was taken
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// URL of the PhotoAlbum
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Privacy data
        /// </summary>
        public string Privacy { get; set; }
        /// <summary>
        /// Privacy data
        /// </summary>
        public string PrivacyValue { get; set; }
        /// <summary>
        /// Number of photos in the PhotoAlbum
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Disk path to the photo album
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// List of comments associated to the album
        /// </summary>
        public ArrayList Comments
        {
            get { lock (LockObj) { return m_comments; } }
        }
        /// <summary>
        /// Cover Photo
        /// </summary>
        public string CoverPicture;
        /// <summary>
        /// Album Type
        /// </summary>
        private string AlbumType;
        private ArrayList m_comments;
        private ArrayList m_likes;

        #endregion

        #endregion

        #region "Methods"
        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        //public FBAlbum(string response)
        //    : base(response)
        //{
        //    MyDataTable = "AlbumData";
        //    AddParser("comments", "FBPost", ref m_comments);
        //}

        /// <summary>
        /// Default constructor, based on a scanner already in progress. For example, it is used when parsing a user Wall, which contains Post elements
        /// </summary>
        /// <param name="scanner">JSON Scanner already in progress</param>
        public FBAlbum(JSONScanner scanner)
            : base(scanner)
        {
            MyDataTable = "AlbumData";
            AddParser("comments", "FBPost", ref m_comments);
            AddParser("likes", "FBObject", ref m_likes);
        }

        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            base.Save(out ErrorMessage);
            if (Saved)
            {
                Saved = false;
                //System.Windows.Forms.MessageBox.Show("saving album: " + MyPartitionDate + " ID:" + MyPartitionID + " Path: " + Path);
                DBLayer.AlbumDataSave(MyPartitionDate, MyPartitionID,
                FromID, FromName, Description,
                Location, Link, Count, Privacy, PrivacyValue,
                Created, Updated, Path, CoverPicture, AlbumType,
                out Saved, out ErrorMessage);
                lock (LockObj)
                {
                    if (m_comments != null && m_comments.Count > 0)
                    {
                        foreach (FBPost post in m_comments)
                        {
                            string error;
                            post.Save(out error);
                            ErrorMessage += error;
                            if (error != "")
                            {
                                //System.Windows.Forms.MessageBox.Show("error saving comment: " + error);
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
                        case "from":
                            FromID = value;
                            break;
                        default:
                            base.AssignValue(name, value);
                            break;
                    }
                    break;
                case "name":
                    switch (parentName)
                    {
                        case "from":
                            FromName = value;
                            break;
                        default:
                            base.AssignValue(name, value);
                            break;
                    }
                    break;
                case "message":
                case "description":
                    switch (parentName)
                    {
                        case null:
                        case "":
                            Description = value;
                            break;
                        case "privacy":
                            Privacy += value;
                            break;
                    }
                    break;
                case "cover_photo":
                case "picture":
                    CoverPicture = value;
                    break;
                case "link":
                    Link = value;
                    //MessageBox.Show("Parsing Album Link:" + Link);
                    break;
                case "type":
                    AlbumType = value;
                    break;
                case "value":
                case "friends":
                    if (parentName == "privacy")
                    {
                        PrivacyValue += value + ";";
                    }
                    else
                    {
                        base.AssignValue(name, value);
                    }
                    break;
                case "deny":
                    if (parentName == "privacy")
                    {
                        PrivacyValue += " except " + value + ";";
                    }
                    else
                    {
                        base.AssignValue(name, value);
                    }
                    break;
                case "privacy":
                    Privacy += value;
                    break;
                case "can_upload":
                    // Ignore for now
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
                case "count":
                    Count = intValue;
                    break;
                case "id":
                    switch (parentName)
                    {
                        /*
                                    case "from":
                                        FromID = intValue.ToString();
                                        break;
                         */
                        default:
                            base.AssignNumericValue(name, intValue);
                            break;
                    }
                    break;
                default:
                    base.AssignNumericValue(name, intValue);
                    break;
            }
        }

        #endregion

    }
}