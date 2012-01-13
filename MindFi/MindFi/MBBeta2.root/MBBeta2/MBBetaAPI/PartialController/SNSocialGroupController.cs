using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    public partial class SNSocialGroup
    {
        //**************** Constructors
        #region Constructors
        public SNSocialGroup()
        {
        }

        public SNSocialGroup(int IDParam)
        {
            ID = IDParam;
            GetFromDB();
            GetMembers();
        }
        #endregion

        //**************** Attributes
        #region Attributes
        //ID
        public int ID { get; set; }
        public string Name { get; set; }

        //Members
        public List<int> MemberIDs { get; set; }
        public List<string> MemberNames { get; set; }

    
        #endregion

        //**************** Methods
        #region Methods
        #endregion
    }
}
