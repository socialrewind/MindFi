using System;
using System.Collections;

namespace MyBackup
{
    /// <summary>
    /// Class that represents a Facebook contact, including users themselves
    /// It contains the parsing for importing data from a JSON response representing a User object taken using the FB Graph API
    ///     https://developers.facebook.com/docs/reference/api/user/
    /// It contains the logic to save to the MyBackup database (PersonIdentity, PersonData tables)
    /// </summary>
    public class FBPerson : FBObject
    {
        #region "Properties"

        #region "Standard FB Properties"
        /// <summary>
        /// First name for the user in the social network
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Middle name for the user in the social network
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Last name for the user in the social network
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Gender for the user in the social network
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Language for the user in the social network
        /// </summary>
        public string Locale { get; set; }
        /// <summary>
        /// Primary website URL for the user in the social network
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// Alias for the user in the social network
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Timezone registered for the user in the social network
        /// </summary>
        public int? UserTimeZone { get; set; }
        /// <summary>
        /// Check if the user in the social network has been verified
        /// </summary>
        public string Verified { get; set; }
        /// <summary>
        /// Text to appear under profile picture
        /// </summary>
        public string About { get; set; }
        /// <summary>
        /// Bio for the user in the social network
        /// </summary>
        public string Bio { get; set; }
        /// <summary>
        /// Quotes for the user in the social network
        /// </summary>
        public string Quotes { get; set; }
        /// <summary>
        /// Birthday for the user in the social network
        /// </summary>
        public string FullBirthday { get; set; }
        /// <summary>
        /// Education history for the user in the social network
        /// </summary>
        public ArrayList Education;
        /// <summary>
        /// Primary email for the user in the social network
        /// </summary>
        public string EMail { get; set; }
        /// <summary>
        /// ID of the hometown
        /// </summary>
        public string HometownID { get; set; }
        /// <summary>
        /// Name of the hometown
        /// </summary>
        public string HometownName { get; set; }
        /// <summary>
        /// Interested in
        /// </summary>
        public string InterestedIn { get; set; }
        /// <summary>
        /// Meeting For
        /// </summary>
        public string MeetingFor { get; set; }
        /// <summary>
        /// ID of the current city
        /// </summary>
        public string LocationID { get; set; }
        /// <summary>
        /// Name of the current city
        /// </summary>
        public string LocationName { get; set; }
        /// <summary>
        /// Political interest for the user in the social network
        /// </summary>
        public string Political { get; set; }
        /// <summary>
        /// Religion the user declares in the social network
        /// </summary>
        public string Religion { get; set; }
        /// <summary>
        /// Relationship status for the user in the social network
        /// </summary>
        public string RelationshipStatus { get; set; }
        /*
        /// <summary>
        /// ID of the significant other
        /// </summary>
        public string SignificantOtherID { get; set; }
        /// <summary>
        /// Name of the significant other
        /// </summary>
        public string SignificantOtherName { get; set; }
         */
        public FBObject SignificantOther { get; set; }
        /// <summary>
        /// User's personal website
        /// </summary>
        public string Website { get; set; }
        /// <summary>
        /// Work history for the user in the social network
        /// </summary>
        public ArrayList Work;
        /// <summary>
        /// Languages spoken by the user
        /// </summary>
        public ArrayList Languages;
        /// <summary>
        /// Favorite sports
        /// </summary>
        public ArrayList Sports;
        /// <summary>
        /// Favorite teams
        /// </summary>
        public ArrayList FavoriteTeams;
        /// <summary>
        /// Favorite Athletes
        /// </summary>
        public ArrayList FavoriteAthletes;
        /// <summary>
        /// Inspirational people
        /// </summary>
        public ArrayList InspirationalPeople;
        // Data for related org
        private decimal OrgPartitionDate;
        private int OrgPartitionID;
        private JSONParser relatedList;
        #endregion

        /// <summary>
        /// Path to the profile picture (once downloaded)
        /// </summary>
        public string ProfilePic { get; set; }
        #endregion

        #region "Methods"

        /// <summary>
        /// Default constructor, based on a parser stream
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        /// <param name="distance">indicates if the person is the user, a friend or other</param>
        public FBPerson(JSONScanner temp, int distance, JSONParser parent)
            : base(temp)
        {
            relatedList = parent;
            Distance = distance;
            PersonInit();
        }

        /// <summary>
        /// Default constructor, based on a JSON response
        /// </summary>
        /// <param name="response">JSON response to parse and build the person object</param>
        /// <param name="distance">indicates if the person is the user, a friend or other</param>
        public FBPerson(string response, int distance, JSONParser parent)
            : base(response)
        {
            relatedList = parent;
            Distance = distance;
            PersonInit();
        }

        private void PersonInit()
        {
            MyDataTable = "PersonData";
            AddParser("work", "FBWork", ref Work);
            AddParser("education", "FBEducation", ref Education);
            AddParser("languages", "FBObject", ref Languages);
            AddParser("favorite_athletes", "FBObject", ref FavoriteAthletes);
            AddParser("favorite_teams", "FBObject", ref FavoriteTeams);
            AddParser("inspirational_people", "FBObject", ref InspirationalPeople);
            AddParser("sports", "FBObject", ref Sports);
        }

        #endregion

