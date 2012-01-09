using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{

    public partial class PersonLight : Entity
    {

        //**************** Constructors
        #region Constructors
        public PersonLight()
        {
        }

        public PersonLight(DBConnector db, int IDParam)
        {
            ID = IDParam;
            GetFromDB(db);
            Selected = true;
        }

        public PersonLight(DBConnector db, Int64 SNIDPAram)
        {
            SNID = Convert.ToInt64(SNIDPAram);
            GetFromDB2(db);
            Selected = true;
        }
        

        #endregion

        //**************** Attributes
        #region Attributes
        // ID: Inherited
        // Name: Inherited
        public Int64 SNID { get; set; }
        public string ProfilePic { get; set; }
        public int SN {get; private set;}
        public bool Selected {get; set;}

        #endregion

        //**************** Methods
        #region Methods
        #endregion

    }
}
