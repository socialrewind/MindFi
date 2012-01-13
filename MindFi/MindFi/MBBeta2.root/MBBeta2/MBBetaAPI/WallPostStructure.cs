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

        public WallPostStructure(int IDParentPost)
        {
            WallPost tempPost;

        
            ParentPost = new WallPost(IDParentPost, true);

            ChildPosts = new List<WallPost>();
            List<int> ChildPostIDs = ParentPost.GetChildPostIDs();

            foreach (int ChildID in ChildPostIDs)
            {
                tempPost = new WallPost(ChildID, false);
                ChildPosts.Add(tempPost);
            }


            ChildPosts.Sort(
                delegate(WallPost p1, WallPost p2)
                {
                    int timecompare = DateTime.Compare((System.DateTime)p1.Date, (System.DateTime)p2.Date);

                    return timecompare;
                }
                );
            
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
