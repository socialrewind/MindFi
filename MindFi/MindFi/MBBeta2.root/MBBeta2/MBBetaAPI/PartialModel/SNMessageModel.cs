﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class SNMessage
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
                    string message = "Reading Message from DB" + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }

        }


        void GetFromConnectedDB(SQLiteConnection conn)
        {

            bool GotData = false;

            try
            {

                //Read Entity
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, SNID, FromID, FromName, ToID, ToName, Message, Subject, Created, Updated from MessageData where MessageID = @MessageID", conn);
                command.Parameters.Add(new SQLiteParameter("MessageID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    //SocialNetwork, SNID, FromID, FromName, ToID, ToName, Message, Subject, Created, Updated
                    SN = reader.GetInt32(0);
                    var result = reader.GetValue(1);
                    SNID = result.ToString();
                    SNFromID = reader.GetString(2);
                    SNFromName = reader.GetString(3);
                    if(!reader.IsDBNull(4))
                        SNToID = reader.GetString(4);
                    if (!reader.IsDBNull(5))
                        SNToName = reader.GetString(5);
                    if (reader.IsDBNull(6))
                        MessageText = "";
                    else
                        MessageText = reader.GetString(6);
                    if (reader.IsDBNull(7))
                        Subject = "";
                    else
                        Subject = reader.GetString(7);
                    if (reader.IsDBNull(8))
                        Date = reader.GetDateTime(9);
                    else
                        Date = reader.GetDateTime(8);                    

                }

                reader.Close();

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }


            if (!GotData)
                throw new Exception("No data available for the message< ");
        }


        private List<int> ChildMessageIDsFromDB()
        {


            List<int> ChildMessageIDs = new List<int>();

            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();

                    SQLiteCommand command = new SQLiteCommand("select MessageID from MessageData where ParentID=@ParentMessage", DBLayer.conn);
                    command.Parameters.Add(new SQLiteParameter("ParentMessage", SNID));
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ChildMessageIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    APIError error = new APIError(this, "Getting Child Messages: " + ex.Message + " ID:  " + ID.ToString(), 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }



            return ChildMessageIDs;

        }

        #endregion
    }
}
