using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class SNEvent
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
                    string message = "Reading Events from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }
        }


        /// <summary>
        /// Gets event Details given Event ID
        /// </summary>
        /// <param name="conn"></param>
        public void GetFromConnectedDB(SQLiteConnection conn)
        {

            bool GotData = false;

            try
            {

                //Read Event
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, SNID, Description, Location, StartTime, EndTime from EventData where EventID=@ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    //SocialNetwork, SNID, Description, Location, StartTime, EndTime
                    SN = reader.GetInt32(0);
                    var value = reader.GetValue(1);
                    SNID = Convert.ToInt64(value);
                    if (reader.IsDBNull(2))
                        Description = "";
                    else
                        Description = reader.GetString(2);
                    if (reader.IsDBNull(3))
                        Location = "N/A";
                    else
                        Location = reader.GetString(3);
                    if (reader.IsDBNull(4))
                        StartTime = DateTime.Now;
                    else
                        StartTime = reader.GetDateTime(4);
                    if (reader.IsDBNull(5))
                        EndTime = DateTime.Now;
                    else
                        EndTime = reader.GetDateTime(4);
                }

                


                reader.Close();


            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

            if (!GotData)
                throw new Exception("No data available for the Event");
        }



        void GetAttendeesFromDB()
        {
            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    GetAttendeesFromConnectedDB(DBLayer.conn);
                }
                catch (Exception ex)
                {
                    string message = "Reading Event Attendees from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }
        }


        /// <summary>
        /// Gets event Details given Event ID
        /// </summary>
        /// <param name="conn"></param>
        public void GetAttendeesFromConnectedDB(SQLiteConnection conn)
        {


            int status;
            string Name;

            AttendingNames = new List<string>();
            MayBeAttendingNames = new List<string>();
            NotAttendingNames = new List<string>();

            try
            {

                //Read Event
                SQLiteCommand command = new SQLiteCommand("select A.FirstName, A.LastName, B.ActionID from PersonData A, ActionData B WHERE B.WhatSNID=@SNID AND B.ActionID IN (14,15,16) AND A.SNID = B.WhoSNID", conn);
                command.Parameters.Add(new SQLiteParameter("SNID", SNID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    
                    status = reader.GetInt32(2);
                    if(reader.IsDBNull(0))
                        Name = "";
                    else
                        Name = reader.GetString(0); 
                    Name+= " ";
                    if(!reader.IsDBNull(1))
                        Name+= reader.GetString(1);
                    switch (status)
                    {
                        case 14:
                            AttendingNames.Add(Name);
                            break;
                        case 15:
                            MayBeAttendingNames.Add(Name);
                            break;
                        default:
                            NotAttendingNames.Add(Name);
                            break;
                    }
                }




                reader.Close();


            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

        }

        #endregion
    }
}
