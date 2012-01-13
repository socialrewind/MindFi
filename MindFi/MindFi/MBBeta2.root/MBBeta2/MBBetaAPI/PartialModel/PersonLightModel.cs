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
                    GetFromConnectedDB(DBLayer.conn);
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
        public void GetFromConnectedDB(SQLiteConnection conn)
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
                        ProfilePic = "Images/nophoto.jpg";
                    else
                        ProfilePic = reader2.GetString(1);


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
                        ProfilePic = "Images/nophoto.jpg";
                    else
                        ProfilePic = reader2.GetString(2);

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


        #endregion

    }
}
