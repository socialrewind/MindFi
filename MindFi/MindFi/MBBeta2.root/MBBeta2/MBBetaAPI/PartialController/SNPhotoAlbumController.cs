using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    /// <summary>
    /// Describes a Photo Album
    /// </summary>
    public partial class SNPhotoAlbum
    {
        //**************** Constructors
        #region Constructors
        public SNPhotoAlbum()
        { 
        }

        //public SNPhotoAlbum(int IDParam)
        //{
        //    ID = IDParam;
        //}

        /// <summary>
        /// Gets a Photo album from a database, given its ID
        /// </summary>
        /// <param name="db"></param>
        /// <param name="IDParam"></param>
        public SNPhotoAlbum(int IDParam)
        {
            ID = IDParam;
            GetFromDB();
            FillPhotosAndRibbon();
            GetComments();
            GetLikes();
        }

        #endregion

        //**************** Attributes
        #region Attributes
        //ID
        public int ID { get; private set; }
        public int SN { get; private set; }
        public Int64 SNID { get; private set; }
        public string AlbumType { get; private set; }

        //Author
        public Int64 SNFromID { get; private set; }
        public string SNFromName{ get; private set; }

        //Attributes
        public string Name { get; set; }
        public string Description { get; private set; }
        public Uri Link { get; private set; }
        public string Path { get; private set; }
        public int NumberOfPhotos { get; set; }
        public Int64 CoverPicture { get; private set; }
        public DateTime Date { get; private set; }

        //Photos
        public List<SNPhoto> PhotoRibbon { get; set; }
        public List<SNPhoto> Photos { get; set; }
        public int NumberOfPhotosInCache { get; set; }
        public int CurrentPhoto { get; set; }           //1 - index

        //Comments
        public List<WallPost> CommentsList;

        //Likes
        public LikeStructure Likes { get; set; }

        #endregion

        //**************** Methods
        #region Methods

        void GetComments()
        {
            CommentsList = new List<WallPost>();

            List<int> CommentIDs = AgentAPI.DBLayer.GetAlbumCommentIDs(SNID);
            foreach (int CommentID in CommentIDs)
            {
                CommentsList.Add(new WallPost(CommentID, true));
            }
        }

        void GetLikes()
        {
            Likes = new LikeStructure(SNID.ToString());
        }

        #endregion
    }
}
