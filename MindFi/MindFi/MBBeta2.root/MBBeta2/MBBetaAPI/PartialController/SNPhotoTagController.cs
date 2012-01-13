using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    public partial class SNPhotoTag
    {
        //**************** Constructors
        #region Constructors
        public SNPhotoTag()
        {
        }

        public SNPhotoTag(int IDParam)
        {
            ID = IDParam;
            GetFromDB();
            //if (SNPersonID > 0)
            //    TaggedPerson = new PersonLight(db, SNPersonID);
            //else
            //{
            //    TaggedPerson = new PersonLight();
            //    TaggedPerson.ID = 0;
            //    TaggedPerson.Name = "Unknown";
            //}

        }
        #endregion

        //**************** Attributes
        #region Attributes

        //ID
        public int ID { get; set; }
        public int SN { get; private set; }

        //Tag info
        public Int64 SNPersonID { get; private set; }
        public Int64 SNPhotoID { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public DateTime Date;
        
        //Tagged Person
        public PersonLight TaggedPerson { get; set; }

        #endregion

        //**************** Methods
        #region Methods
        #endregion
    }
}
