using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

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
                    string message = "Reading Photo Tag from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
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
                //TODO: Complete person name when available in table for efficiency
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, PersonSNID, PhotoSNID, X, Y, Created, PersonName from TagData where TagDataID = @ID", conn);
                /*
                SQLiteCommand command = new SQLiteCommand("select TagData.SocialNetwork, TagData.PersonSNID, TagData.PhotoSNID, TagData.X, TagData.Y, TagData.Created, Name"
                                        + " from TagData left outer join PersonData on PersonSNID = PersonData.SNID left outer join Entities on PersonData.PersonID = Entities.ID where TagDataID = @ID", conn);
                 * */
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    //SocialNetwork, PersonSNID, PhotoSNID, X, Y, Created
                    SN = reader.GetInt32(0);
                    if (reader.IsDBNull(1))
                    {
                        SNPersonID = 0;
                    }
                    else
                    {
                        var value = reader.GetValue(1);
                        if ((string)value != "")
                            SNPersonID = Convert.ToInt64(value);
                        else
                            SNPersonID = 0;
                    }
                    var value2 = reader.GetValue(2);
                    SNPhotoID = Convert.ToInt64(value2);
                    var value3 = reader.GetValue(3);
                    X = Convert.ToInt32(value3);
                    var value4 = reader.GetValue(4);
                    Y = Convert.ToInt32(value4);
                    var Date = reader.GetDateTime(5);
                    if (reader.IsDBNull(6))
                    {
                        PersonName = "Unknown";
                    }
                    else
                    {
                        var value7 = reader.GetValue(6);
                        PersonName = value7.ToString();
                    }
                }

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

            // TODO: Revisit cases that don't have a tag
            if (!GotData)
                throw new Exception("No data available for the photo");
        }

        #endregion       
    }
}
