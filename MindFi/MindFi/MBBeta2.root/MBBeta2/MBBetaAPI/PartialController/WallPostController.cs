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

        public WallPost(int IDParam, bool IsParent)
        {
            ID = IDParam;
            
            GetFromDB();

            if(IsParent)
                GetLikes();

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

        ////Recipient
        //public int InternalToID { get; set; }
        //public string ToID { get; set; }
        public string ToName { get; set; }
        public string WallName { get; set; }

        //Attributes
        public string Title { get; set; }
        public string Message { get; set; }
        public Uri Picture { get; set; }
        public string PictureLink { get; set; } //used to set style. If picture link is empty, don't show picture.
        public Uri Link { get; set; }
        public string Caption { get; set; }
        public string Story { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string PostType { get; set; }
        public string ParentID { get; set; }    //Should be int, of internal PostID, not SNID!

        public string ApplicationName { get; set; }

        public int CommentsCount { get; set; }

        //Likes
        public List<SNLike> LikesList { get; set; }
        public int NumberOfLikes { get; set; }


        public PersonLight FromPerson { get; set; }
        public PersonLight ToPerson { get; set; }

        // Data for async Requests
        public int PostRequestID { get; private set; }
        public int PostRequestState { get; private set; }
        public string PostRequestType { get; private set; }
        public string PostResponseValue { get; private set; }
        public string ErrorMessage { get; private set; }

        #endregion

        //**************** Methods
        #region Methods

        public List<int> GetChildPostIDs()
        {

            List<int> ChildPostIDs;

            ChildPostIDs = ChildPostIDsFromDB();

            return ChildPostIDs;

        }

        int GetNumberOfChildPosts(string ParentPost)
        {
            int NumberOfChilds;

            NumberOfChilds = GetNumberOfChildPostsFromDB(ParentPost);

            return NumberOfChilds;
        }


        void GetLikes()
        {
            LikeStructure Likes = new LikeStructure(SNID.ToString());
            NumberOfLikes = Likes.NumberOfLikes;
            LikesList = Likes.LikesList;
        }

        #endregion
    }
}
