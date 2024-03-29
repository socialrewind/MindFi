﻿using System;
using System.Collections;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Represents a row in the Education list (School object under Education node in FB)
    /// </summary>
    public class FBEducation : FBObject
    {
        #region "Properties"
        public string YearID { get; set; }
        public string YearName { get; set; }
        public string DegreeID { get; set; }
        public string DegreeName { get; set; }
        public string ConcentrationID { get; set; }
        public string ConcentrationName { get; set; }
        public string Type { get; set; }
        #endregion

        #region "Private members"
        private FBPerson relatedPerson;
        private decimal YearPartitionDate;
        private int YearPartitionID;
        private ArrayList m_with;
        private ArrayList m_concentration;

        #endregion

        #region "Methods"
        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="temp">JSON scanner pointed to the current object to parse</param>
        public FBEducation(JSONScanner temp, JSONParser parent)
            : base(temp)
        {
            // should throw if ever the parent is not an FBPerson
            relatedPerson = (FBPerson)parent;
            MyDataTable = "OrganizationData";
            AddParser("with", "FBPerson", ref m_with);
            AddParser("concentration", "FBObject", ref m_concentration);
        }
        #endregion

        #region "Internal functions"
        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            base.Save(out ErrorMessage);
            if (Saved)
            {
                Saved = false;
                DBLayer.RelationSave(relatedPerson.ID, Verb.STUDYAT, this.ID,
                    Type, (DegreeName != null) ? DegreeName + " " : ""
                    + ((ConcentrationName != null) ? ConcentrationName : ""),
                    YearID, null,
                    out YearPartitionDate, out YearPartitionID,
                    out Saved, out ErrorMessage);
                // save relation of who studied with
                if (m_with != null)
                {
                    foreach (FBPerson coworker in m_with)
                    {
                        string error;
                        decimal cPartitionDate;
                        int cPartitionID;

                        coworker.Save(out error);
                        ErrorMessage += error;
                        DBLayer.RelationSave(relatedPerson.ID, Verb.STUDIEDWITH, coworker.ID,
                            "@", this.Name,
                            null, null,
                            out cPartitionDate, out cPartitionID,
                            out Saved, out error);

                        ErrorMessage += error;
                    }
                }
                // save relation of what was studied
                if (m_concentration != null)
                {
                    foreach (FBObject what in m_concentration)
                    {
                        string error;
                        decimal wPartitionDate;
                        int wPartitionID;

                        what.Save(out error);
                        ErrorMessage += error;
                        DBLayer.RelationSave(relatedPerson.ID, Verb.STUDIEDWHAT, what.ID, 
                            "@", this.Name,
                            null, null,
                            out wPartitionDate, out wPartitionID,
                            out Saved, out error);

                        ErrorMessage += error;
                    }
                }
            }
            else
            {
                // System.Windows.Forms.MessageBox.Show(ErrorMessage);
            }
        }

        protected override void AssignValue(string name, string value)
        {
            switch (name)
            {
                case "id":
                    if (parentName == "school")
                        SNID = value;
                    else if (parentName == "year")
                        YearID = value;
                    else if (parentName == "degree")
                        DegreeID = value;
                    else if (parentName == "concentration")
                        ConcentrationID += value + ",";
                    else
                    {
                        // DEBUG CODE
                        string error = "Possible bug parsing id: parent " + parentName + " is unexpected";
                    }
                    break;
                case "name":
                    if (parentName == "school")
                        Name = value;
                    else if (parentName == "year")
                        YearName = value;
                    else if (parentName == "degree")
                        DegreeName = value;
                    else if (parentName == "concentration")
                        ConcentrationName += value + ",";
                    else
                    {
                        // DEBUG CODE
                        string error = "Possible bug parsing name: parent " + parentName + " is unexpected";
                    }
                    break;
                case "type":
                    Type = value;
                    break;
                case "concentration":
                    // seems something to ignore for now
                    break;
                default:
                    base.AssignValue(name, value);
                    break;
            }
        }

        #endregion

    }
}