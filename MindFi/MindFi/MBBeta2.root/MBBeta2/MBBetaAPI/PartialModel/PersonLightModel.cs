using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class PersonLight
    {
        //**************** Constructors
        #region Constructors
        #endregion

        //**************** Attributes
        #region Attributes
        #endregion

        //**************** Methods
        #region Methods

        void GetFromDB()
        {
            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    PersonLightGetFromConnectedDB(DBLayer.conn);
                }
                catch (Exception ex)
                {
                    string message = "Reading Person Light from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }

        }

        /// <summary>
        /// Method for getting person data using internal Person ID as identifier
        /// </summary>
        /// <param name="conn"></param>
        protected void PersonLightGetFromConnectedDB(SQLiteConnection conn)
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
                command = new SQLiteCommand("select SocialNetwork, ProfilePic, SNID from  PersonData where PersonID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader2 = command.ExecuteReader();
                while (reader2.Read())
                {
                    
                    GotData = true;
                    //SocialNetwork, ProfilePic, SNID 
                    SN = reader2.GetInt32(0);
                    if (reader2.IsDBNull(1))
                    {
                        ProfilePic = "Images/nophoto.jpg";
                    }
                    else
                    {
                        ProfilePic = AsyncReqQueue.ProfilePhotoDestinationDir + reader2.GetString(1);
                    }


                    SNID = reader2.GetInt64(2);
                    
                    
                }

                reader2.Close();

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

            if (!GotData)
                throw new Exception("No data available for the person");
        }


        //Methods for getting Person using SNID as identifier
        void GetFromDB2()
        {
            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    GetFromConnectedDB2(DBLayer.conn);
                }
                catch (Exception ex)
                {
                    string message = "Reading Person Light from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }

        }


        public void GetFromConnectedDB2(SQLiteConnection conn)
        {

            bool GotData = false;

            try
            {

                //Read Person
                SQLiteCommand command = new SQLiteCommand("select PersonID, SocialNetwork, ProfilePic from PersonData where SNID = @SNID", conn);
                command.Parameters.Add(new SQLiteParameter("SNID", SNID));

                SQLiteDataReader reader2 = command.ExecuteReader();
                while (reader2.Read())
                {

                    GotData = true;
                    //PersonID, SocialNetwork, ProfilePic
                    ID = reader2.GetInt32(0);
                    SN = reader2.GetInt32(1);
                    if (reader2.IsDBNull(2))
                    {
                        ProfilePic = "Images/nophoto.jpg";
                    }
                    else
                    {
                        ProfilePic = AsyncReqQueue.ProfilePhotoDestinationDir + reader2.GetString(2);
                    }

                }

                reader2.Close();

                //Read Entity
                command = new SQLiteCommand("select Name from Entities where ID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    Name = reader.GetString(0);
                }

                reader.Close();

                

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

            if (!GotData)
                throw new Exception("No data available for the person");
        }

        public static List<PersonLight> GetAllFriends()
        {
            int ID;
            string Name;
            Int64 SNID;
            string ProfilePic;
            int SN;
            List<PersonLight> FriendsList = new List<PersonLight>();

            // TODO: DBLayer modularization
            // Query
            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    SQLiteCommand command = new SQLiteCommand("select A.ID, A.Name, B.SocialNetwork, B.ProfilePic, B.SNID from Entities A, PersonData B where A.Type='MBBetaAPI.AgentAPI.FBPerson' and B.Distance < 2 and A.ID = B.PersonID ", DBLayer.conn);
                    // iterate over query results
                    SQLiteDataReader reader2 = command.ExecuteReader();
                    while (reader2.Read())
                    {
                        ID = reader2.GetInt32(0);
                        if (!reader2.IsDBNull(1))
                        {
                            Name = reader2.GetString(1);
                        }
                        else
                        {
                            // TODO: Localize
                            Name = "Unknown";// should not be null, double check
                        }
                        SN = reader2.GetInt32(2);
                        if (reader2.IsDBNull(3))
                        {
                            ProfilePic = "Images/nophoto.jpg";
                        }
                        else
                        {
                            ProfilePic = AsyncReqQueue.ProfilePhotoDestinationDir + reader2.GetString(3);
                        }
                        if (!reader2.IsDBNull(4))
                        {
                            SNID = reader2.GetInt64(4);
                        }
                        else
                        {
                            SNID = -1; // should not be the normal case
                        }

                        FriendsList.Add(new PersonLight(ID, Name, SN, ProfilePic, SNID));
                    }
                }
                catch
                {
                    APIError error = new APIError(null, "Reading All Friends from DB", 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }
            return FriendsList;
        }

        #endregion

    }
}
