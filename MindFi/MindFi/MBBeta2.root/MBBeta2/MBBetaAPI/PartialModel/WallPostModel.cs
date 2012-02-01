using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public partial class WallPost : INotifyPropertyChanged
    {
        //**************** Constructors
        #region Constructors
        #endregion

        //**************** Attributes
        #region Attributes
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        //**************** Methods
        #region Methods

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

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
                //Read Post
                // TODO: Clean unused fields
                SQLiteCommand command = new SQLiteCommand("select SocialNetwork, SNID, FromID, FromName, ToID, ToName, Message, Picture, Link, Caption, Description, PostData.Created, PostData.Updated, CommentCount, LikesCount, PostData.ParentID, PostType, PostRequestID, State, RequestType, ResponseValue, Story from PostData left outer join RequestsQueue on PostRequestID= RequestsQueue.ID where PostID = @ID", conn);
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
                        //ToID = reader.GetString(4);
                        if (reader.IsDBNull(5))
                        {
                            ToName = "";
                        }
                        else
                        {
                            ToName = reader.GetString(5);
                        }
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
                        {
                            if (reader.IsDBNull(12))
                            {
                                Date = DateTime.MinValue;
                            }
                            else
                            {
                                Date = reader.GetDateTime(12);
                            }
                        }
                        else
                        {
                            Date = reader.GetDateTime(11);
                        }
                        Date = Date.ToLocalTime();

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

                        if (reader.IsDBNull(17))
                        {
                            PostRequestID = 0;
                        }
                        else
                        {
                            PostRequestID = reader.GetInt32(17);
                        }
                        if (reader.IsDBNull(18))
                        {
                            PostRequestState = 0;
                        }
                        else
                        {
                            PostRequestState = reader.GetInt32(18);
                        }
                        if (reader.IsDBNull(19))
                        {
                            PostRequestType = "";
                        }
                        else
                        {
                            PostRequestType = reader.GetString(19);
                        }
                        if (reader.IsDBNull(20))
                        {
                            PostResponseValue = "";
                        }
                        else
                        {
                            PostResponseValue = reader.GetString(20);
                        }
                        if (reader.IsDBNull(21))
                        {
                            Story = "";
                        }
                        else
                        {
                            Story = reader.GetString(21);
                        }
                        WallName = FromName;
                        if (ToName != "" && ToName != FromName )
                        {
                            WallName += " > " + ToName;
                        }
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
                        FromPhotoPath = AsyncReqQueue.ProfilePhotoDestinationDir + reader.GetString(0);
                    }
                }
                reader.Close();
                if (!GotData2)
                    FromPhotoPath = "Images/nophoto.jpg";

                // TODO: Double check possible states
                // Parse process, if needed
                switch (PostRequestState)
                {
                    case AsyncReqQueue.RECEIVED:
                        ProcessReceivedRequest();
                        NotifyPropertyChanged("All");
                        break;
                    case AsyncReqQueue.PARSED:
                        if ( Title == null && Message == null)
                        {
                            ProcessReceivedRequest();
                            ErrorMessage += "Appeared already parsed but no special data available";
                            NotifyPropertyChanged("All");
                        }
                        break;
                    case 0:
                        // create the new request and send - prioritize
                        string errorMessage;
                        AsyncReqQueue apiReq = FBAPI.Post(SNID, ProcessOnePost);
                        if (apiReq.QueueAndSend(999))
                        {
                            DBLayer.UpdatePostRequest(ID, apiReq.ID, out errorMessage);
                            if (errorMessage != "")
                            {
                                ErrorMessage = errorMessage;
                            }
                        }
                        else
                        {
                            // TODO: Localized
                            ErrorMessage = "Cannot request information while disconnected";
                        }
                        break;
                    default:
                        break;
                }

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }

            if (!GotData)
                throw new Exception("No data available for the Wall Post" + ID.ToString());
        }



        int GetNumberOfChildPostsFromDB(string SNIDPost)
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



        public List<int> ChildPostIDsFromDB()
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

        #region "Response methods"
        private void ProcessReceivedRequest()
        {
            switch (PostRequestType)
            {
                case "FBPost":
                    string errorData;
                    FBPost aPost = new FBPost(PostResponseValue);
                    aPost.Parse();
                    // TODO: Very it is optimized when likes are not available, and don't process if all likes are already here
                    if ( aPost.HasLikes ) //if (aPost.LikesCount > 0)
                    {
                        AsyncReqQueue apiReq;
                        apiReq = FBAPI.Likes(aPost.SNID, AsyncReqQueue.SIZETOGETPERPAGE, ProcessLikes, aPost.ID);
                        apiReq.Queue(800);
                    }
                    //TODO: check if it needs a separate ProcessOnePost associated to the children...
                    //if (aPost.CommentCount > CommentsCount)
                    //{
                    //    
                    //    AsyncReqQueue apiReq = FBAPI.Post(aPost.SNID, ProcessOnePost);
                    //    apiReq.Queue(800);
                    //}
                    // sync data from aPost
                    // TODO: Double check all fields are used
                    Title = aPost.Caption;
                    Message = aPost.Message;
                    Description = aPost.Description;
                    ApplicationName = aPost.ApplicationName;
                    Date = (aPost.Updated != null) ? (DateTime)aPost.Updated : ((aPost.Created != null) ? (DateTime)aPost.Created : DateTime.MinValue);
                    CommentsCount = (aPost.CommentCount != null) ? (int)aPost.CommentCount : 0;
                    if ( aPost.Link != null && aPost.Link != "")
                    {
                        Link = new Uri(aPost.Link);
                    }
                    if (aPost.Picture != null && aPost.Picture != "")
                    {
                        Picture = new Uri(aPost.Picture);
                    }
                    aPost.Parsed = true;
                    aPost.Save(out errorData);
                    break;
                // TODO: default case and other types for Twitter, LinkedIn
            }
        }


        /// <summary>
        /// Process details of a single post
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON post data</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public bool ProcessOnePost(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            string errorData = "";

            if (result)
            {
                ProcessReceivedRequest();

                if (errorData == "")
                {
                    return true;
                }
                // corrected bug: return an error without mark as failed
                return false;
            }
            return false;
        }

        /// <summary>
        /// Process likes
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the object ID that is liked</param>
        /// <param name="parentSNID">CHECK: Reference to the object SNID</param>
        public static bool ProcessLikes(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            // TODO: notify change on likes
            return AsyncReqQueue.ProcessActions(hwnd, result, response, Verb.LIKE, parent, parentSNID);
        }

        #endregion
    }
}
