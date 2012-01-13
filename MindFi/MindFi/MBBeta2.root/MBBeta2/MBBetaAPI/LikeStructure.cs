using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MBBetaAPI
{
    public class LikeStructure
    {
        //**************** Constructors
        #region Constructors
        public LikeStructure()
        {
        }

        public LikeStructure(string ObjectIDParam)
        {
            ObjectSNID = ObjectIDParam;
            GetLikes();
        }


        #endregion

        //**************** Attributes
        #region Attributes

        //Object
        public string ObjectSNID { get; private set; }

        //Likes
        public List<SNLike> LikesList { get; private set; }
        public int NumberOfLikes { get; private set; }

        #endregion

        //**************** Methods
        #region Methods

        void GetLikes()
        {

            LikesList = new List<SNLike>();
            NumberOfLikes = 0;

            List<int> LikeIDs = AgentAPI.DBLayer.GetLikeIDs(ObjectSNID);


            foreach (int LikeID in LikeIDs)
            {

                LikesList.Add(new SNLike(LikeID));
                NumberOfLikes++;

            }

            NumberOfLikes = LikeIDs.Count();
        }

        #endregion
    }
}
