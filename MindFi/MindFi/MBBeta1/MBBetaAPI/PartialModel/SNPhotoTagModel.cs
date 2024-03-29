﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MBBetaAPI
{
    public partial class SNPhotoTag
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
            using (SQLiteConnection conn = new SQLiteConnection(db.ConnString))
            {
                try
                {
                    conn.Open();
                    GetFromConnectedDB(conn);
                    conn.Close();
                }
                catch (Exception ex)
                {
                    string message = "Reading Photo Tag from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
            }

        }


        /// <summary>
        /// Method for getting photo tag data using internal Tag ID as identifier
        /// </summary>
        /// <param name="conn"></param>
        public void GetFromConnectedDB(SQLiteConnection conn)
        {

            bool GotData = false;

            try
            {

                //Read Tag
                //TODO: Complete person name when available in table.
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, PersonSNID, PhotoSNID, X, Y, Created from TagData where PartitionID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    //SocialNetwork, PersonSNID, PhotoSNID, X, Y, Created
                    SN = reader.GetInt32(0);
                    if (reader.IsDBNull(1))
                        SNPersonID = 0;
                    else
                    {
                        var value = reader.GetValue(1);
                        if (value != "")
                            SNPersonID = Convert.ToInt64(value);
                        else
                            SNPersonID = 0;
                    }
                    var value2 = reader.GetValue(2);
                    SNPhotoID = Convert.ToInt64(value2);
                    value2 = reader.GetValue(3);
                    X = Convert.ToInt32(value2);
                    value2 = reader.GetValue(4);
                    Y = Convert.ToInt32(value2);
                    Date = reader.GetDateTime(5);

                }

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

            if (!GotData)
                throw new Exception("No data available for the photo");
        }

        #endregion       
    }
}
