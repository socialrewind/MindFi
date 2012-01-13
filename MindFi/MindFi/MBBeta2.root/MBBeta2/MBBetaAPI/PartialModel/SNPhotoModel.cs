using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class SNPhoto
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
                    string message = "Reading Photo from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }

        }

        /// <summary>
        /// Method for getting photo data using internal Phtoo ID as identifier
        /// </summary>
        /// <param name="conn"></param>
        public void GetFromConnectedDB(SQLiteConnection conn)
        {


            bool GotData = false;

            try
            {

                //Read Photo
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, SNID, FromID, FromName, Icon, Source, Link, Height, Width, Created, Updated, Path, ParentID from PhotoData where PhotoID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    //SocialNetwork, SNID, FromID, FromName, Icon, Source, Link, Height, Width, Created, Updated, Path
                    SN = reader.GetInt32(0);
                    var value = reader.GetValue(1);
                    SNID = Convert.ToInt64(value);
                    value = reader.GetValue(2);
                    SNFromID = Convert.ToInt64(value);
                    SNFromName = reader.GetString(3);
                    if (!reader.IsDBNull(4))
                        Icon = new Uri(reader.GetString(4));
                    if (!reader.IsDBNull(5))
                        Source = new Uri(reader.GetString(5));
                    if (!reader.IsDBNull(6))
                        Link = new Uri(reader.GetString(6));
                    Height = reader.GetInt32(7);
                    Width = reader.GetInt32(8);
                    if (reader.IsDBNull(9))
                        Date = reader.GetDateTime(10);
                    else
                        Date = reader.GetDateTime(9);
                    if (reader.IsDBNull(11))
                        Path = "Images/nophoto.jpg";
                    else
                        Path = reader.GetString(11);
                    AlbumID = reader.GetInt32(12);

                }

                reader.Close();

                //Read from Entity
                command = new SQLiteCommand("select Name from Entities where ID=@ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));
                SQLiteDataReader reader2 = command.ExecuteReader();
                while(reader2.Read())
                {
                    //Name
                    if(reader2.IsDBNull(0))
                        Caption = "";
                    else
                        Caption = reader2.GetString(0);
                }

                reader2.Close();

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
