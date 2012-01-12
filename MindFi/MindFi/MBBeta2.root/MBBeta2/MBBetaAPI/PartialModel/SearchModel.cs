using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class Search
    {
        //**************** Constructors
        #region Constructors
        #endregion

        //**************** Attributes
        #region Attributes
        #endregion

        //**************** Methods
        #region Methods

        //Methods for search
        //TODO: Change methods to use FTS on DB. For now, we'll use 'like' keyword. 
        //      Expected to be very slow

        bool SearchMatchingPostIDsFromDB(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            bool success = true;
            SQLiteCommand command;
            SQLiteDataReader reader;
            List<int> PostIDs = new List<int>();

            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();

                    //Search on Posts FTS Table

                    //Search PostIDs (all or with date filter)
                    if (StartDate == DateTime.MinValue)
                    {
                        command = new SQLiteCommand("select PostID from FTSPostData where FTSPostData MATCH @SearchText", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", SearchText));
                    }
                    else
                    {
                        command = new SQLiteCommand("select PostID from FTSPostData WHERE FTSPostData MATCH @SearchText and Created>@start and Created<@end", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", SearchText));
                        command.Parameters.Add(new SQLiteParameter("start", StartDate));
                        command.Parameters.Add(new SQLiteParameter("end", EndDate));
                    }
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PostIDs.Add(reader.GetInt32(0));
                    }
                    reader.Close();

                    var UniquePostIDs = PostIDs.Distinct();

                    foreach (int UniqueID in UniquePostIDs)
                        MatchingPostIDs.Add(UniqueID);

                }
                catch
                {
                    success = false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }
            return success;
        }


        

        bool SearchMatchingAlbumIDsFromDB(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            bool success = true;
            SQLiteCommand command;
            SQLiteDataReader reader;
            List<int> AlbumIDs = new List<int>();

            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();

                    //Read Album IDs from FTS enabled table
                    if (StartDate == DateTime.MinValue)
                    {
                        command = new SQLiteCommand("select AlbumID from FTSAlbumData where FTSAlbumData MATCH @SearchText", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", SearchText));
                    }
                    else
                    {
                        command = new SQLiteCommand("select AlbumID from FTSAlbumData where FTSAlbumData MATCH @SearchText AND Created>@start and Created<@end", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", SearchText));
                        command.Parameters.Add(new SQLiteParameter("start", StartDate));
                        command.Parameters.Add(new SQLiteParameter("end", EndDate));
                    }
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AlbumIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                    var UniqueAlbumIDs = AlbumIDs.Distinct();

                    foreach (int UniqueID in UniqueAlbumIDs)
                        MatchingAlbumIDs.Add(UniqueID);

                }
                catch
                {
                    success = false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }
            return success;

        }


        bool SearchMatchingPhotoIDsFromDB(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            bool success = true;
            SQLiteCommand command;
            SQLiteDataReader reader;
            List<int> PhotoIDs = new List<int>();
            List<string> AlbumPaths = new List<string>();

            //Read PhotoIDs from FTS Table
            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    if (StartDate == DateTime.MinValue)
                    {
                        command = new SQLiteCommand("select PhotoID from FTSPhotoData WHERE FTSPhotoData MATCH @SearchText", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", SearchText));
                    }
                    else
                    {
                        command = new SQLiteCommand("select PhotoID from FTSPhotoData WHERE FTSPhotoData MATCH @SearchText and Created>@start and Created<@end", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", SearchText));
                        command.Parameters.Add(new SQLiteParameter("start", StartDate));
                        command.Parameters.Add(new SQLiteParameter("end", EndDate));
                    }
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PhotoIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                    MatchingPhotoIDs = PhotoIDs;

                    //if (PhotoIDs.Count() > 0)
                    //{

                    //    //Create an dummy album
                    //    SNPhotoAlbum pa = new SNPhotoAlbum(1);
                    //    pa.Name = "Search results for " + SearchText;
                    //    pa.NumberOfPhotos = PhotoIDs.Count();
                    //    //pa.CurrentPhoto = 1;
                    //    pa.Likes = new LikeStructure();
                    //    pa.PhotoRibbon = new List<SNPhoto>();
                    //    pa.Photos = new List<SNPhoto>();

                    //    int i = 0;
                    //    foreach (int PhotoID in PhotoIDs)
                    //    {
                    //        SNPhoto NewPhoto = new SNPhoto(db, PhotoID);
                    //        if (i <= 5)
                    //            pa.PhotoRibbon.Add(NewPhoto);
                    //        pa.Photos.Add(NewPhoto);
                    //        i++;
                    //    }

                    //    MatchingPhotos.Add(pa);
                    //}

                }
                catch
                {
                    success = false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }
            return success;
        }


        bool SearchMatchingMessageIDsFromDB(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            bool success = true;
            SQLiteCommand command;
            SQLiteDataReader reader;
            List<int> MessageIDs = new List<int>();

            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();

                    //Search Messages
                    if (StartDate == DateTime.MinValue)
                    {
                        command = new SQLiteCommand("select MessageID from FTSMessageData where FTSMessageData MATCH @SearchText", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", SearchText));
                    }
                    else
                    {
                        command = new SQLiteCommand("select MessageID from FTSMessageData where FTSMessageData MATCH @SearchText AND Created>@start AND Created<@end", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", SearchText));
                        command.Parameters.Add(new SQLiteParameter("start", StartDate));
                        command.Parameters.Add(new SQLiteParameter("end", EndDate));
                    }
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        MessageIDs.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                        
                    var UniqueMessageIDs = MessageIDs.Distinct();
                    foreach (int UniqueID in UniqueMessageIDs)
                        MatchingMessageIDs.Add(UniqueID);


                }
                catch
                {
                    success = false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }
            return success;
        }


        bool SearchMatchingEventIDsFromDB(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            bool success = true;
            SQLiteCommand command;
            SQLiteDataReader reader;
            List<int> EventIDs = new List<int>();

            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();

                    //Search Entities
                    if (StartDate == DateTime.MinValue)
                    {
                        command = new SQLiteCommand("select EventID from FTSEventData WHERE FTSEventData MATCH @SearchText", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", SearchText));
                    }
                    else
                    {
                        command = new SQLiteCommand("select EventID from FTSEventData WHERE FTSEventData MATCH @SearchText AND EventDate>@start and EventDate<@end", DBLayer.conn);
                        command.Parameters.Add(new SQLiteParameter("SearchText", "%" + SearchText + "%"));
                        command.Parameters.Add(new SQLiteParameter("start", StartDate));
                        command.Parameters.Add(new SQLiteParameter("end", EndDate));
                    }

                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        EventIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                    var UniqueEventIDs = EventIDs.Distinct();
                    foreach (int UniqueID in UniqueEventIDs)
                        MatchingEventIDs.Add(UniqueID);
                    
                }
                catch
                {
                    success = false;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }
            return success;
        }



        #endregion
    }
}
