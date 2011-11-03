using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that represents an element in the work history
    /// It contains the parsing for importing data from a JSON response representing a list of friends taken using the FB Graph API 
    ///     https://developers.facebook.com/docs/reference/api/user, work property
    /// It contains the logic to save to the MyBackup database (OrganizationIdentity table)
    /// </summary>
    public class FBWork:FBObject
    {
        #region "Properties"
        public string LocationID { get; set; }
        public string LocationName { get; set; }
        public string PositionID { get; set; }
        public string PositionName { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        #endregion

        #region "Private members"
        private FBPerson relatedPerson;
	private decimal OrgPartitionDate;
	private int OrgPartitionID;
        #endregion

        #region "Methods"
        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="temp">JSON scanner pointed to the current object to parse</param>
        public FBWork(JSONScanner temp, JSONParser parent)
            : base(temp)
        {
	    // should throw if ever the parent is not an FBPerson
	    relatedPerson = (FBPerson) parent;
	    MyDataTable = "OrganizationData";
        }
        #endregion

        #region "Internal functions"
        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
	    base.Save(out ErrorMessage);
	    if ( Saved )
	    {
		Saved = false;
	    	DBLayer.RelationSave(relatedPerson.ID, Verb.WORKAT, this.ID,
			PositionName, Description,
			StartDate, EndDate,
			out OrgPartitionDate, out OrgPartitionID,
			out Saved, out ErrorMessage);
	    } else
	    {
		// System.Windows.Forms.MessageBox.Show(ErrorMessage);
	    }
        }

        protected override void AssignValue(string name, string value)
        {
            switch (name)
            {
                case "id":
                    if (parentName == "employer")
                        SNID = value;
                    else if (parentName == "position")
                        PositionID = value;
                    else if (parentName == "location")
                        LocationID = value;
                    break;
                case "name":
                    if (parentName == "employer")
                        Name = value;
                    else if (parentName == "position")
                        PositionName = value;
                    else if (parentName == "location")
                        LocationName = value;
                    break;
                case "description":
                    Description = value;
                    break;
                case "start_date":
                    StartDate = value;
                    break;
                case "end_date":
                    EndDate = value;
                    break;

                default:
		    base.AssignValue(name, value);
                    break;
            }
        }

	#endregion

    }
}