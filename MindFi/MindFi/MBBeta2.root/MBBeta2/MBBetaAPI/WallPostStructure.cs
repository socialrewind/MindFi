using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

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

            ChildPosts = new ObservableCollection<WallPost>();
            ChildPostsList = new List<WallPost>();
            List<int> ChildPostIDs = ParentPost.GetChildPostIDs();

            foreach (int ChildID in ChildPostIDs)
            {
                tempPost = new WallPost(ChildID, false);
                ChildPostsList.Add(tempPost);
            }


            ChildPostsList.Sort(
                delegate(WallPost p1, WallPost p2)
                {
                    int timecompare = DateTime.Compare((System.DateTime)p1.Date, (System.DateTime)p2.Date);

                    return timecompare;
                }
                );

            //Get Observable Collection ready
            foreach (var item in ChildPostsList)
                ChildPosts.Add(item);
        }

        #endregion

        //**************** Attributes
        #region Attributes

        public WallPost ParentPost
        {
            get;
            set;
        }

        private List<WallPost> ChildPostsList { get; set; }

        public ObservableCollection<WallPost> ChildPosts
        {
            get;
            set;
        }

        #endregion

        //**************** Methods
        #region Methods

        public List<WallPost> GetChildPosts(WallPostStructure PE)
        {
            return PE.ChildPostsList;
        }

        public WallPost GetParentPost(WallPostStructure PE)
        {
            return PE.ParentPost;
        }

        #endregion
    }
}
