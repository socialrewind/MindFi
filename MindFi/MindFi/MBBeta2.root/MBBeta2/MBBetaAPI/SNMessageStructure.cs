using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    public class SNMessageStructure
    {

                //**************** Constructors
        #region Constructors
        public SNMessageStructure()
        {
        }

        public SNMessageStructure(DBConnector db, int IDParentMessage)
        {
            SNMessage tempMessage;
            
            ParentMessage = new SNMessage(db, IDParentMessage);
            ChildMessages = new List<SNMessage>();

            List<int> ChildMessageIDs = ParentMessage.GetChildMessageIDs(db);

            //Add the parent message to the conversation
            ChildMessages.Add(ParentMessage);

            foreach (int ChildID in ChildMessageIDs)
            {
                tempMessage = new SNMessage(db, ChildID);
                ChildMessages.Add(tempMessage);
            }
            

        }

        #endregion

        //**************** Attributes
        #region Attributes

        public SNMessage ParentMessage { get; set; }
        public List<SNMessage> ChildMessages { get; set; }

        #endregion

        //**************** Methods
        #region Methods
        #endregion
    }
}
