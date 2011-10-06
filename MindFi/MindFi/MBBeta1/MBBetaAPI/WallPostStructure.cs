using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    public class WallPostStructure
    {
        //**************** Constructors
        #region Constructors

        public WallPostStructure(DBConnector db, int IDParentPost)
        {
            WallPost tempPost;

            ParentPost = new WallPost(db, IDParentPost);
            ChildPosts = new List<WallPost>();

            List<int> ChildPostIDs = ParentPost.GetChildPostIDs(db);

            foreach (int ChildID in ChildPostIDs)
            {
                tempPost = new WallPost(db, ChildID);
                ChildPosts.Add(tempPost);
            }
        }

        #endregion

        //**************** Attributes
        #region Attributes

        public WallPost ParentPost { get; set; }
        public List<WallPost> ChildPosts { get; set; }

        #endregion

        //**************** Methods
        #region Methods

        public List<WallPost> GetChildPosts(WallPostStructure PE)
        {
            return PE.ChildPosts;
        }

        public WallPost GetParentPost(WallPostStructure PE)
        {
            return PE.ParentPost;
        }

        #endregion
    }
}
