using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class SNLike
    {
        //**************** Constructors
        #region Constructors
        #endregion

        //**************** Attributes
        #region Attributes
        #endregion

        //**************** Methods
        #region Methods

        void GetFromDB(DBConnector db)
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
                    string message = "Reading Likes from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }
        }


        /// <summary>
        /// Method for getting "Likes" data using internal Tag ID as identifier
        /// </summary>
        /// <param name="conn"></param>
        public void GetFromConnectedDB(SQLiteConnection conn)
        {

            bool GotData = false;

            try
            {

                //Read Like
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, WhoSNID, WhatSNID from ActionData where ActionDataID= @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    //SocialNetwork, WhoSNID, WhatSNID
                    SN = reader.GetInt32(0);
                    
                    var value = reader.GetValue(1);
                    SNFromID = Convert.ToInt64(value);
                    LikedObject = reader.GetString(2);
                }

                reader.Close();

                //Get Name
                command = new SQLiteCommand("select A.Name from Entities A, PersonData B Where A.ID = B.PersonID AND B.SNID=@SNFromID", conn);
                command.Parameters.Add(new SQLiteParameter("SNFromID", SNFromID));

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    //Name
                    FromName = reader.GetString(0);

                }

                reader.Close();

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

            if (!GotData)
                throw new Exception("No data available for the Like");
        }

        #endregion
    }
}
