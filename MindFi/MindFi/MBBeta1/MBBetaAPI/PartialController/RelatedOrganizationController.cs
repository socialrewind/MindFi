using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    /// <summary>
    /// Gets organization with extended relationship data
    /// </summary>
    public partial class RelatedOrganization : Organization
    {
        //**************** Constructors
        #region Constructors

        public RelatedOrganization(int IDPAram)
        {
            ID = IDPAram;
        }

        
        #endregion

        //**************** Attributes
        #region Attributes
        //Entity Attributes + Organization Attributes + 
        public string Adverb { get; set; }
        public string IndirectObject { get; set; }

        #endregion

        //**************** Methods
        #region Methods
        #endregion
    }
}
