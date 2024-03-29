﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

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
                    string message = "Reading Photo Album from DB: " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
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
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, SNID, FromID, FromName, Description, Link, PhotoCount, Created, Updated, Path, CoverPicture, AlbumType from AlbumData where AlbumID = @ID", conn);
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
                        Path = reader.GetString(9);
                    if (!reader.IsDBNull(10))
                    {
                        value = reader.GetValue(10);
                        CoverPicture = Convert.ToInt64(value);
                    }
                    AlbumType = reader.GetString(11);
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


        void FillPhotosAndRibbon(DBConnector db)
        {
            SNPhoto TmpPhoto;

            PhotoRibbon = new List<SNPhoto>();
            Photos = new List<SNPhoto>();
            List<int> PhotoIDs = new List<int>();
            
            try
            {

                using (SQLiteConnection conn = new SQLiteConnection(db.ConnString))
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand("select PhotoID from PhotoData where ParentID = @ID", conn);
                    command.Parameters.Add(new SQLiteParameter("ID", ID));
                    SQLiteDataReader reader = command.ExecuteReader();
                
                    while (reader.Read())
                    {
                        PhotoIDs.Add(reader.GetInt32(0));
                    }

                    conn.Close();
                }
            }
            catch(Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " Album ID:  " + ID.ToString(), 1);
            }


            int i = 0;
            foreach(int PhotoID in PhotoIDs)
            {

                TmpPhoto = new SNPhoto(db, PhotoID);
                //tmp.PhotoPath = AlbumPath + "\\" + tmp.ID.ToString() + ".jpg";

                if (i < 5)
                {
                    PhotoRibbon.Add(TmpPhoto);
                    Photos.Add(TmpPhoto);
                }
                else
                    Photos.Add(TmpPhoto);

                i++;

             }

        }
        
        #endregion
    }
}
