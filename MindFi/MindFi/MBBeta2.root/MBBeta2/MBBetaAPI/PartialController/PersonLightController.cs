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

        public PersonLight(int IDParam)
        {
            ID = IDParam;
            GetFromDB();
            Selected = true;
        }

        public PersonLight(Int64 SNIDPAram)
        {
            SNID = Convert.ToInt64(SNIDPAram);
            GetFromDB2();
            Selected = true;
        }
        
        public PersonLight(int id, string name, int sn, string profilePic, Int64 snid)
        {
            ID = id;
            Name = name;
            SN = sn;
            ProfilePic = profilePic;
            SNID = snid;
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
