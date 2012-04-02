using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class SNPhotoAlbum
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
                    string message = "Reading Photo Album from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }

        }

        /// <summary>
        /// Method for getting photo albums data using internal Album ID as identifier
        /// </summary>
        /// <param name="conn"></param>
        public void GetFromConnectedDB(SQLiteConnection conn)
        {

            bool GotData = false;

            try
            {

                //Read Photo Album
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, SNID, FromID, FromName, Description, Link, PhotoCount, Created, Updated, Path, CoverPicture, AlbumType, AllPhotosDownloaded from AlbumData where AlbumID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    //SocialNetwork, SNID, FromID, FromName, Description, Link, PhotoCount, Created, Updated, Path, CoverPicture, AlbumType
                    SN = reader.GetInt32(0);
                    SNID = reader.GetInt64(1);
                    var value = reader.GetValue(2);
                    SNFromID = Convert.ToInt64(value);
                    SNFromName = reader.GetString(3);
                    if (reader.IsDBNull(4))
                        Description = "";
                    else
                        Description = reader.GetString(4);
                    if (!reader.IsDBNull(5))
                        Link = new Uri(reader.GetString(5));
                    NumberOfPhotos = reader.GetInt32(6);
                    if (reader.IsDBNull(7))
                        Date = reader.GetDateTime(8);
                    else
                        Date = reader.GetDateTime(7);
                    if (!reader.IsDBNull(9))
                        Path = AsyncReqQueue.AlbumDestinationDir + reader.GetString(9);
                    if (!reader.IsDBNull(10))
                    {
                        value = reader.GetValue(10);
                        CoverPicture = Convert.ToInt64(value);
                    }
                    AlbumType = reader.GetString(11);
                    //AllPhotosDownloaded
                    if (reader.IsDBNull(12))
                        AllPhotosDownloaded = false;
                    else
                        AllPhotosDownloaded = reader.GetBoolean(12);

                    PhotosToDownlad = !AllPhotosDownloaded;
                }
                reader.Close();

                //Read Entity
                command = new SQLiteCommand("select Name from Entities where ID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader2 = command.ExecuteReader();
                while (reader2.Read())
                {
                    if(reader2.IsDBNull(0))
                        Name = "";
                    else
                        Name = reader2.GetString(0);
                }
                reader2.Close();

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

            if (!GotData)
                throw new Exception(" No data available for the photo album. ");
        }


        void FillPhotosAndRibbon()
        {
            SNPhoto TmpPhoto;

            PhotoRibbon = new List<SNPhoto>();
            Photos = new List<SNPhoto>();
            List<int> PhotoIDs = new List<int>();

            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    SQLiteCommand command = new SQLiteCommand("select PhotoID from PhotoData where ParentID = @ID and Path is not null", DBLayer.conn);
                    command.Parameters.Add(new SQLiteParameter("ID", ID));
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        PhotoIDs.Add(reader.GetInt32(0));
                    }
                }
                catch (Exception ex)
                {
                    APIError error = new APIError(this, ex.Message + " Album ID:  " + ID.ToString(), 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }


            foreach(int PhotoID in PhotoIDs)
            {

                TmpPhoto = new SNPhoto(PhotoID);
                if (TmpPhoto.SNID == CoverPicture)
                {
                    PhotoRibbon.Add(TmpPhoto);
                }
                // TODO: performance optimization, maybe later...
                Photos.Add(TmpPhoto);
             }

        }
        
        #endregion
    }
}
