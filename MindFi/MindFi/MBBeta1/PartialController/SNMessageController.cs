using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    /// <summary>
    /// Class representing a Social Netowrok private Message
    /// </summary>
    public partial class SNMessage
    {
        //**************** Constructors
        #region Constructors
        public SNMessage()
        {
        }

        public SNMessage(DBConnector db, int IDParam)
        {
            ID = IDParam;
            GetFromDB(db);
            FromPerson = new PersonLight(db, Convert.ToInt64(SNFromID));
            //ToPerson = new PersonLight(db, SNToID);
            FromProfilePic = FromPerson.ProfilePic;
        }

        #endregion

        //**************** Attributes
        #region Attributes

        // ID
        public int ID { get; set; }
        public int SN { get; set; }
        public string SNID { get; set; }

        //Sender
        public string SNFromID { get; set; }
        public string SNFromName { get; set; }
        public PersonLight FromPerson { get; set; }
        public string FromProfilePic { get; set; }


        //Recipient
        public string SNToID { get; set; }
        public string SNToName { get; set; }
        public PersonLight ToPerson { get; set; }

        //Attributes
        public string Subject { get; set; }
        public string MessageText { get; set; }
        public DateTime Date { get; set; }

        #endregion

        //**************** Methods
        #region Methods

        public List<int> GetChildMessageIDs(DBConnector db)
        {

            List<int> ChildMessageIDs;

            ChildMessageIDs = ChildMessageIDsFromDB(db);

            return ChildMessageIDs;

        }

        #endregion
    }
}
