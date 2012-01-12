using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class WallPost
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
                    string msg = "Reading Wall Post from DB" + ex.Message.ToString();
                    APIError error = new APIError(this, msg, 1);
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

                //Read Person
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, SNID, FromID, FromName, ToID, ToName, Message, Picture, Link, Caption, Description, Created, Updated, CommentCount, LikesCount, ParentID, PostType from PostData where PostID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {

                    GotData = true;
                    
                    
                        //SocialNetwork, SNID, FromID, FromName, ToID, ToName, Message
                        SN = reader.GetInt32(0);
                        var value = reader.GetValue(1);
                        SNID = Convert.ToString(value);
                        FromID = reader.GetString(2);
                        FromName = reader.GetString(3);
                        //TODO: Enable when data is available
                        //ToID = reader.GetString(4);
                        //ToName = reader.GetString(5);
                        if (reader.IsDBNull(6))
                            Message = "";
                        else
                            Message = reader.GetString(6);

                        //Picture, Link, Caption, Description, Created, Updated, 
                        if (!reader.IsDBNull(7))
                            Picture = new Uri(reader.GetString(7));
                        if (!reader.IsDBNull(8))
                            Link = new Uri(reader.GetString(8));
                        if (reader.IsDBNull(9))
                            Caption = "";
                        else
                            Caption = reader.GetString(9);
                        if (reader.IsDBNull(10))
                            Description = "";
                        else
                            Description = reader.GetString(10);
                        if (reader.IsDBNull(11))
                            if(reader.IsDBNull(12))
                                Date = DateTime.MinValue;
                            else
                                Date = reader.GetDateTime(12);
                        else
                            Date = reader.GetDateTime(11);


                        //CommentCount, LikesCount, ParentID, PostType
                        if (reader.IsDBNull(13))
                            CommentsCount = 0;
                        else
                            CommentsCount = reader.GetInt32(13);
                        //TODO: Enable when data available
                        //LikesCount = reader.GetInt32(14);
                        if (reader.IsDBNull(15))
                            ParentID = "";
                        else
                            ParentID = reader.GetString(15);
                        if (reader.IsDBNull(16))
                            PostType = "";
                        else
                            PostType = reader.GetString(16);
                    

                }

                reader.Close();

                command = new SQLiteCommand("select Name from Entities where ID=@ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                        Title = "";
                    else
                        Title = reader.GetString(0);
                }
                reader.Close();

                command = new SQLiteCommand("select ProfilePic from PersonData where SNID=@ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", this.FromID));
                reader = command.ExecuteReader();
                bool GotData2 = false;
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                        FromPhotoPath = "Images/nophoto.jpg";
                    else
                    {
                        GotData2 = true;
                        FromPhotoPath = reader.GetString(0);
                    }
                }
                reader.Close();
                if (!GotData2)
                    FromPhotoPath = "Images/nophoto.jpg";
                    

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

            if (!GotData)
                throw new Exception("No data available for the Wall Post" + ID.ToString());
        }



        int GetNumberOfChildPostsFromDB(DBConnector db, string SNIDPost)
        {
            int Total = 0;
            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();

                    SQLiteCommand command = new SQLiteCommand("select COUNT(*) from PostData where ParentPost=@IDPost", DBLayer.conn);
                    command.Parameters.Add(new SQLiteParameter("IDPost", SNIDPost));
                    SQLiteDataReader reader = command.ExecuteReader();
                    bool GotData = false;
                    while (reader.Read())
                    {
                        GotData = true;
                        Total = reader.GetInt32(0);
                    }
                    reader.Close();
                    if (!GotData)
                        throw new Exception("No data available for the post");

                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }


            return Total;

        }



        public List<int> ChildPostIDsFromDB(DBConnector db)
        {

            List<int> ChildPostIDs = new List<int>();

            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();

                    SQLiteCommand command = new SQLiteCommand("select PostID from PostData where ParentID=@ParentID", DBLayer.conn);
                    command.Parameters.Add(new SQLiteParameter("ParentID", SNID));
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ChildPostIDs.Add(reader.GetInt32(0));
                    }

                    reader.Close();
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            } 

            return ChildPostIDs;

        }



        #endregion


    }
}
