using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that contains methods to manage the SQLite database
    /// </summary>
    public partial class DBLayer
    {
        //**************** Methods
        #region Methods

        public static List<int> GetFriendIDs()
        {
            List<int> PIDs = new List<int>();

            lock (obj)
            {
                DatabaseInUse = true;
                try
                {
                    GetConn();
                    //Distance 1 = Friend
                    SQLiteCommand command = new SQLiteCommand("select A.ID from Entities A, PersonData B where A.Type='MBBetaAPI.AgentAPI.FBPerson' and B.Distance < 2 and A.ID = B.PersonID ", conn);
                    //SQLiteCommand command = new SQLiteCommand("select A.ID from Entities A left outer join PersonData B where A.Type='MBBetaAPI.AgentAPI.FBPerson' and A.ID = B.PersonID ", conn);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                }
                catch
                {
                    APIError error = new APIError(null, "Reading Person IDs from DB", 1);
                }
                finally
                {
                    DatabaseInUse = false;
                }
            }

            return PIDs;
        }

        public static List<int> GetFriendOfFriendsIDs()
        {
            List<int> PIDs = new List<int>();

            lock (obj)
            {
                DatabaseInUse = true;
                try
                {
                    GetConn();
                    SQLiteCommand command = new SQLiteCommand("select A.ID from Entities A, PersonData B where A.Type='MBBetaAPI.AgentAPI.FBPerson' and B.Distance=2 and A.ID = B.PersonID ", conn);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                }
                catch
                {
                    APIError error = new APIError(null, "Reading Friends Of Friends IDs from DB", 1);
                }
                finally
                {
                    DatabaseInUse = false;
                }
            }

            return PIDs;
        }

        public static List<int> GetSocialGroupIDs()
        {
            List<int> PIDs = new List<int>();

            lock (obj)
            {
                DatabaseInUse = true;
                try
                {
                    GetConn();
                    SQLiteCommand command = new SQLiteCommand("select ID from Entities where Type='MBBetaAPI.AgentAPI.FBFriendList'", conn);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                }
                catch
                {
                    APIError error = new APIError(null, "Reading Social Group IDs from DB", 1);
                }
                finally
                {
                    DatabaseInUse = false;
                }
            }

            return PIDs;
        }

        /// <summary>
        /// Gets a Post List from DB, limited by start and end dates, offset, and record count
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="Offset"></param>
        /// <param name="Limit"></param>
        /// <returns></returns>
        public static List<int> GetPosts(DateTime start, DateTime end, int Offset, int Limit)
        {
            List<int> PIDs = new List<int>();

            lock (obj)
            {
                DatabaseInUse = true;
                try
                {
                    GetConn();
                    SQLiteCommand command = new SQLiteCommand("select PostID from PostData where ParentID is NULL and Created>=@start and Created<@end ORDER BY Created DESC LIMIT @OFFSET,@LIMIT", conn);
                    command.Parameters.Add(new SQLiteParameter("start", start));
                    command.Parameters.Add(new SQLiteParameter("end", end));
                    command.Parameters.Add(new SQLiteParameter ("OFFSET", Offset));
                    command.Parameters.Add(new SQLiteParameter ("LIMIT", Limit));
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                }
                catch
                {
                    APIError error = new APIError(null, "Reading Post IDs from DB", 1);
                }
                finally
                {
                    DatabaseInUse = false;
                }
            }

            return PIDs;
        }

        /// <summary>
        /// Gets Posts given a list of persons and a date range
        /// </summary>
        /// <param name="start">Initial day</param>
        /// <param name="end">End day + 1 (so posts of the end day are included up to 23:59:59, using <)</param>
        /// <param name="PersonList"></param>
        /// <returns></returns>
        public static List<int> GetPostsByPersonID(DateTime start, DateTime end, List<long> PersonList)
        {
            List<int> PostIDs = new List<int>();
            string IDList = "";
            int i = 0;

            foreach(long PersonSNID in PersonList)
            {
                IDList += PersonList[i].ToString();
                if (i < (PersonList.Count - 1))
                    IDList += ", ";
                i++;
            }

            //If there is people in the list
            if (i > 0)
            {
                lock (obj)
                {
                    DatabaseInUse = true;
                    try
                    {
                        GetConn();

                        string textcommand = "select PostID from PostData where ParentID is NULL and FromID in(";
                        textcommand += IDList;
                        textcommand += ") and Created>=@start and Created<@end";

                        SQLiteCommand command = new SQLiteCommand(textcommand, conn);
                        command.Parameters.Add(new SQLiteParameter("start", start));
                        command.Parameters.Add(new SQLiteParameter("end", end));
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PostIDs.Add(reader.GetInt32(0));
                        }

                        reader.Close();

                        //Read posts where person made a comment 
                        List<string> CommentIDs = new List<string>();
                        textcommand = "select A.PostID from PostData A, PostData B where B.FromID in (";
                        textcommand += IDList;
                        textcommand += ") AND B.ParentID IS NOT NULL and B.Created>=@start and B.Created<@end AND B.ParentID = A.SNID";

                        command = new SQLiteCommand(textcommand, conn);
                        command.Parameters.Add(new SQLiteParameter("start", start));
                        command.Parameters.Add(new SQLiteParameter("end", end));
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PostIDs.Add(reader.GetInt32(0));
                        }

                        reader.Close();
                    }
                    catch
                    {
                        APIError error = new APIError(null, "Reading Post IDs from List of Persons in DB", 1);
                    }
                    finally
                    {
                        DatabaseInUse = false;
                    }
                }
            }



            return PostIDs;
        }

        /// <summary>
        /// Gets the number of posts in DB, inside a time range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int CountPosts(DateTime start, DateTime end)
        {
            int count = 0;

            lock (obj)
            {
                DatabaseInUse = true;
                try
                {
                    GetConn();
                    SQLiteCommand command = new SQLiteCommand("select count (*) from PostData where ParentID is NULL and Created>=@start and Created<@end", conn);
                    command.Parameters.Add(new SQLiteParameter("start", start));
                    command.Parameters.Add(new SQLiteParameter("end", end));
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        count = reader.GetInt32(0);
                    }

                    reader.Close();
                }
                catch
                {
                    APIError error = new APIError(null, "Reading Count Post IDs from DB", 1);
                }
                finally
                {
                    DatabaseInUse = false;
                }
            }

            return count;
        }

        public static List<int> GetSNParentMessageIDs(DateTime start, DateTime end, List<PersonLight> SelectedPeople)
        {
            int i, total;
            total = SelectedPeople.Count();

            List<int> ParentMessageIDs = new List<int>();

            for (i = 0; i < total; i++)
            {
                lock (obj)
                {
                    DatabaseInUse = true;
                    try
                    {
                        GetConn();

                        SQLiteCommand command = new SQLiteCommand("select distinct MessageID from MessageData where ParentID IS NULL and FromID=@FromID and ((Updated>=@start and Updated<@end) OR (Created>=@start and Created<@end))", conn);
                        command.Parameters.Add(new SQLiteParameter("FromID", SelectedPeople[i].SNID));
                        command.Parameters.Add(new SQLiteParameter("start", start));
                        command.Parameters.Add(new SQLiteParameter("end", end));
                        SQLiteDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            //GotData = true;
                            ParentMessageIDs.Add(reader.GetInt32(0));
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        APIError error = new APIError(null, ex.Message + "Reading Parent Message IDs from DB", 1);
                    }
                    finally
                    {
                        DatabaseInUse = false;
                    }
                }
            }

            return ParentMessageIDs;
        }

        public static List<int> GetAlbumIDs(DateTime start, DateTime end, List<PersonLight> SelectedPeople)
        {
            

            int i, total;
            total = SelectedPeople.Count();

            List<int> AlbumIDs = new List<int>();

            for (i = 0; i < total; i++)
            {
                lock (obj)
                {
                    DatabaseInUse = true;
                    try
                    {
                        GetConn();

                        //TODO: This query must change to look into PhotoData Table, according to people appearing in Photos
                        SQLiteCommand command = new SQLiteCommand("select distinct AlbumID from AlbumData where FromID=@FromID and ((Updated>=@start and Updated<@end) OR (Created>=@start and Created<@end))", conn);
                        command.Parameters.Add(new SQLiteParameter("FromID", SelectedPeople[i].SNID));
                        command.Parameters.Add(new SQLiteParameter("start", start));
                        command.Parameters.Add(new SQLiteParameter("end", end));
                        SQLiteDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            //GotData = true;
                            AlbumIDs.Add(reader.GetInt32(0));
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        APIError error = new APIError(null, ex.Message + "Reading Album IDs from DB", 1);
                    }
                    finally
                    {
                        DatabaseInUse = false;
                    }
                }
            }

            return AlbumIDs;
        }


        public static List<int> GetTagIDs(Int64 PhotoID)
        {
            List<int> PhotoIDs = new List<int>();

                lock (obj)
                {
                    DatabaseInUse = true;
                    try
                    {
                        GetConn();

                        SQLiteCommand command = new SQLiteCommand("select TagDataID from TagData where PhotoSNID=@PhotoID", conn);
                        command.Parameters.Add(new SQLiteParameter("PhotoID", PhotoID));
                        SQLiteDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            PhotoIDs.Add(reader.GetInt32(0));
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        APIError error = new APIError(null, ex.Message + "Reading PhotoTag IDs from DB", 1);
                    }
                    finally
                    {
                        DatabaseInUse = false;
                    }
                }
                
            
            return PhotoIDs;

        }

        /// <summary>
        /// Get Photo Comment IDs from PhotoID
        /// </summary>
        /// <param name="PhotoID"></param>
        /// <returns></returns>
        public static List<int> GetPhotoCommentIDs(Int64 PhotoID)
        {
            List<int> PostIDs = new List<int>();

            lock (obj)
            {
                DatabaseInUse = true;
                try
                {
                    GetConn();

                    SQLiteCommand command = new SQLiteCommand("select PostID from PostData where ParentID=@PhotoID", conn);
                    command.Parameters.Add(new SQLiteParameter("PhotoID", PhotoID));
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        PostIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                }
                catch (Exception ex)
                {
                    APIError error = new APIError(null, ex.Message + "Reading Photo Comments IDs from DB", 1);
                }
                finally
                {
                    DatabaseInUse = false;
                }
            }

            return PostIDs;

        }

        /// <summary>
        /// Get Album Comment IDs from AlbumID
        /// </summary>
        /// <param name="PhotoID"></param>
        /// <returns></returns>
        public static List<int> GetAlbumCommentIDs(Int64 AlbumID)
        {
            List<int> PostIDs = new List<int>();

            lock (obj)
            {
                DatabaseInUse = true;
                try
                {
                    GetConn();

                    SQLiteCommand command = new SQLiteCommand("select PostID from PostData where ParentID=@AlbumID", conn);
                    command.Parameters.Add(new SQLiteParameter("AlbumID", AlbumID));
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        PostIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                }
                catch (Exception ex)
                {
                    APIError error = new APIError(null, ex.Message + "Reading Album Comments IDs from DB", 1);
                }
                finally
                {
                    DatabaseInUse = false;
                }
            }

            return PostIDs;

        }

        /// <summary>
        /// Get LikeIDs from Object ID
        /// </summary>
        /// <param name="SNID of Object"></param>
        /// <returns></returns>
        public static List<int> GetLikeIDs(string SNID)
        {
            List<int> LikeIDs = new List<int>();

            lock (obj)
            {
                DatabaseInUse = true;
                try
                {
                    GetConn();

                    SQLiteCommand command = new SQLiteCommand("select ActionDataID from ActionData where SocialNetwork='1' AND WhatSNID=@SNID", conn);
                    command.Parameters.Add(new SQLiteParameter("SNID", SNID));
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        LikeIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();


                }
                catch (Exception ex)
                {
                    APIError error = new APIError(null, ex.Message + "Reading Like IDs from DB", 1);
                }
                finally
                {
                    DatabaseInUse = false;
                }
            }

            return LikeIDs;

        }


        public static List<int> GetEventIDsByPersonIDs(DateTime start, DateTime end, List<PersonLight> SelectedPeople)
        {
            int i, total;
            total = SelectedPeople.Count();

            List<int> EventIDs = new List<int>();

            for (i = 0; i < total; i++)
            {
                lock (obj)
                {
                    DatabaseInUse = true;
                    try
                    {
                        GetConn();

                        // Updated to show events associated to the user, separate of attending status
                        SQLiteCommand command = new SQLiteCommand("select distinct A.EventID from EventData A, ActionData B WHERE (A.ParentID=@IDAttending or (B.WhoSNID=@IDAttending AND B.ActionID in(14,15,16) AND B.WhatSNID=A.SNID)) AND A.StartTime>=@start and A.StartTime<@end", conn);
                        command.Parameters.Add(new SQLiteParameter("IDAttending", SelectedPeople[i].SNID));
                        command.Parameters.Add(new SQLiteParameter("start", start));
                        command.Parameters.Add(new SQLiteParameter("end", end));
                        SQLiteDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            //GotData = true;
                            EventIDs.Add(reader.GetInt32(0));
                        }

                        reader.Close();

                    }
                    catch (Exception ex)
                    {
                        APIError error = new APIError(null, ex.Message + "Reading Event IDs from DB", 1);
                    }
                    finally
                    {
                        DatabaseInUse = false;
                    }
                }
            }

            return EventIDs;

        }

        #endregion


    }
}
