using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    public partial class Organization: Entity
    {
        //**************** Constructors
        #region Constructors

        public Organization()
        {
        }


        public Organization(int IDParam)
        {
            ID = IDParam;
            GetFromDB();
        }
        #endregion

        //**************** Attributes
        #region Attributes
        //ID inherited from Entity
        //Name inherited from Entity
        public int SN { get; set; }
        public Int64 SNID { get; set; }


        #endregion

        //**************** Methods
        #region Methods
        #endregion

    }
}