        #region "Internal functions"
        public override void Save(out string ErrorMessage)
        {
            ErrorMessage = "";
            //DBLayer.BeginTransaction();
            base.Save(out ErrorMessage);
            if (Saved)
            {
                Saved = false;
                DBLayer.PersonDataSave(MyPartitionDate, MyPartitionID,
                    Distance, ProfilePic, Link, FirstName, MiddleName, LastName,
                    FullBirthday, UserName, Gender, Locale, RelationshipStatus, 
                    Religion, Political, UserTimeZone, About, Bio, Quotes, 
                    Verified, Updated,
                    out Saved, out ErrorMessage);
                if (Education != null)
                {
                    foreach (FBEducation school in Education)
                    {
                        string error;
                        school.Save(out error);
                        ErrorMessage += error;
                    }
                }
                if (Work != null)
                {
                    foreach (FBWork workplace in Work)
                    {
                        string error;
                        workplace.Save(out error);
                        ErrorMessage += error;
                    }
                }
                if (Languages != null)
                {
                    foreach (FBObject language in Languages)
                    {
                        string error;
                        decimal LangPartitionDate;
                        int LangPartitionID;

                        language.Save(out error);
                        ErrorMessage += error;
                        DBLayer.RelationSave(this.ID, Verb.SPEAK, language.ID,
                            null, null,
                            null, null,
                            out LangPartitionDate, out LangPartitionID,
                            out Saved, out error);

                        ErrorMessage += error;
                    }
                }
                if (FavoriteAthletes != null) FanSave(FavoriteAthletes);
                if (FavoriteTeams != null) FanSave(FavoriteTeams);
                if (InspirationalPeople != null) FanSave(InspirationalPeople);
                if (Sports != null) FanSave(Sports);
                if (relatedList != null && relatedList.ID != -1)
                {
                    string error;
                    DBLayer.RelationSave(this.ID, Verb.ISPARTOF, relatedList.ID, null, null, null, null,
                        out OrgPartitionDate, out OrgPartitionID,
                        out Saved, out error);
                    ErrorMessage += error;
                }
                if (SignificantOther != null)
                {
                    string error;
                    decimal SOPartitionDate;
                    int SOPartitionID;

                    SignificantOther.Save(out error);
                    ErrorMessage += error;
                    DBLayer.RelationSave(this.ID, Verb.SIGNIFICANTOTHERWITH, SignificantOther.ID,
                        RelationshipStatus, null,
                        null, null,
                        out SOPartitionDate, out SOPartitionID,
                        out Saved, out error);

                    ErrorMessage += error;
                }

            }
            else
            {
                // System.Windows.Forms.MessageBox.Show("didnt save person " + ID + " because of\n" + ErrorMessage);
            }
            //DBLayer.CommitTransaction();
        }

        private void FanSave(ArrayList list)
        {
            foreach (FBObject interest in list)
            {
                string error;
                decimal LangPartitionDate;
                int LangPartitionID;

                interest.Save(out error);
                if (error == "")
                {
                    DBLayer.RelationSave(this.ID, Verb.FANOF, interest.ID,
                        null, null,
                        null, null,
                        out LangPartitionDate, out LangPartitionID,
                        out Saved, out error);
                }
            }
        }

        protected override void AssignValue(string name, string value)
        {
            switch (name)
            {
                case "id":
                    switch (parentName)
                    {
                        case "hometown":
                            HometownID += value + ";";
                            break;
                        case "location":
                            LocationID += value + ";";
                            break;
                        case "significant_other":
                            if (SignificantOther == null)
                            {
                                SignificantOther = new FBObject(value, "");
                            }
                            else
                            {
                                SignificantOther.SNID = value;
                            }
                            break;
                        default:
                            base.AssignValue(name, value);
                            break;
                    }
                    break;
                case "name":
                    switch (parentName)
                    {
                        case "hometown":
                            HometownName += value + ";";
                            break;
                        case "location":
                            LocationName += value + ";";
                            break;
                        case "significant_other":
                            if (SignificantOther == null)
                            {
                                SignificantOther = new FBObject("", value);
                            }
                            else
                            {
                                SignificantOther.Name = value;
                            }
                            break;
                        default:
                            base.AssignValue(name, value);
                            break;
                    }
                    break;
                case "first_name":
                    FirstName = value;
                    break;
                case "middle_name":
                    MiddleName = value;
                    break;
                case "last_name":
                    LastName = value;
                    break;
                case "birthday":
                    FullBirthday = value;
                    break;
                case "bio":
                    Bio = value;
                    break;
                case "link":
                    // eliminate internal backslashes, not needed in URL for storage
                    Link = value;
                    break;
                case "relationship_status":
                    RelationshipStatus = value;
                    break;
                case "username":
                    UserName = value;
                    break;
                case "email":
                    EMail = value;
                    break;
                case "website":
                    Website = value;
                    break;
                case "gender":
                    Gender = value;
                    break;
                case "political":
                    Political = value;
                    break;
                case "verified":
                    Verified = value;
                    break;
                case "locale":
                    Locale = value;
                    break;
                case "about":
                    About = value;
                    break;
                case "quotes":
                    Quotes = value;
                    break;
                case "religion":
                    Religion = value;
                    break;
                case "hometown":
                    HometownID += value + ";";
                    break;
                case "location":
                    LocationID += value + ";";
                    break;
                case "significant_other":
                    if (SignificantOther == null)
                    {
                        SignificantOther = new FBObject("", value);
                    }
                    else
                    {
                        SignificantOther.Name = value;
                    }
                    break;
                case "interested_in":
                    InterestedIn = value;
                    break;
                case "meeting_for":
                    MeetingFor = value;
                    break;
                default:
                    base.AssignValue(name, value);
                    break;
            }
        }

        protected override void AssignNumericValue(string name, int intValue)
        {
            switch (name)
            {
                case "timezone":
                    UserTimeZone = intValue;
                    break;
                default:
                    base.AssignNumericValue(name, intValue);
                    break;
            }
        }

        #endregion

    }
}