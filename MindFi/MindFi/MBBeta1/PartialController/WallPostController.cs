using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    public partial class WallPost
    {
        //**************** Constructors
        #region Constructors
        public WallPost()
        {
        }

        public WallPost(DBConnector db, int IDParam)
        {
            ID = IDParam;
            GetFromDB(db);
            GetLikes(db);

            //TODO: Check if Person ID exists. If yes, add Person Light
        }


        #endregion

        //**************** Attributes
        #region Attributes

        //ID
        public int ID { get; set; }
        public int SN { get; set; }
        public string SNID { get; set; }

        //Author
        public int InternalFromID { get; set; }
        public string FromID { get; set; }
        public string FromName { get; set; }
        public string FromPhotoPath { get; set; }

        //Recipient
        public int InternalToID { get; set; }
        public string ToID { get; set; }
        public string ToName { get; set; }

        //Attributes
        public string Title { get; set; }
        public string Message { get; set; }
        public Uri Picture { get; set; }        
        public Uri Link { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string PostType { get; set; }
        public string ParentID { get; set; }    //Should be int, of internal PostID, not SNID!

        public int CommentsCount { get; set; }

        //Likes
        public List<SNLike> LikesList { get; set; }
        public int NumberOfLikes { get; set; }


        public PersonLight FromPerson { get; set; }
        public PersonLight ToPerson { get; set; }


        #endregion

        //**************** Methods
        #region Methods

        public List<int> GetChildPostIDs(DBConnector db)
        {

            List<int> ChildPostIDs;

            ChildPostIDs = ChildPostIDsFromDB(db);

            return ChildPostIDs;

        }

        int GetNumberOfChildPosts(DBConnector db, string ParentPost)
        {
            int NumberOfChilds;

            NumberOfChilds = GetNumberOfChildPostsFromDB(db, ParentPost);

            return NumberOfChilds;
        }


        void GetLikes(DBConnector db)
        {
            LikeStructure Likes = new LikeStructure(db, SNID.ToString());
            NumberOfLikes = Likes.NumberOfLikes;
            LikesList = Likes.LikesList;
        }

        #endregion
    }
}
