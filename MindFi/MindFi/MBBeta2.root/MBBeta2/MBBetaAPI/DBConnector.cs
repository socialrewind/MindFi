using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MBBetaAPI
{
    /// <summary>
    /// Class that contains methods to manage the SQLite database
    /// </summary>
    public class DBConnector
    {
        //**************** Constructors
        #region Constructors

        public DBConnector()
        {
        }

        public DBConnector(string ConnStringParam)
        {
            ConnString = ConnStringParam;

            conn = new SQLiteConnection(ConnString);

        }

        #endregion

        //**************** Attributes
        #region Attributes

        public string ConnString;
        public SQLiteConnection conn;

        #endregion

        //**************** Methods
        #region Methods

        public List<int> GetFriendIDs()
        {
            List<int> PIDs = new List<int>();

            using (SQLiteConnection conn = new SQLiteConnection(ConnString))
            {
                try
                {
                    conn.Open();
                    //Distance 1 = Friend
                    //SQLiteCommand command = new SQLiteCommand("select A.ID from Entities A, PersonData B where A.Type='MBBetaAPI.AgentAPI.FBPerson' and B.Distance < 2 and A.ID = B.PersonID ", conn);
                    SQLiteCommand command = new SQLiteCommand("select A.ID from Entities A left outer join PersonData B where A.Type='MBBetaAPI.AgentAPI.FBPerson' and A.ID = B.PersonID ", conn);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                    conn.Close();
                }
                catch
                {
                    APIError error = new APIError(this, "Reading Person IDs from DB", 1);
                }
            }

            return PIDs;
        }

        public List<int> GetFriendOfFriendsIDs()
        {
            List<int> PIDs = new List<int>();

            using (SQLiteConnection conn = new SQLiteConnection(ConnString))
            {
                try
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand("select A.ID from Entities A, PersonData B where A.Type='MBBetaAPI.AgentAPI.FBPerson' and B.Distance=2 and A.ID = B.PersonID ", conn);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                    conn.Close();
                }
                catch
                {
                    APIError error = new APIError(this, "Reading Friends Of Friends IDs from DB", 1);
                }
            }

            return PIDs;
        }

        public List<int> GetSocialGroupIDs()
        {
            List<int> PIDs = new List<int>();

            using (SQLiteConnection conn = new SQLiteConnection(ConnString))
            {
                try
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand("select ID from Entities where Type='MyBackup.FBFriendList'", conn);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                    conn.Close();
                }
                catch
                {
                    APIError error = new APIError(this, "Reading Social Group IDs from DB", 1);
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
        public List<int> GetPosts(DateTime start, DateTime end, int Offset, int Limit)
        {
            List<int> PIDs = new List<int>();

            using (SQLiteConnection conn = new SQLiteConnection(ConnString))
            {
                try
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand("select PostID from PostData where ParentID is NULL and Created>=@start and Created<=@end ORDER BY Created DESC LIMIT @OFFSET,@LIMIT", conn);
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
                    conn.Close();
                }
                catch
                {
                    APIError error = new APIError(this, "Reading Post IDs from DB", 1);
                }
            }

            return PIDs;
        }

        /// <summary>
        /// Gets Posts given a list of persons and a date range
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="PersonList"></param>
        /// <returns></returns>
        public List<int> GetPostsByPersonID(DateTime start, DateTime end, List<long> PersonList)
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
                using (SQLiteConnection conn = new SQLiteConnection(ConnString))
                {
                    try
                    {

                        conn.Open();

                        string textcommand = "select PostID from PostData where ParentID is NULL and FromID in(";
                        textcommand += IDList;
                        textcommand += ") and Created>=@start and Created<=@end";

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
                        textcommand += ") AND B.ParentID IS NOT NULL and B.Created>=@start and B.Created<=@end AND B.ParentID = A.SNID";

                        command = new SQLiteCommand(textcommand, conn);
                        command.Parameters.Add(new SQLiteParameter("start", start));
                        command.Parameters.Add(new SQLiteParameter("end", end));
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            PostIDs.Add(reader.GetInt32(0));
                        }

                        reader.Close();


                        conn.Close();
                    }
                    catch
                    {
                        APIError error = new APIError(this, "Reading Post IDs from List of Persons in DB", 1);
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
        public int CountPosts(DateTime start, DateTime end)
        {
            int count = 0;

            using (SQLiteConnection conn = new SQLiteConnection(ConnString))
            {
                try
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand("select count (*) from PostData where ParentID is NULL and Created>=@start and Created<=@end", conn);
                    command.Parameters.Add(new SQLiteParameter("start", start));
                    command.Parameters.Add(new SQLiteParameter("end", end));
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        count = reader.GetInt32(0);
                    }

                    reader.Close();
                    conn.Close();
                }
                catch
                {
                    APIError error = new APIError(this, "Reading Count Post IDs from DB", 1);
                }
            }

            return count;
        }


        public List<int> GetSNParentMessageIDs(DateTime start, DateTime end, List<PersonLight> SelectedPeople)
        {
            int i, total;
            total = SelectedPeople.Count();

            List<int> ParentMessageIDs = new List<int>();

            try
            {
                for (i = 0; i < total; i++)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(ConnString))
                    {
                        conn.Open();

                        //TODO: Change when ToId is complete
                        //SQLiteCommand command = new SQLiteCommand("select distinct MessageID from MessageData where ParentID IS NULL and (FromID=@FromID OR ToId Like @LikeId) and ((Updated>@start and Updated<@end) OR (Created>@start and Created<@end))", conn);
                        SQLiteCommand command = new SQLiteCommand("select distinct MessageID from MessageData where ParentID IS NULL and FromID=@FromID and ((Updated>=@start and Updated=<@end) OR (Created>=@start and Created<=@end))", conn);
                        command.Parameters.Add(new SQLiteParameter("FromID", SelectedPeople[i].SNID));
                        //string LikeId = "%" + selectedPeopleDetails[i].SocialNetworkID.ToString() + "%";
                        //command.Parameters.Add(new SqlParameter("LikeId", LikeId));
                        command.Parameters.Add(new SQLiteParameter("start", start));
                        command.Parameters.Add(new SQLiteParameter("end", end));
                        SQLiteDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            //GotData = true;
                            ParentMessageIDs.Add(reader.GetInt32(0));
                        }

                        reader.Close();


                        conn.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                APIError error = new APIError(this, ex.Message + "Reading Parent Message IDs from DB", 1);
            }


            return ParentMessageIDs;
        }


        public List<int> GetAlbumIDs(DateTime start, DateTime end, List<PersonLight> SelectedPeople)
        {
            

            int i, total;
            total = SelectedPeople.Count();

            List<int> AlbumIDs = new List<int>();

            try
            {
                for (i = 0; i < total; i++)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(ConnString))
                    {
                        conn.Open();

                        //TODO: This query must change to look into PhotoData Table, according to people appearing in Photos
                        SQLiteCommand command = new SQLiteCommand("select distinct AlbumID from AlbumData where FromID=@FromID and ((Updated>=@start and Updated<=@end) OR (Created>=@start and Created<=@end))", conn);
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

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + "Reading Album IDs from DB", 1);
            }

            return AlbumIDs;
        }


        public List<int> GetTagIDs(Int64 PhotoID)
        {
            List<int> PhotoIDs = new List<int>();

            try
            {
                
                using (SQLiteConnection conn = new SQLiteConnection(ConnString))
                {
                    conn.Open();

                    SQLiteCommand command = new SQLiteCommand("select TagDataID from TagData where PhotoSNID=@PhotoID", conn);
                    command.Parameters.Add(new SQLiteParameter("PhotoID", PhotoID));
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        PhotoIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                    conn.Close();
                }
                
            }
            catch (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + "Reading PhotoTag IDs from DB", 1);
            }

            return PhotoIDs;

        }

        /// <summary>
        /// Get Photo Comment IDs from PhotoID
        /// </summary>
        /// <param name="PhotoID"></param>
        /// <returns></returns>
        public List<int> GetPhotoCommentIDs(Int64 PhotoID)
        {
            List<int> PostIDs = new List<int>();

            try
            {

                using (SQLiteConnection conn = new SQLiteConnection(ConnString))
                {
                    conn.Open();

                    SQLiteCommand command = new SQLiteCommand("select PostID from PostData where ParentID=@PhotoID", conn);
                    command.Parameters.Add(new SQLiteParameter("PhotoID", PhotoID));
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        PostIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + "Reading Photo Comments IDs from DB", 1);
            }

            return PostIDs;

        }

        /// <summary>
        /// Get Album Comment IDs from AlbumID
        /// </summary>
        /// <param name="PhotoID"></param>
        /// <returns></returns>
        public List<int> GetAlbumCommentIDs(Int64 AlbumID)
        {
            List<int> PostIDs = new List<int>();

            try
            {

                using (SQLiteConnection conn = new SQLiteConnection(ConnString))
                {
                    conn.Open();

                    SQLiteCommand command = new SQLiteCommand("select PostID from PostData where ParentID=@AlbumID", conn);
                    command.Parameters.Add(new SQLiteParameter("AlbumID", AlbumID));
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        PostIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + "Reading Album Comments IDs from DB", 1);
            }

            return PostIDs;

        }

        /// <summary>
        /// Get LikeIDs from Object ID
        /// </summary>
        /// <param name="SNID of Object"></param>
        /// <returns></returns>
        public List<int> GetLikeIDs(string SNID)
        {
            List<int> LikeIDs = new List<int>();

            try
            {

                using (SQLiteConnection conn = new SQLiteConnection(ConnString))
                {
                    conn.Open();

                    SQLiteCommand command = new SQLiteCommand("select ActionDataID from ActionData where SocialNetwork='1' AND WhatSNID=@SNID", conn);
                    command.Parameters.Add(new SQLiteParameter("SNID", SNID));
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        LikeIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + "Reading Like IDs from DB", 1);
            }

            return LikeIDs;

        }


        public List<int> GetEventIDsByPersonIDs(DateTime start, DateTime end, List<PersonLight> SelectedPeople)
        {
            int i, total;
            total = SelectedPeople.Count();

            List<int> EventIDs = new List<int>();

            try
            {
                for (i = 0; i < total; i++)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(ConnString))
                    {
                        conn.Open();

                        SQLiteCommand command = new SQLiteCommand("select A.EventID from EventData A, ActionData B WHERE B.WhoSNID=@IDAttending AND B.ActionID in(14,15,16) AND B.WhatSNID=A.SNID AND A.StartTime>=@start and A.StartTime<=@end", conn);
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

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + "Reading Event IDs from DB", 1);
            }

            return EventIDs;

        }

        #endregion


    }
}
