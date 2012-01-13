using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    public partial class SNLike
    {
        //**************** Constructors
        #region Constructors
        public SNLike()
        {
        }

        public SNLike(int IDParam )
        {
            ID = IDParam;
            GetFromDB();
        }


        #endregion

        //**************** Attributes
        #region Attributes
        //ID
        public int ID { get; set; }
        public int SN { get; set; }
        public int FromID { get; private set; }
        public Int64 SNFromID { get; private set; }
        public string FromName { get; private set; }
        public string LikedObject { get; private set; }

        //Attributes
        
        

        #endregion

        //**************** Methods
        #region Methods
        #endregion
    }
}
