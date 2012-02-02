using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.ComponentModel;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class Person : INotifyPropertyChanged
    {
        //**************** Constructors
        #region Constructors
        #endregion

        //**************** Attributes
        #region Attributes
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        //**************** Methods
        #region Methods

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        void GetFromDB()
        {
            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    PersonGetFromConnectedDB(DBLayer.conn);
                }
                catch (Exception ex)
                {
                    string message = "Reading Person from DB" + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }

        }


        protected void PersonGetFromConnectedDB(SQLiteConnection conn)
        {


            bool GotData = false;

            try
            {

                //Read Entity
                SQLiteCommand command = new SQLiteCommand("select Name from Entities where ID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    Name = reader.GetString(0);
                }

                reader.Close();

                //Read Person
                command = new SQLiteCommand("select SocialNetwork, SNID, ProfilePic, Link, FirstName, MiddleName, LastName, BirthDay, BirthMonth, BirthYear, About, Bio, Quotes, RelationshipStatus, Distance, DataRequestID, State, RequestType, ResponseValue, SignificantOther from PersonData left outer join RequestsQueue on DataRequestID= RequestsQueue.ID where PersonID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader2 = command.ExecuteReader();
                while (reader2.Read())
                {
                    
                    GotData = true;

                    //SocialNetwork, SNID, ProfilePic, Link, FirstName, MiddleName, LastName
                        SN = reader2.GetInt32(0);
                        SNID = reader2.GetInt64(1);
                        if (reader2.IsDBNull(2))
                        {
                            ProfilePic = "Images/nophoto.jpg";
                        }
                        else
                        {
                            ProfilePic = AsyncReqQueue.ProfilePhotoDestinationDir + reader2.GetString(2);
                        }
                        if (reader2.IsDBNull(3))
                        {
                            SNLink = null;
                        }
                        else
                        {
                            SNLink = new Uri(reader2.GetString(3));
                        }
                        if (reader2.IsDBNull(4))
                        {
                            FirstName = "";
                        }
                        else
                        {
                            FirstName = reader2.GetString(4);
                        }
                        if (reader2.IsDBNull(5))
                        {
                            MiddleName = "";
                        }
                        else
                        {
                            MiddleName = reader2.GetString(5);
                        }
                        if (reader2.IsDBNull(6))
                        {
                            LastName = "";
                        }
                        else
                        {
                            LastName = reader2.GetString(6);
                        }

                        //BirthDay, BirthMonth, BirthYear, About, Bio, Quotes

                        if (reader2.IsDBNull(7))
                        {
                            BirthDay = 0;
                        }
                        else
                        {
                            BirthDay = reader2.GetInt32(7);
                        }
                        if (reader2.IsDBNull(8))
                        {
                            BirthMonth = 0;
                        }
                        else
                        {
                            BirthMonth = reader2.GetInt32(8);
                        }
                        if (reader2.IsDBNull(9))
                        {
                            BirthYear = 0;
                        }
                        else
                        {
                            BirthYear = reader2.GetInt32(9);
                        }
                        if (reader2.IsDBNull(10))
                        {
                            About = "";
                        }
                        else
                        {
                            About = reader2.GetString(10);
                        }
                        if (reader2.IsDBNull(11))
                        {
                            Bio = "";
                        }
                        else
                        {
                            Bio = reader2.GetString(11);
                        }
                        if (reader2.IsDBNull(12))
                        {
                            Quotes = "";
                        }
                        else
                        {
                            Quotes = reader2.GetString(12);
                        }

                        //RelationshipStatus, Distance

                        if (reader2.IsDBNull(13))
                        {
                            RelationshipStatus = "";
                        }
                        else
                        {
                            RelationshipStatus = reader2.GetString(13);
                        }

                        if (reader2.IsDBNull(14))
                        {
                            Distance = 99;
                        }
                        else
                        {
                            Distance = reader2.GetInt32(14);
                        }

                        if (reader2.IsDBNull(15))
                        {
                            DataRequestID = 0;
                        }
                        else
                        {
                            DataRequestID = reader2.GetInt32(15);
                        }
                        if (reader2.IsDBNull(16))
                        {
                            DataRequestState = 0;
                        }
                        else
                        {
                            DataRequestState = reader2.GetInt32(16);
                        }
                        if (reader2.IsDBNull(17))
                        {
                            DataRequestType = "";
                        }
                        else
                        {
                            DataRequestType = reader2.GetString(17);
                        }
                        if (reader2.IsDBNull(18))
                        {
                            DataResponseValue = "";
                        }
                        else
                        {
                            DataResponseValue = reader2.GetString(18);
                        }
                        if (reader2.IsDBNull(19))
                        {
                            SignificantOtherName = null;
                        }
                        else
                        {
                            SignificantOtherName = reader2.GetString(19);
                        }
                }

                reader2.Close();

                // TODO: Double check possible states
                // Parse process, if needed
                switch (DataRequestState)
                {
                    case AsyncReqQueue.RECEIVED:
                        ProcessReceivedRequest();
                        NotifyPropertyChanged("All");
                        break;
                    case AsyncReqQueue.PARSED:
                        if (FirstName == null || FirstName=="")
                        {
                            ProcessReceivedRequest();
                            ErrorMessage += "Appeared already parsed but no special data available";
                            NotifyPropertyChanged("All");
                        }
                        break;
                    case 0:
                        // create the new request and send - prioritize
                        string errorMessage;
                        AsyncReqQueue apiReq = FBAPI.Profile(SNID, ProcessPerson);
                        if (apiReq.QueueAndSend(999))
                        {
                            DBLayer.UpdateDataRequest(ID, apiReq.ID, out errorMessage);
                            if (errorMessage == "")
                            {
                                // TODO: Use ProfilePicRequest state
                                apiReq = FBAPI.ProfilePic(SNID.ToString(),
                                    AsyncReqQueue.ProfilePhotoDestinationDir + SNID.ToString() + ".jpg",
                                    ProcessFriendPic, ID, SNID.ToString());
                                apiReq.QueueAndSend(999);
                                DBLayer.UpdatePictureRequest(ID, apiReq.ID, out errorMessage);

                            }
                            else
                            {
                                ErrorMessage = errorMessage;
                            }
                        }
                        else
                        {
                            // TODO: Localized
                            ErrorMessage = "Cannot request information while disconnected";
                        }
                        break;
                    default:
                        break;
                }
            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }




            if (!GotData)
                throw new Exception("No data available for the person");
        }

        private void ProcessReceivedRequest()
        {
            switch (DataRequestType)
            {
                case "FBPerson":
                    string errorData;
                    FBPerson currentFriend = new FBPerson(DataResponseValue, Distance, null);
                    currentFriend.Parse();
                    // sync data from currentFriend
                    // TODO: Double check all fields are used
                    FirstName = currentFriend.FirstName;
                    LastName = currentFriend.LastName;
                    MiddleName = currentFriend.MiddleName;
                    Name = currentFriend.Name;
                    if (currentFriend.Link != null && currentFriend.Link != "")
                    {
                        SNLink = new Uri(currentFriend.Link);
                    }
                    About = currentFriend.About;
                    Bio = currentFriend.Bio;
                    Quotes = currentFriend.Quotes;
                    RelationshipStatus = currentFriend.RelationshipStatus;
                    string tempDay, tempMonth, tempYear;
                    DBLayer.ProcessFullBirthday(currentFriend.FullBirthday, out tempDay, out tempMonth, out tempYear);
                    int tempBirthDay, tempBirthMonth, tempBirthYear;
                    if (int.TryParse(tempDay, out tempBirthDay))
                    {
                        BirthDay = tempBirthDay;
                    }
                    if (int.TryParse(tempMonth, out tempBirthMonth))
                    {
                        BirthMonth = tempBirthMonth;
                    }
                    if (int.TryParse(tempYear, out tempBirthYear))
                    {
                        BirthYear = tempBirthYear;
                    }
                    // no need to parse, only with internal method
                    //currentFriend.Parsed = true;
                    currentFriend.Save(out errorData);
                    break;
                // TODO: default case and other types for Twitter, LinkedIn
            }
        }


        //Returns a list of organizations related to a person
        //Verb: WORKAT(3) / STUDYAT (4)
        List<RelatedOrganization> GetRelatedOrganizationsFromDB(int Verb)
        {
            var Orgs = new List<RelatedOrganization>();

            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    //Read Entities
                    SQLiteCommand command = new SQLiteCommand("select ObjectID, Adverb, IndirectObject from RelationsData where SubjectID = @ID AND VerbID = @Verb", DBLayer.conn);
                    command.Parameters.Add(new SQLiteParameter("ID", ID));
                    command.Parameters.Add(new SQLiteParameter("Verb", Verb));

                    SQLiteDataReader reader = command.ExecuteReader();
                    int i = 0;
                    while (reader.Read())
                    {
                        Orgs.Add(new RelatedOrganization(reader.GetInt32(0)));
                        if (!reader.IsDBNull(1))
                            Orgs[i].Adverb = reader.GetString(1);
                        if (!reader.IsDBNull(2))
                            Orgs[i].IndirectObject = reader.GetString(2);
                        i++;
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    string message = "Reading List of Organization for Person from DB: " + Type + " : " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }


            //Fill base data needed for organization, not available in Relations Table
            foreach (RelatedOrganization RO in Orgs)
            {
                Organization O = new Organization(RO.ID);
                RO.Name = O.Name;
                RO.SN = O.SN;
                RO.SNID = O.SNID;                
            }

            return Orgs;
        }


        PersonLight GetSignificantOtherFromDB()
        {

            int RelatedID = 0;
            PersonLight SO = null;

            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    //Read Action Data where ActionID = 2 (In relation to...)
                    SQLiteCommand command = new SQLiteCommand("select ObjectID from RelationsData where VerbID=" + Verb.SIGNIFICANTOTHERWITH + " AND SubjectID=@ID", DBLayer.conn);
                    command.Parameters.Add(new SQLiteParameter("ID", ID));

                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var value = reader.GetValue(0);
                        RelatedID = Convert.ToInt32(value);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    string message = "Reading Significant Other for Person from DB: " + Type + " : " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }

            if (RelatedID != 0)
            {
                SO = new PersonLight(RelatedID);
            }

            return SO;
        }

        #endregion
        #region "Single item processing methods"
        /// <summary>
        /// process one friend profile
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public bool ProcessPerson(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (result)
            {
                DataResponseValue = response;
                DataRequestType = "FBPerson";
                DataRequestState = AsyncReqQueue.RECEIVED;
                ProcessReceivedRequest();
                NotifyPropertyChanged("All");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Process a profile picture
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">Filename</param>
        /// <param name="parent">Reference to the friend database ID</param>
        /// <param name="parentSNID">Reference to the friend social network ID</param>
        /// <returns>Request vas processed true/false</returns>
        public bool ProcessFriendPic(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            string errorData = "";

            if (result)
            {
                if (parent != null)
                {
                    if (parent != null && response.IndexOf("jpg") > 0)
                    {
                        ProfilePic = response;
                        DBLayer.UpdateProfilePic((long)parent, response, out errorData);
                        NotifyPropertyChanged("ProfilePic");
                        if (errorData == "") return true;
                    }
                }
                // corrected bug: return an error without mark as failed
                return false;
            }
            return false;
        }

        #endregion

    }
}
