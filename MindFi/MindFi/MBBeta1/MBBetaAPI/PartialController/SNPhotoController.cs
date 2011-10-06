using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    /// <summary>
    /// Represents a photo uploaded to a Social Network
    /// </summary>
    public partial class SNPhoto
    {

        //**************** Constructors
        #region Constructors
        public SNPhoto()
        {
        }

        public SNPhoto(int IDParam)
        {
            ID = IDParam;
        }

        public SNPhoto(DBConnector db, int IDParam)
        {
            ID = IDParam;
            GetFromDB(db);
            GetTags(db);
            GetComments(db);
            GetLikes(db);
        }

        #endregion

        //**************** Attributes
        #region Attributes
        //ID
        public int ID { get; private set; }
        public int SN { get; private set; }
        public Int64 SNID { get; private set; }
        public int PhotoNumber { get; set; }

        //Author
        public Int64 SNFromID { get; private set; }
        public string SNFromName { get; private set; }

        //Attributes
        public Uri Icon { get; private set; }
        public Uri Source { get; private set; }
        public Uri Link { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public DateTime Date { get; private set; }
        public string Path { get; private set; }
        public string Caption { get; private set; }

        //Album
        public int AlbumID { get; private set; }

        //Tags
        public List<SNPhotoTag> TagsList { get; private set; }

        //Comments
        public List<WallPost> CommentsList { get; private set; }

        //Likes
        public LikeStructure Likes { get; private set; }

        

        #endregion

        //**************** Methods
        #region Methods

        void GetTags(DBConnector db)
        {

            TagsList = new List<SNPhotoTag>();
            
            List<int> TagIDs = db.GetTagIDs(SNID);
            foreach (int TagID in TagIDs)
            {
                TagsList.Add(new SNPhotoTag(db, TagID));
            }
        }


        void GetComments(DBConnector db)
        {

            CommentsList = new List<WallPost>();
            
            List<int> CommentIDs = db.GetPhotoCommentIDs(SNID);
            foreach (int CommentID in CommentIDs)
            {
                CommentsList.Add(new WallPost(db, CommentID));
            }
        }

        void GetLikes(DBConnector db)
        {
            Likes = new LikeStructure(db, SNID.ToString());
        }
        
        #endregion


    }
}
