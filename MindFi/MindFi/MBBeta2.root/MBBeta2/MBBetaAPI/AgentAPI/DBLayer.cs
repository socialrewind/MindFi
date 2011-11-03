using System;
using System.Collections;
using System.Data.SQLite;
using System.IO;
using System.Threading;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Class that contains methods to manage the SQLite database
    /// </summary>
    public class DBLayer
    {
        public static string ConnString;
        private static volatile Object obj = new Object();
        private static string lastError;
        private static volatile SQLiteConnection conn = null;
        private static volatile SQLiteTransaction mytransaction = null;
        private static volatile Mutex mut = new Mutex();

        public static void GetConn()
        {
            lock (obj)
            {
                if (conn == null)
                {
                    conn = new SQLiteConnection(DBLayer.ConnString);
                    conn.Open();
                }
            }

        }

        public static void BeginTransaction()
        {
            //System.Windows.Forms.MessageBox.Show("Begin trans");
            mut.WaitOne();
            lock (obj)
            {
                if (conn != null)
                {
                    mytransaction = conn.BeginTransaction();
                }
            }
        }

        public static void CommitTransaction()
        {
            //System.Windows.Forms.MessageBox.Show("Commit trans");
            lock (obj)
            {
                if (mytransaction != null)
                    mytransaction.Commit();
                mytransaction = null;
            }
            mut.ReleaseMutex();
        }

        public static void RollbackTransaction()
        {
            lock (obj)
            {
                if (mytransaction != null)
                {
                    mytransaction.Rollback();
                }
                mytransaction = null;
            }
            mut.ReleaseMutex();
        }

        public static bool CreateDB(string file, string user, string password = null)
        {
            lock (obj)
            {

                // Copy the database to destination
                string origin =  AppDomain.CurrentDomain.BaseDirectory + "\\Template.db";

                if (file != origin)
                {
                    File.Copy(origin, file, true);
                }

                lock (obj)
                {
                    DBLayer.ConnString = "Data Source=" + file + "";
                    conn = null;
                    GetConn();
                    SQLiteCommand InsertCmd = new SQLiteCommand(
                        "insert into Config(Username, CurrentStep) values (?,?)"
                            , conn);
                    SQLiteParameter pName = new SQLiteParameter();
                    pName.Value = user;
                    InsertCmd.Parameters.Add(pName);
                    SQLiteParameter pCurrent = new SQLiteParameter();
                    pCurrent.Value = 0;
                    InsertCmd.Parameters.Add(pCurrent);
                    InsertCmd.ExecuteNonQuery();
                    conn.ChangePassword(password);
                    conn.Close();
                    conn = null;
                    DBLayer.ConnString = "Data Source=" + file + ";Password=" + password;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets current configuration
        /// </summary>
        public static void GetConfig(out string UserName, out int CurrentStep, out string ErrorMessage)
        {
            CurrentStep = -1;
            UserName = "";
            ErrorMessage = "";

            lock (obj)
            {
                try
                {
                    GetConn();
                    // try to read, if not, create
                    SQLiteCommand CheckCmd = new SQLiteCommand("select Username, CurrentStep from Config", conn);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        UserName = reader.GetString(0);
                        CurrentStep = reader.GetInt32(1);
                    }
                    reader.Close();
                    ErrorMessage = "";
                    return;
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error connecting to the database: " + ex.ToString() + "\n";
                }
            }
        }

        /// <summary>
        /// Gets the list of Social Networks
        /// </summary>
        public static ArrayList GetSNList(out string ErrorMessage)
        {
            ArrayList temp = new ArrayList();

            lock (obj)
            {
                try
                {
                    GetConn();
                    // try to read, if not, create
                    SQLiteCommand CheckCmd = new SQLiteCommand("select ID, SNName from SocialNetworks", conn);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    temp.Clear();
                    while (reader.Read())
                    {
                        temp.Add(new SocialNetwork(reader.GetInt32(0), reader.GetString(1)));
                    }
                    reader.Close();
                    ErrorMessage = "";
                    return temp;
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error connecting to the database: " + ex.ToString() + "\n";
                    return null;
                }
            }
        }

        public static bool ReqQueueSave(long ID, string ReqType, int Priority,
            long? ParentID, string ParentSNID,
            DateTime Created, DateTime Updated, string ReqURL,
            int State, string Filename, bool AddToken, bool AddDateRange,
            bool Saved, out string ErrorMessage)
        {


            lock (obj)
            {
                try
                {
                    GetConn();

                    if (!Saved)
                    {
                        // insert
                        SQLiteCommand InsertCmd = new SQLiteCommand(
                        "insert into RequestsQueue(ID,RequestType,Priority,ParentID,ParentSNID,Created," +
                        "RequestString,State,Filename,AddToken, AddDateRange) values (?,?,?,?,?,?,?,?,?,?,?)"
                        , conn);
                        SQLiteParameter pId = new SQLiteParameter();
                        pId.Value = ID;
                        InsertCmd.Parameters.Add(pId);
                        SQLiteParameter pType = new SQLiteParameter();
                        pType.Value = ReqType;
                        InsertCmd.Parameters.Add(pType);
                        SQLiteParameter pPriority = new SQLiteParameter();
                        pPriority.Value = Priority;
                        InsertCmd.Parameters.Add(pPriority);
                        SQLiteParameter pPID = new SQLiteParameter();
                        pPID.Value = ParentID;
                        InsertCmd.Parameters.Add(pPID);
                        SQLiteParameter pPSNID = new SQLiteParameter();
                        pPSNID.Value = ParentSNID;
                        InsertCmd.Parameters.Add(pPSNID);
                        SQLiteParameter pCreated = new SQLiteParameter();
                        pCreated.Value = Created;
                        InsertCmd.Parameters.Add(pCreated);
                        SQLiteParameter pURL = new SQLiteParameter();
                        pURL.Value = ReqURL;
                        InsertCmd.Parameters.Add(pURL);
                        SQLiteParameter pState = new SQLiteParameter();
                        pState.Value = State;
                        InsertCmd.Parameters.Add(pState);
                        SQLiteParameter pFilename = new SQLiteParameter();
                        pFilename.Value = Filename;
                        InsertCmd.Parameters.Add(pFilename);
                        SQLiteParameter pToken = new SQLiteParameter();
                        pToken.Value = AddToken;
                        InsertCmd.Parameters.Add(pToken);
                        SQLiteParameter pDateR = new SQLiteParameter();
                        pDateR.Value = AddDateRange;
                        InsertCmd.Parameters.Add(pDateR);
                        InsertCmd.ExecuteNonQuery();
                        Saved = true;
                    }
                    else
                    {
                        // update
                        SQLiteCommand UpdateCmd = new SQLiteCommand(
                            "Update RequestsQueue set RequestType=?,Priority=?,ParentID=?,ParentSNID=?," +
                            "Updated=?,RequestString=?,State=?,Filename=?,AddToken=?,AddDateRange=? where ID=?"
                        , conn);
                        SQLiteParameter pUType = new SQLiteParameter();
                        pUType.Value = ReqType;
                        UpdateCmd.Parameters.Add(pUType);
                        SQLiteParameter pUPriority = new SQLiteParameter();
                        pUPriority.Value = Priority;
                        UpdateCmd.Parameters.Add(pUPriority);
                        SQLiteParameter pPID = new SQLiteParameter();
                        pPID.Value = ParentID;
                        UpdateCmd.Parameters.Add(pPID);
                        SQLiteParameter pPSNID = new SQLiteParameter();
                        pPSNID.Value = ParentSNID;
                        UpdateCmd.Parameters.Add(pPSNID);
                        SQLiteParameter pUpdated = new SQLiteParameter();
                        pUpdated.Value = Updated;
                        UpdateCmd.Parameters.Add(pUpdated);
                        SQLiteParameter pUURL = new SQLiteParameter();
                        pUURL.Value = ReqURL;
                        UpdateCmd.Parameters.Add(pUURL);
                        SQLiteParameter pUState = new SQLiteParameter();
                        pUState.Value = State;
                        UpdateCmd.Parameters.Add(pUState);
                        SQLiteParameter pUFilename = new SQLiteParameter();
                        pUFilename.Value = Filename;
                        UpdateCmd.Parameters.Add(pUFilename);
                        SQLiteParameter pUToken = new SQLiteParameter();
                        pUToken.Value = AddToken;
                        UpdateCmd.Parameters.Add(pUToken);
                        SQLiteParameter pUDateR = new SQLiteParameter();
                        pUDateR.Value = AddDateRange;
                        UpdateCmd.Parameters.Add(pUDateR);
                        SQLiteParameter pUId = new SQLiteParameter();
                        pUId.Value = ID;
                        UpdateCmd.Parameters.Add(pUId);
                        UpdateCmd.ExecuteNonQuery();
                    }
                    ErrorMessage = "";
                    return Saved;
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving to the database " + ex.ToString();
                    return false;
                }
            }
        }

        public static bool RespQueueSave(long? ID, string response, int State = AgentAPI.AsyncReqQueue.RECEIVED)
        {
            string SQL = "";

            if (ID == null)
                return false;

            lock (obj)
            {
                bool Saved = false;
                string ErrorMessage;
                try
                {
                    GetConn();
                    SQL = "Update RequestsQueue set State=?, ResponseValue=?, Updated=? where ID=?";
                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pUState = new SQLiteParameter();
                    pUState.Value = State;
                    UpdateCmd.Parameters.Add(pUState);
                    SQLiteParameter pUResponse = new SQLiteParameter();
                    pUResponse.Value = response;
                    UpdateCmd.Parameters.Add(pUResponse);
                    SQLiteParameter pUpdated = new SQLiteParameter();
                    pUpdated.Value = DateTime.Now;
                    UpdateCmd.Parameters.Add(pUpdated);
                    SQLiteParameter pUId = new SQLiteParameter();
                    pUId.Value = ID;
                    UpdateCmd.Parameters.Add(pUId);
                    UpdateCmd.ExecuteNonQuery();
                    int outrows = UpdateCmd.ExecuteNonQuery();
                    if (outrows > 0)
                    {
                        Saved = true;
                        /*System.Windows.Forms.MessageBox.Show("Request was saved: ID=" + ID 
                        + ", State=" + State + "\nSQL=" + SQL);*/
                        ErrorMessage = "";
                    }
                    else
                    {
                        /*System.Windows.Forms.MessageBox.Show("No requests saved: ID=" + ID 
                        + ", State=" + State + "\nSQL=" + SQL);*/
                        ErrorMessage = "0 rows saved";
                    }

                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving to the database " + ex.ToString();
                    /* System.Windows.Forms.MessageBox.Show("No requests saved: ID=" + ID 
                        + ", State=" + State + "\nSQL=" + SQL+ "\nError=" + ErrorMessage);*/
                }
                return Saved;
            }
        }

        public static bool QueueRetryUpdate()
        {
            string SQL = "";

            lock (obj)
            {
                string ErrorMessage;
                try
                {
                    GetConn();
                    SQL = "Update RequestsQueue set State=? where State=? and Updated<?";
                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pUState = new SQLiteParameter();
                    pUState.Value = AsyncReqQueue.RETRY;
                    UpdateCmd.Parameters.Add(pUState);
                    SQLiteParameter pUState2 = new SQLiteParameter();
                    pUState2.Value = AsyncReqQueue.SENT;
                    UpdateCmd.Parameters.Add(pUState2);
                    // only update "manually" requests that have taken more than twice the standard timeout
                    SQLiteParameter pUpdated = new SQLiteParameter();
                    pUpdated.Value = DateTime.Now.AddMilliseconds(-FBAPI.DEFAULT_TIMEOUT * 2);
                    UpdateCmd.Parameters.Add(pUpdated);
                    UpdateCmd.ExecuteNonQuery();
                    int outrows = UpdateCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error updating requests to retry " + ex.ToString();
                    /* System.Windows.Forms.MessageBox.Show("No requests saved: ID=" + ID 
                        + ", State=" + State + "\nSQL=" + SQL+ "\nError=" + ErrorMessage);*/
                    return false;
                }
                return true;
            }
        }

        public static bool ReqQueueIncreasePriority(long? ID, int PriorityIncrease)
        {
            if (ID == null)
                return false;

            lock (obj)
            {
                bool Saved = false;
                string ErrorMessage;
                try
                {
                    GetConn();
                    SQLiteCommand UpdateCmd = new SQLiteCommand(
                        "Update RequestsQueue set Priority=Priority+? where ID=?"
                            , conn);
                    SQLiteParameter pUPriority = new SQLiteParameter();
                    pUPriority.Value = PriorityIncrease;
                    UpdateCmd.Parameters.Add(pUPriority);
                    SQLiteParameter pUId = new SQLiteParameter();
                    pUId.Value = ID;
                    UpdateCmd.Parameters.Add(pUId);
                    UpdateCmd.ExecuteNonQuery();
                    int outrows = UpdateCmd.ExecuteNonQuery();
                    if (outrows > 0)
                    {
                        Saved = true;
                    }
                    ErrorMessage = "";
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving to the database " + ex.ToString();
                }
                return Saved;
            }
        }

        public static int AvailableReqQueueSave(out string ErrorMessage)
        {
            int tempID = 0;
            ErrorMessage = "";

            lock (obj)
            {

                try
                {
                    GetConn();

                    SQLiteCommand ReadCmd = new SQLiteCommand(
                        "select max(ID)+1 as ID from RequestsQueue", conn);
                    SQLiteDataReader reader = ReadCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            tempID = reader.GetInt32(0);
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error getting ID from the database " + ex.ToString();
                }
            } // lock
            return tempID;
        }

        public static int EntitySave(int ID, string Name, string ObjType, bool Active,
                bool Saved, out bool SavedAfter, out string ErrorMessage)
        {
            int tempID = 1;
            ErrorMessage = "";
            SavedAfter = false;
            if (Name == null)
            {
                Name = "";
            }

            lock (obj)
            {

                try
                {
                    GetConn();

                    if (!Saved)
                    {
                        SQLiteCommand ReadCmd = new SQLiteCommand(
                        "select max(ID)+1 as ID from Entities", conn);
                        SQLiteDataReader reader = ReadCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                tempID = reader.GetInt32(0);
                            }
                        }
                        reader.Close();
                        // insert
                        SQLiteCommand InsertCmd = new SQLiteCommand(
                        "insert into Entities(ID,Name,Type,Active) values (?,?,?,?)"
                        , conn);
                        SQLiteParameter pId = new SQLiteParameter();
                        pId.Value = tempID;
                        InsertCmd.Parameters.Add(pId);
                        SQLiteParameter pName = new SQLiteParameter();
                        pName.Value = Name;
                        InsertCmd.Parameters.Add(pName);
                        SQLiteParameter pType = new SQLiteParameter();
                        pType.Value = ObjType;
                        InsertCmd.Parameters.Add(pType);
                        SQLiteParameter pActive = new SQLiteParameter();
                        pActive.Value = Active ? 1 : 0;
                        InsertCmd.Parameters.Add(pActive);
                        InsertCmd.ExecuteNonQuery();
                        SavedAfter = true;
                    }
                    else
                    {
                        // update
                        SQLiteCommand UpdateCmd = new SQLiteCommand(
                        "Update Entities set Name=?, Active=? where ID=?"
                        , conn);
                        SQLiteParameter pUName = new SQLiteParameter();
                        pUName.Value = Name;
                        UpdateCmd.Parameters.Add(pUName);
                        SQLiteParameter pUActive = new SQLiteParameter();
                        pUActive.Value = Active ? 1 : 0;
                        UpdateCmd.Parameters.Add(pUActive);
                        SQLiteParameter pUId = new SQLiteParameter();
                        pUId.Value = ID;
                        tempID = ID;
                        UpdateCmd.Parameters.Add(pUId);
                        UpdateCmd.ExecuteNonQuery();
                        SavedAfter = true;
                    }
                    ErrorMessage = "";
                    return tempID;
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error getting ID from the database " + ex.ToString();
                }
            } // lock
            return tempID;
        }

        private static int GetReferenceTable(string Table, out string Field)
        {
            int ReferenceTable = -1;
            Field = "";
            switch (Table)
            {
                case "PersonData":
                    ReferenceTable = 0;
                    Field = "PersonID";
                    break;
                case "PersonURL":
                    ReferenceTable = 1;
                    Field = "PersonID";
                    break;
                case "OrganizationData":
                    ReferenceTable = 2;
                    Field = "OrganizationID";
                    break;
                case "OrganizationURL":
                    ReferenceTable = 3;
                    Field = "OrganizationID";
                    break;
                case "PostData":
                    ReferenceTable = 4;
                    Field = "PostID";
                    break;
                case "MessageData":
                    ReferenceTable = 5;
                    Field = "MessageID";
                    break;
                case "AlbumData":
                    ReferenceTable = 6;
                    Field = "AlbumID";
                    break;
                case "PhotoData":
                    ReferenceTable = 7;
                    Field = "PhotoID";
                    break;
                case "TagData":
                    ReferenceTable = 8;
                    Field = "";
                    break;
                case "ActionData":
                    ReferenceTable = 9;
                    Field = "";
                    break;
                case "EventData":
                    ReferenceTable = 10;
                    Field = "EventID";
                    break;
                case "NotificationData":
                    ReferenceTable = 11;
                    Field = "NotificationID";
                    break;
                case "NoteData":
                    ReferenceTable = 12;
                    Field = "NoteID";
                    break;
                default:
                    // TODO: message box for easier diagnostic when not in final production
                    throw new Exception("Invalid table for DataSave: " + Table);
            }
            return ReferenceTable;
        }

        public static void DataSave(int ID, int SocialNetwork, string SNID, string Table,
            out decimal PartitionDate, out int PartitionID, out bool SavedAfter,
            out string ErrorMessage)
        {
            int tempID = 0;
            ErrorMessage = "";
            SavedAfter = false;
            string Field;
            int ReferenceTable = GetReferenceTable(Table, out Field);
            string SQL;

            lock (obj)
            {

                SQL = "insert into DailyID (Day, ReferenceTable, FreeID) values (?, ?, 2)";

                PartitionID = tempID;
                DateTime tempD = DateTime.Now;
                PartitionDate = tempD.Year * 10000 +
                tempD.Month * 100 + tempD.Day;

                try
                {

                    GetConn();
                    // TODO: Transactions for optimization

                    // Verify if the record already exists for the corresponding SNID
                    SQLiteCommand ExistsCmd = new SQLiteCommand(
                        "select PartitionDate, PartitionID from " + Table
                            + " where SNID=? and SocialNetwork=? and Active=1",
                            conn);
                    SQLiteParameter pSNID = new SQLiteParameter();
                    pSNID.Value = SNID;
                    ExistsCmd.Parameters.Add(pSNID);
                    SQLiteParameter pSN = new SQLiteParameter();
                    pSN.Value = SocialNetwork;
                    ExistsCmd.Parameters.Add(pSN);
                    SQLiteDataReader prevRecord = ExistsCmd.ExecuteReader();
                    SQLiteParameter pDay = new SQLiteParameter();
                    pDay.Value = PartitionDate;

                    if (prevRecord.Read() && !prevRecord.IsDBNull(0))
                    {
                        // Update existing record
                        PartitionDate = prevRecord.GetDecimal(0);
                        PartitionID = prevRecord.GetInt32(1);
                        prevRecord.Close();
                        SQL = "update " + Table
                        + " set LastUpdate=? where PartitionDate=? and PartitionID=?";
                        SQLiteCommand InsertCmd = new SQLiteCommand(SQL, conn);
                        SQLiteParameter pLastUpd = new SQLiteParameter();
                        pLastUpd.Value = tempD;
                        pDay.Value = PartitionDate; // could be different!
                        InsertCmd.Parameters.Add(pLastUpd);
                        InsertCmd.Parameters.Add(pDay);
                        SQLiteParameter pPID = new SQLiteParameter();
                        pPID.Value = PartitionID;
                        InsertCmd.Parameters.Add(pPID);
                        int outrows = InsertCmd.ExecuteNonQuery();
                        if (outrows > 0)
                        {
                            SavedAfter = true;
                        }
                    }
                    else
                    {
                        prevRecord.Close();
                        // First part: get next ID for the appropriate table
                        // TODO: separate function
                        SQLiteCommand ReadCmd = new SQLiteCommand(
                        "select FreeID from DailyID  where Day=? and ReferenceTable=?",
                            conn);
                        ReadCmd.Parameters.Add(pDay);
                        SQLiteParameter pRefTable = new SQLiteParameter();
                        pRefTable.Value = ReferenceTable;
                        ReadCmd.Parameters.Add(pRefTable);
                        SQLiteDataReader reader = ReadCmd.ExecuteReader();

                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                tempID = reader.GetInt32(0);
                                SQL = "update DailyID set FreeID=FreeID+1 where Day=? and ReferenceTable=?";
                            }
                        }
                        else
                        {
                            tempID = 1; // first execution, I am #1, but free is 2
                        }
                        PartitionID = tempID;
                        reader.Close();
                        // insert or update dailyID
                        SQLiteCommand InsertCmd = new SQLiteCommand(SQL, conn);
                        InsertCmd.Parameters.Add(pDay);
                        InsertCmd.Parameters.Add(pRefTable);
                        InsertCmd.ExecuteNonQuery();
                        // now Insert into appropriate table
                        // TODO: make it smarter insert/update behaviour, for incremental backup
                        SQL = "insert into " + Table
                        + "(PartitionDate, PartitionID, " + Field + ", SocialNetwork, SNID, Active, LastUpdate)"
                        + " values (?, ?, ?, ?, ?, 1, ?)";
                        // note - reusing
                        InsertCmd = new SQLiteCommand(SQL, conn);
                        InsertCmd.Parameters.Add(pDay);
                        SQLiteParameter pID = new SQLiteParameter();
                        pID.Value = tempID;
                        InsertCmd.Parameters.Add(pID);
                        SQLiteParameter personID = new SQLiteParameter();
                        personID.Value = ID;
                        InsertCmd.Parameters.Add(personID);
                        InsertCmd.Parameters.Add(pSN);
                        InsertCmd.Parameters.Add(pSNID);
                        SQLiteParameter pLastUpd = new SQLiteParameter();
                        pLastUpd.Value = tempD;
                        InsertCmd.Parameters.Add(pLastUpd);
                        int outrows = InsertCmd.ExecuteNonQuery();
                        if (outrows > 0)
                        {
                            // MessageBox.Show("DataSave in Table " + Table + " Insert: " + outrows);
                            SavedAfter = true;
                        }
                    }
                    ErrorMessage = "";
                    return;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.ToString()
                + "\n SNID: " + SNID;
                    //System.Windows.Forms.MessageBox.Show("Error in data save:" + ErrorMessage);
                }
            } // lock
            return;
        }

        public static bool VerifyExists(int SocialNetwork, string SNID, string Table, out int currentID)
        {
            currentID = -1;
            bool Exists = false;
            string Field;

            lock (obj)
            {
                GetReferenceTable(Table, out Field);

                try
                {
                    GetConn();
                    SQLiteCommand ExistsCmd = new SQLiteCommand(
                    "select " + Field + " from " + Table
                            + " where SNID=? and SocialNetwork=? and Active=1",
                            conn);
                    SQLiteParameter pSNID = new SQLiteParameter();
                    pSNID.Value = SNID;
                    ExistsCmd.Parameters.Add(pSNID);
                    SQLiteParameter pSN = new SQLiteParameter();
                    pSN.Value = SocialNetwork;
                    ExistsCmd.Parameters.Add(pSN);
                    SQLiteDataReader reader = ExistsCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            currentID = reader.GetInt32(0);
                            Exists = true;
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    //MessageBox.Show ( "Error verifying data in " + Table + ":\n" + ex.ToString() );
                    lastError = ex.ToString();
                }
            }
            return Exists;
        }

        public static bool PersonDataSave(decimal PartitionDate, int PartitionID,
            int Distance, string ProfilePic, string Link,
            string FirstName, string MiddleName, string LastName,
            string FullBirthday, string UserName, string Gender, string Locale,
            string RelStatus, string Religion, string Political,
            int? UserTimeZone, string About, string Bio, string Quotes, string Verified,
            DateTime? Updated,
            out bool Saved, out string ErrorMessage)
        {
            ErrorMessage = "";
            Saved = false;

            lock (obj)
            {
                try
                {
                    GetConn();

                    // birthday preprocessing
                    string birthDay = "", birthMonth = "", birthYear = "";
                    if (FullBirthday != null)
                    {
                        int bLen = FullBirthday.Length;
                        if (bLen >= 2)
                        {
                            birthMonth = FullBirthday.Substring(0, 2);
                            if (bLen >= 5)
                            {
                                birthDay = FullBirthday.Substring(3, 2);
                                if (bLen >= 10)
                                {
                                    birthYear = FullBirthday.Substring(6);
                                }
                            }
                        }
                        //MessageBox.Show("Processed birthday: " + birthDay + " / " + birthMonth + " / " + birthYear);
                    }

                    // update
                    string SQL = "Update Persondata set Verified=?";
                    SQLiteParameter pDistance = new SQLiteParameter();
                    SQLiteParameter pLink = new SQLiteParameter();
                    SQLiteParameter pPic = new SQLiteParameter();
                    SQLiteParameter pFirstName = new SQLiteParameter();
                    SQLiteParameter pMiddleName = new SQLiteParameter();
                    SQLiteParameter pLastName = new SQLiteParameter();
                    SQLiteParameter pUserName = new SQLiteParameter();
                    SQLiteParameter pGender = new SQLiteParameter();
                    SQLiteParameter pLocale = new SQLiteParameter();
                    SQLiteParameter pRelStatus = new SQLiteParameter();
                    SQLiteParameter pReligion = new SQLiteParameter();
                    SQLiteParameter pPolitical = new SQLiteParameter();
                    SQLiteParameter pUserTimeZone = new SQLiteParameter();
                    SQLiteParameter pAbout = new SQLiteParameter();
                    SQLiteParameter pBio = new SQLiteParameter();
                    SQLiteParameter pQuotes = new SQLiteParameter();
                    SQLiteParameter pBDay = new SQLiteParameter();
                    SQLiteParameter pBMonth = new SQLiteParameter();
                    SQLiteParameter pBYear = new SQLiteParameter();
                    SQLiteParameter pVerified = new SQLiteParameter();
                    SQLiteParameter pUpdated = new SQLiteParameter();

                    pDistance.Value = Distance;
                    pPic.Value = ProfilePic;
                    pLink.Value = Link;
                    pFirstName.Value = FirstName;
                    pMiddleName.Value = MiddleName;
                    pLastName.Value = LastName;
                    pUserName.Value = UserName;
                    pGender.Value = Gender;
                    pLocale.Value = Locale;
                    pRelStatus.Value = RelStatus;
                    pReligion.Value = Religion;
                    pPolitical.Value = Political;
                    pUserTimeZone.Value = UserTimeZone;
                    pAbout.Value = About;
                    pBio.Value = Bio;
                    pQuotes.Value = Quotes;
                    pBDay.Value = birthDay;
                    pBMonth.Value = birthMonth;
                    pBYear.Value = birthYear;
                    bool isVerified = (Verified == "true");
                    pVerified.Value = isVerified;
                    pUpdated.Value = Updated;

                    SQL += ", Distance=min(Distance,?)";
                    if (ProfilePic != null) SQL += ", ProfilePic=?";
                    if (Link != null) SQL += ", Link=?";
                    if (FirstName != null) SQL += ", FirstName=?";
                    if (MiddleName != null) SQL += ", MiddleName=?";
                    if (LastName != null) SQL += ", LastName=?";
                    if (UserName != null) SQL += ", UserName=?";
                    if (Gender != null) SQL += ", Gender=?";
                    if (Locale != null) SQL += ", Locale=?";
                    if (RelStatus != null) SQL += ", RelationshipStatus=?";
                    if (Religion != null) SQL += ", Religion=?";
                    if (Political != null) SQL += ", Political=?";
                    if (UserTimeZone != null) SQL += ", UserTimeZone=?";
                    if (About != null) SQL += ", About=?";
                    if (Bio != null) SQL += ", Bio=?";
                    if (Quotes != null) SQL += ", Quotes=?";
                    if (birthDay != "") SQL += ", BirthDay=?";
                    if (birthMonth != "") SQL += ", BirthMonth=?";
                    if (birthYear != "") SQL += ", BirthYear=?";
                    if (Updated != null) SQL += ", Updated=?";

                    SQL += " where PartitionDate=? and PartitionID=?";
                    SQLiteParameter pPartitionDate = new SQLiteParameter();
                    pPartitionDate.Value = PartitionDate;
                    SQLiteParameter pPartitionID = new SQLiteParameter();
                    pPartitionID.Value = PartitionID;

                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);

                    UpdateCmd.Parameters.Add(pVerified);
                    // if ( Distance != null ) 
                    UpdateCmd.Parameters.Add(pDistance);
                    if (ProfilePic != null) UpdateCmd.Parameters.Add(pPic);
                    if (Link != null) UpdateCmd.Parameters.Add(pLink);
                    if (FirstName != null) UpdateCmd.Parameters.Add(pFirstName);
                    if (MiddleName != null) UpdateCmd.Parameters.Add(pMiddleName);
                    if (LastName != null) UpdateCmd.Parameters.Add(pLastName);
                    // Birthday fields are here

                    if (UserName != null) UpdateCmd.Parameters.Add(pUserName);
                    if (Gender != null) UpdateCmd.Parameters.Add(pGender);
                    if (Locale != null) UpdateCmd.Parameters.Add(pLocale);
                    if (RelStatus != null) UpdateCmd.Parameters.Add(pRelStatus);
                    if (Religion != null) UpdateCmd.Parameters.Add(pReligion);
                    if (Political != null) UpdateCmd.Parameters.Add(pPolitical);
                    if (UserTimeZone != null) UpdateCmd.Parameters.Add(pUserTimeZone);
                    if (About != null) UpdateCmd.Parameters.Add(pAbout);
                    if (Bio != null) UpdateCmd.Parameters.Add(pBio);
                    if (Quotes != null) UpdateCmd.Parameters.Add(pQuotes);
                    if (birthDay != "") UpdateCmd.Parameters.Add(pBDay);
                    if (birthMonth != "") UpdateCmd.Parameters.Add(pBMonth);
                    if (birthYear != "") UpdateCmd.Parameters.Add(pBYear);
                    if (Updated != null) UpdateCmd.Parameters.Add(pUpdated);
                    UpdateCmd.Parameters.Add(pPartitionDate);
                    UpdateCmd.Parameters.Add(pPartitionID);

                    // System.Windows.Forms.MessageBox.Show("ready to execute: " + SQL + " affecting " + PartitionDate + " - " + PartitionID + " with bday: " + birthDay + " / " + birthMonth + " / " + birthYear);
                    int outrows = UpdateCmd.ExecuteNonQuery();
                    // DEBUG
                    // System.Windows.Forms.MessageBox.Show("saved " + outrows + " persons");
                    if (outrows > 0)
                    {
                        Saved = true;
                    }
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving Person Data\n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show(ErrorMessage);
                }
            } // lock
            return Saved;
        } // function

        public static bool UpdateProfilePic(long PersonID, string Path,
            out string ErrorMessage)
        {
            ErrorMessage = "";
            bool Saved = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // update
                    string SQL = "Update Persondata set ProfilePic=? where PersonID=?";
                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pID = new SQLiteParameter();
                    SQLiteParameter pPic = new SQLiteParameter();
                    pID.Value = PersonID;
                    pPic.Value = Path;
                    UpdateCmd.Parameters.Add(pPic);
                    UpdateCmd.Parameters.Add(pID);
                    UpdateCmd.ExecuteNonQuery();
                    Saved = true;
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving Profile pic\n" + ex.ToString();
                }
            } // lock
            return Saved;
        }

        public static void PostDataSave(decimal PartitionDate, int PartitionID,
            string FromID, string FromName, string ToID, string ToName, string Message,
            string Picture, string Link, string Caption, string Description,
            string Source, string Icon, string Attribution, string Privacy, string PrivacyValue,
            DateTime? Created, DateTime? Updated,
            string ActionsID, string ActionsName, string ApplicationID, string ApplicationName,
            string PostType, string ParentID, int? CommentCount, int? LikesCount,
            out bool Saved, out string ErrorMessage)
        {
            ErrorMessage = "";
            Saved = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // update
                    string SQL = "Update PostData set LastUpdate=?";
                    SQLiteParameter pLU = new SQLiteParameter();
                    SQLiteParameter pFromID = new SQLiteParameter();
                    SQLiteParameter pFromName = new SQLiteParameter();
                    SQLiteParameter pToID = new SQLiteParameter();
                    SQLiteParameter pToName = new SQLiteParameter();
                    SQLiteParameter pMessage = new SQLiteParameter();
                    SQLiteParameter pPicture = new SQLiteParameter();
                    SQLiteParameter pLink = new SQLiteParameter();
                    SQLiteParameter pCaption = new SQLiteParameter();
                    SQLiteParameter pDescription = new SQLiteParameter();
                    SQLiteParameter pSource = new SQLiteParameter();
                    SQLiteParameter pIcon = new SQLiteParameter();
                    SQLiteParameter pAttribution = new SQLiteParameter();
                    SQLiteParameter pPrivacy = new SQLiteParameter();
                    SQLiteParameter pPrivacyValue = new SQLiteParameter();
                    SQLiteParameter pCreated = new SQLiteParameter();
                    SQLiteParameter pUpdated = new SQLiteParameter();
                    SQLiteParameter pActionsID = new SQLiteParameter();
                    SQLiteParameter pActionsName = new SQLiteParameter();
                    SQLiteParameter pAppID = new SQLiteParameter();
                    SQLiteParameter pAppName = new SQLiteParameter();
                    SQLiteParameter pPostType = new SQLiteParameter();
                    SQLiteParameter pParentID = new SQLiteParameter();
                    SQLiteParameter pCommentCount = new SQLiteParameter();
                    SQLiteParameter pLikesCount = new SQLiteParameter();

                    pLU.Value = DateTime.Now;
                    pFromID.Value = FromID;
                    pFromName.Value = FromName;
                    pToID.Value = ToID;
                    pToName.Value = ToName;
                    pMessage.Value = Message;
                    pPicture.Value = Picture;
                    pLink.Value = Link;
                    pCaption.Value = Caption;
                    pDescription.Value = Description;
                    pSource.Value = Source;
                    pIcon.Value = Icon;
                    pAttribution.Value = Attribution;
                    pPrivacy.Value = Privacy;
                    pPrivacyValue.Value = PrivacyValue;
                    pCreated.Value = Created;
                    pUpdated.Value = Updated;
                    pActionsID.Value = ActionsID;
                    pActionsName.Value = ActionsName;
                    pAppID.Value = ApplicationID;
                    pAppName.Value = ApplicationName;
                    pPostType.Value = PostType;
                    pParentID.Value = ParentID;
                    pCommentCount.Value = CommentCount;
                    pLikesCount.Value = LikesCount;

                    if (FromID != null) SQL += ", FromID=?";
                    if (FromName != null) SQL += ", FromName=?";
                    if (ToID != null) SQL += ", ToID=?";
                    if (ToName != null) SQL += ", ToName=?";
                    if (Message != null) SQL += ", Message=?";
                    if (Picture != null) SQL += ", Picture=?";
                    if (Link != null) SQL += ", Link=?";
                    if (Caption != null) SQL += ", Caption=?";
                    if (Description != null) SQL += ", Description=?";
                    if (Source != null) SQL += ", Source=?";
                    if (Icon != null) SQL += ", Icon=?";
                    if (Attribution != null) SQL += ", Attribution=?";
                    if (Privacy != null) SQL += ", Privacy =?";
                    if (PrivacyValue != null) SQL += ", PrivacyValue =?";
                    if (Created != null) SQL += ", Created=?";
                    if (Updated != null) SQL += ", Updated=?";
                    if (ActionsID != null) SQL += ", ActionsID=?";
                    if (ActionsName != null) SQL += ", ActionsName=?";
                    if (ApplicationID != null) SQL += ", ApplicationID=?";
                    if (ApplicationName != null) SQL += ", ApplicationName=?";
                    if (PostType != null) SQL += ", PostType=?";
                    if (ParentID != null) SQL += ", ParentID=?";
                    if (CommentCount != null) SQL += ", CommentCount=?";
                    if (LikesCount != null) SQL += ", LikesCount=?";

                    SQL += " where PartitionDate=? and PartitionID=?";
                    SQLiteParameter pPartitionDate = new SQLiteParameter();
                    pPartitionDate.Value = PartitionDate;
                    SQLiteParameter pPartitionID = new SQLiteParameter();
                    pPartitionID.Value = PartitionID;

                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);

                    UpdateCmd.Parameters.Add(pLU);
                    if (FromID != null) UpdateCmd.Parameters.Add(pFromID);
                    if (FromName != null) UpdateCmd.Parameters.Add(pFromName);
                    if (ToID != null) UpdateCmd.Parameters.Add(pToID);
                    if (ToName != null) UpdateCmd.Parameters.Add(pToName);
                    if (Message != null) UpdateCmd.Parameters.Add(pMessage);
                    if (Picture != null) UpdateCmd.Parameters.Add(pPicture);
                    if (Link != null) UpdateCmd.Parameters.Add(pLink);
                    if (Caption != null) UpdateCmd.Parameters.Add(pCaption);
                    if (Description != null) UpdateCmd.Parameters.Add(pDescription);
                    if (Source != null) UpdateCmd.Parameters.Add(pSource);
                    if (Icon != null) UpdateCmd.Parameters.Add(pIcon);
                    if (Attribution != null) UpdateCmd.Parameters.Add(pAttribution);
                    if (Privacy != null) UpdateCmd.Parameters.Add(pPrivacy);
                    if (PrivacyValue != null) UpdateCmd.Parameters.Add(pPrivacyValue);
                    if (Created != null) UpdateCmd.Parameters.Add(pCreated);
                    if (Updated != null) UpdateCmd.Parameters.Add(pUpdated);
                    if (ActionsID != null) UpdateCmd.Parameters.Add(pActionsID);
                    if (ActionsName != null) UpdateCmd.Parameters.Add(pActionsName);
                    if (ApplicationID != null) UpdateCmd.Parameters.Add(pAppID);
                    if (ApplicationName != null) UpdateCmd.Parameters.Add(pAppName);
                    if (PostType != null) UpdateCmd.Parameters.Add(pPostType);
                    if (ParentID != null) UpdateCmd.Parameters.Add(pParentID);
                    if (CommentCount != null) UpdateCmd.Parameters.Add(pCommentCount);
                    if (LikesCount != null) UpdateCmd.Parameters.Add(pLikesCount);

                    UpdateCmd.Parameters.Add(pPartitionDate);
                    UpdateCmd.Parameters.Add(pPartitionID);

                    int outrows = UpdateCmd.ExecuteNonQuery();
                    // DEBUG
                    // MessageBox.Show("saved " + outrows + " posts");
                    if (outrows > 0)
                    {
                        Saved = true;
                        // Adding FTS records
                        SQL = "insert into FTSPostData (PostID,Message,Description,Created) select PostID,Message, Description, Created from PostData where PartitionDate=? and PartitionID=?;";
                        SQLiteCommand FTSCmd = new SQLiteCommand(SQL, conn);
                        FTSCmd.Parameters.Add(pPartitionDate);
                        FTSCmd.Parameters.Add(pPartitionID);

                        outrows = FTSCmd.ExecuteNonQuery();
                        if (outrows == 0)
                        {
                            //System.Windows.Forms.MessageBox.Show("error inserting FTS PostData");
                        }
                    }
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving Post\n" + ex.ToString();
                    // MessageBox.Show(ErrorMessage);
                }
            } // lock
            return;
        }

        public static void MessageDataSave(decimal PartitionDate, int PartitionID,
            string FromID, string FromName, string FromEmail, string ToID, string ToName, string ToEmail,
            string Message, string Subject,
            DateTime? Created, DateTime? Updated, string ParentID,
            out bool Saved, out string ErrorMessage)
        {
            NoteDataSave(PartitionDate, PartitionID, FromID, FromName, FromEmail, ToID, ToName, ToEmail,
                    Message, Subject, null, Created, Updated, ParentID, "MessageData", out Saved, out ErrorMessage);
        }

        public static void NoteDataSave(decimal PartitionDate, int PartitionID,
            string FromID, string FromName, string FromEmail, string ToID, string ToName, string ToEmail,
            string Message, string Subject, string Icon,
            DateTime? Created, DateTime? Updated, string ParentID, string Table,
            out bool Saved, out string ErrorMessage)
        {
            ErrorMessage = "";
            Saved = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // update
                    string SQL = "Update " + Table + " set LastUpdate=?";
                    SQLiteParameter pLU = new SQLiteParameter();
                    SQLiteParameter pFromID = new SQLiteParameter();
                    SQLiteParameter pFromName = new SQLiteParameter();
                    SQLiteParameter pFromEmail = new SQLiteParameter();
                    SQLiteParameter pToID = new SQLiteParameter();
                    SQLiteParameter pToName = new SQLiteParameter();
                    SQLiteParameter pToEmail = new SQLiteParameter();
                    SQLiteParameter pMessage = new SQLiteParameter();
                    SQLiteParameter pSubject = new SQLiteParameter();
                    SQLiteParameter pIcon = new SQLiteParameter();
                    SQLiteParameter pCreated = new SQLiteParameter();
                    SQLiteParameter pUpdated = new SQLiteParameter();
                    SQLiteParameter pParentID = new SQLiteParameter();

                    pLU.Value = DateTime.Now;
                    pFromID.Value = FromID;
                    pFromName.Value = FromName;
                    pFromEmail.Value = FromEmail;
                    pToID.Value = ToID;
                    pToName.Value = ToName;
                    pToEmail.Value = ToEmail;
                    pMessage.Value = Message;
                    pSubject.Value = Subject;
                    pIcon.Value = Icon;
                    pCreated.Value = Created;
                    pUpdated.Value = Updated;
                    pParentID.Value = ParentID;

                    if (FromID != null) SQL += ", FromID=?";
                    if (FromName != null) SQL += ", FromName=?";
                    if (FromEmail != null) SQL += ", FromEmail=?";
                    if (ToID != null) SQL += ", ToID=?";
                    if (ToName != null) SQL += ", ToName=?";
                    if (ToEmail != null) SQL += ", ToEmail=?";
                    if (Message != null) SQL += ", Message=?";
                    if (Subject != null) SQL += ", Subject=?";
                    if (Icon != null) SQL += ", Icon=?";
                    if (Created != null) SQL += ", Created=?";
                    if (Updated != null) SQL += ", Updated=?";
                    if (ParentID != null) SQL += ", ParentID=?";

                    SQL += " where PartitionDate=? and PartitionID=?";
                    SQLiteParameter pPartitionDate = new SQLiteParameter();
                    pPartitionDate.Value = PartitionDate;
                    SQLiteParameter pPartitionID = new SQLiteParameter();
                    pPartitionID.Value = PartitionID;

                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);

                    UpdateCmd.Parameters.Add(pLU);
                    if (FromID != null) UpdateCmd.Parameters.Add(pFromID);
                    if (FromName != null) UpdateCmd.Parameters.Add(pFromName);
                    if (FromEmail != null) UpdateCmd.Parameters.Add(pFromEmail);
                    if (ToID != null) UpdateCmd.Parameters.Add(pToID);
                    if (ToName != null) UpdateCmd.Parameters.Add(pToName);
                    if (ToEmail != null) UpdateCmd.Parameters.Add(pToEmail);
                    if (Message != null) UpdateCmd.Parameters.Add(pMessage);
                    if (Subject != null) UpdateCmd.Parameters.Add(pSubject);
                    if (Icon != null) UpdateCmd.Parameters.Add(pIcon);
                    if (Created != null) UpdateCmd.Parameters.Add(pCreated);
                    if (Updated != null) UpdateCmd.Parameters.Add(pUpdated);
                    if (ParentID != null) UpdateCmd.Parameters.Add(pParentID);

                    UpdateCmd.Parameters.Add(pPartitionDate);
                    UpdateCmd.Parameters.Add(pPartitionID);

                    int outrows = UpdateCmd.ExecuteNonQuery();
                    // DEBUG
                    //System.Windows.Forms.MessageBox.Show("SQL: " + SQL);
                    //System.Windows.Forms.MessageBox.Show("saved " + outrows + " messages");
                    if (outrows > 0)
                    {
                        Saved = true;
                        ErrorMessage = "";
                        // Adding FTS records
                        if (Table == "MessageData")
                        {
                            SQL = "insert into FTSMessageData (MessageID, Message, Created) select MessageID, Message, Created from MessageData where PartitionDate=? and PartitionID=?;";
                            SQLiteCommand FTSCmd = new SQLiteCommand(SQL, conn);
                            FTSCmd.Parameters.Add(pPartitionDate);
                            FTSCmd.Parameters.Add(pPartitionID);

                            outrows = FTSCmd.ExecuteNonQuery();
                            if (outrows == 0)
                            {
                                //System.Windows.Forms.MessageBox.Show("error inserting FTS MessageData");
                            }
                        }
                    }
                    else
                    {
                        ErrorMessage = "0 rows saved on table " + Table;
                    }
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving message or note (" + Table + ")\n" + ex.ToString();
                    // DEBUG
                    //System.Windows.Forms.MessageBox.Show(ErrorMessage);
                }
            } // lock
            return;
        }

        public static void NotificationDataSave(decimal PartitionDate, int PartitionID,
            string FromID, string FromName, string FromEmail, string ToID, string ToName, string ToEmail,
            string Title, string Link, string AppName, int Unread,
            DateTime? Created, DateTime? Updated, string ParentID,
            out bool Saved, out string ErrorMessage)
        {
            ErrorMessage = "";
            Saved = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // update
                    string SQL = "Update NotificationData set LastUpdate=?";
                    SQLiteParameter pLU = new SQLiteParameter();
                    SQLiteParameter pFromID = new SQLiteParameter();
                    SQLiteParameter pFromName = new SQLiteParameter();
                    SQLiteParameter pFromEmail = new SQLiteParameter();
                    SQLiteParameter pToID = new SQLiteParameter();
                    SQLiteParameter pToName = new SQLiteParameter();
                    SQLiteParameter pToEmail = new SQLiteParameter();
                    SQLiteParameter pTitle = new SQLiteParameter();
                    SQLiteParameter pLink = new SQLiteParameter();
                    SQLiteParameter pAppName = new SQLiteParameter();
                    SQLiteParameter pUnread = new SQLiteParameter();
                    SQLiteParameter pCreated = new SQLiteParameter();
                    SQLiteParameter pUpdated = new SQLiteParameter();
                    SQLiteParameter pParentID = new SQLiteParameter();

                    pLU.Value = DateTime.Now;
                    pFromID.Value = FromID;
                    pFromName.Value = FromName;
                    pFromEmail.Value = FromEmail;
                    pToID.Value = ToID;
                    pToName.Value = ToName;
                    pToEmail.Value = ToEmail;
                    pTitle.Value = Title;
                    pLink.Value = Link;
                    pAppName.Value = AppName;
                    pUnread.Value = Unread;
                    pCreated.Value = Created;
                    pUpdated.Value = Updated;
                    pParentID.Value = ParentID;

                    if (FromID != null) SQL += ", FromID=?";
                    if (FromName != null) SQL += ", FromName=?";
                    if (FromEmail != null) SQL += ", FromEmail=?";
                    if (ToID != null) SQL += ", ToID=?";
                    if (ToName != null) SQL += ", ToName=?";
                    if (ToEmail != null) SQL += ", ToEmail=?";
                    if (Title != null) SQL += ", Title=?";
                    if (Link != null) SQL += ", Link=?";
                    if (AppName != null) SQL += ", AppName=?";
                    SQL += ", Unread =?";
                    if (Created != null) SQL += ", Created=?";
                    if (Updated != null) SQL += ", Updated=?";
                    if (ParentID != null) SQL += ", ParentID=?";

                    SQL += " where PartitionDate=? and PartitionID=?";
                    SQLiteParameter pPartitionDate = new SQLiteParameter();
                    pPartitionDate.Value = PartitionDate;
                    SQLiteParameter pPartitionID = new SQLiteParameter();
                    pPartitionID.Value = PartitionID;

                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);

                    UpdateCmd.Parameters.Add(pLU);
                    if (FromID != null) UpdateCmd.Parameters.Add(pFromID);
                    if (FromName != null) UpdateCmd.Parameters.Add(pFromName);
                    if (FromEmail != null) UpdateCmd.Parameters.Add(pFromEmail);
                    if (ToID != null) UpdateCmd.Parameters.Add(pToID);
                    if (ToName != null) UpdateCmd.Parameters.Add(pToName);
                    if (ToEmail != null) UpdateCmd.Parameters.Add(pToEmail);
                    if (Title != null) UpdateCmd.Parameters.Add(pTitle);
                    if (Link != null) UpdateCmd.Parameters.Add(pLink);
                    if (AppName != null) UpdateCmd.Parameters.Add(pAppName);
                    UpdateCmd.Parameters.Add(pUnread);
                    if (Created != null) UpdateCmd.Parameters.Add(pCreated);
                    if (Updated != null) UpdateCmd.Parameters.Add(pUpdated);
                    if (ParentID != null) UpdateCmd.Parameters.Add(pParentID);

                    UpdateCmd.Parameters.Add(pPartitionDate);
                    UpdateCmd.Parameters.Add(pPartitionID);

                    int outrows = UpdateCmd.ExecuteNonQuery();
                    // DEBUG
                    //System.Windows.Forms.MessageBox.Show("SQL: " + SQL);
                    //System.Windows.Forms.MessageBox.Show("saved " + outrows + " messages");
                    if (outrows > 0)
                    {
                        Saved = true;
                    }
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving notification\n" + ex.ToString();
                    // DEBUG
                    //System.Windows.Forms.MessageBox.Show(ErrorMessage);
                }
            } // lock
            return;
        }

        public static void EventDataSave(decimal PartitionDate, int PartitionID,
            string Description, string Location, DateTime? StartTime, DateTime? EndTime,
            DateTime? Created, DateTime? Updated, string ParentID,
            out bool Saved, out string ErrorMessage)
        {
            ErrorMessage = "";
            Saved = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // update
                    string SQL = "Update EventData set LastUpdate=?";
                    SQLiteParameter pLU = new SQLiteParameter();
                    SQLiteParameter pDescription = new SQLiteParameter();
                    SQLiteParameter pLocation = new SQLiteParameter();
                    SQLiteParameter pStartTime = new SQLiteParameter();
                    SQLiteParameter pEndTime = new SQLiteParameter();
                    SQLiteParameter pCreated = new SQLiteParameter();
                    SQLiteParameter pUpdated = new SQLiteParameter();
                    SQLiteParameter pParentID = new SQLiteParameter();

                    pLU.Value = DateTime.Now;
                    pDescription.Value = Description;
                    pLocation.Value = Location;
                    pStartTime.Value = StartTime;
                    pEndTime.Value = EndTime;
                    pCreated.Value = Created;
                    pUpdated.Value = Updated;
                    pParentID.Value = ParentID;

                    if (Description != null) SQL += ", Description=?";
                    if (Location != null) SQL += ", Location=?";
                    if (StartTime != null) SQL += ", StartTime=?";
                    if (EndTime != null) SQL += ", EndTime=?";
                    if (Created != null) SQL += ", Created=?";
                    if (Updated != null) SQL += ", Updated=?";
                    if (ParentID != null) SQL += ", ParentID=?";

                    SQL += " where PartitionDate=? and PartitionID=?";
                    SQLiteParameter pPartitionDate = new SQLiteParameter();
                    pPartitionDate.Value = PartitionDate;
                    SQLiteParameter pPartitionID = new SQLiteParameter();
                    pPartitionID.Value = PartitionID;

                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);

                    UpdateCmd.Parameters.Add(pLU);
                    if (Description != null) UpdateCmd.Parameters.Add(pDescription);
                    if (Location != null) UpdateCmd.Parameters.Add(pLocation);
                    if (StartTime != null) UpdateCmd.Parameters.Add(pStartTime);
                    if (EndTime != null) UpdateCmd.Parameters.Add(pEndTime);
                    if (Created != null) UpdateCmd.Parameters.Add(pCreated);
                    if (Updated != null) UpdateCmd.Parameters.Add(pUpdated);
                    if (ParentID != null) UpdateCmd.Parameters.Add(pParentID);

                    UpdateCmd.Parameters.Add(pPartitionDate);
                    UpdateCmd.Parameters.Add(pPartitionID);

                    int outrows = UpdateCmd.ExecuteNonQuery();
                    // DEBUG
                    //System.Windows.Forms.MessageBox.Show("SQL: " + SQL);
                    //System.Windows.Forms.MessageBox.Show("saved " + outrows + " messages");
                    if (outrows > 0)
                    {
                        Saved = true;
                        // Adding FTS records
                        SQL = "insert into FTSEventDAta (EventID, Description, Location, EventDate) select EventID, Description, Location, StartTime from EventData where PartitionDate=? and PartitionID=?;";
                        SQLiteCommand FTSCmd = new SQLiteCommand(SQL, conn);
                        FTSCmd.Parameters.Add(pPartitionDate);
                        FTSCmd.Parameters.Add(pPartitionID);

                        outrows = FTSCmd.ExecuteNonQuery();
                        if (outrows == 0)
                        {
                            //System.Windows.Forms.MessageBox.Show("error inserting FTS EventData");
                        }

                    }
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving event\n" + ex.ToString();
                    // DEBUG
                    //System.Windows.Forms.MessageBox.Show(ErrorMessage);
                }
            } // lock
            return;
        }

        public static void RelationSave(int SubjectID, int VerbID,
            int? ObjectID, string Adverb, string IndirectObject,
            string StartTime, string EndTime,
            out decimal PartitionDate, out int PartitionID,
            out bool SavedAfter, out string ErrorMessage)
        {
            int tempID = 0;
            ErrorMessage = "";
            SavedAfter = false;
            int ReferenceTable = 4;
            string SQL;

            lock (obj)
            {

                SQL = "insert into DailyID (Day, ReferenceTable, FreeID) values (?, ?, 2)";

                PartitionID = tempID;
                DateTime tempD = DateTime.Now;
                PartitionDate = tempD.Year * 10000 +
                tempD.Month * 100 + tempD.Day;
                try
                {
                    GetConn();
                    // TODO: Transactions for optimization
                    // TODO: mark adecuately active -> historic
                    string SQLExists = "select PartitionDate, PartitionID from RelationsData"
                            + " where SubjectID=? and VerbID=? and Active=1";
                    if (ObjectID != null)
                    {
                        SQLExists += " and ObjectID=?";
                        SQLExists += " and Adverb=?";
                        SQLExists += " and IndirectObject=?";
                    }
                    else
                        SQLExists += " and ObjectID is null";

                    SQLiteCommand ExistsCmd = new SQLiteCommand(SQLExists, conn);

                    SQLiteParameter pSID = new SQLiteParameter();
                    pSID.Value = SubjectID;
                    ExistsCmd.Parameters.Add(pSID);
                    SQLiteParameter pVID = new SQLiteParameter();
                    pVID.Value = VerbID;
                    ExistsCmd.Parameters.Add(pVID);
                    SQLiteParameter pOID = new SQLiteParameter();
                    SQLiteParameter pAdv = new SQLiteParameter();
                    SQLiteParameter pOI = new SQLiteParameter();
                    if (ObjectID != null)
                    {
                        pOID.Value = ObjectID;
                        ExistsCmd.Parameters.Add(pOID);
                        pAdv.Value = Adverb;
                        ExistsCmd.Parameters.Add(pAdv);
                        pOI.Value = IndirectObject;
                        ExistsCmd.Parameters.Add(pOI);
                    }
                    SQLiteDataReader prevRecord = ExistsCmd.ExecuteReader();
                    SQLiteParameter pDay = new SQLiteParameter();
                    pDay.Value = PartitionDate;

                    if (prevRecord.Read() && !prevRecord.IsDBNull(0))
                    {
                        PartitionDate = prevRecord.GetDecimal(0);
                        PartitionID = prevRecord.GetInt32(1);
                        prevRecord.Close();
                        // now Insert into appropriate table
                        // TODO: make it smarter insert/update behaviour, for incremental backup
                        // TODO: update other fields!
                        SQL = "update RelationsData"
                        + " set LastUpdate=? where PartitionDate=? and PartitionID=?";
                        SQLiteCommand InsertCmd = new SQLiteCommand(SQL, conn);
                        SQLiteParameter pLastUpd = new SQLiteParameter();
                        pLastUpd.Value = tempD;
                        InsertCmd.Parameters.Add(pLastUpd);
                        pDay.Value = PartitionDate;
                        InsertCmd.Parameters.Add(pDay);
                        SQLiteParameter pPID = new SQLiteParameter();
                        pPID.Value = PartitionID;
                        InsertCmd.Parameters.Add(pPID);
                        int outrows = InsertCmd.ExecuteNonQuery();
                        if (outrows > 0)
                        {
                            SavedAfter = true;
                        }
                    }
                    else
                    {
                        prevRecord.Close();
                        // First part: get next ID for the appropriate table
                        // TODO: separate function
                        SQLiteCommand ReadCmd = new SQLiteCommand(
                        "select FreeID from DailyID  where Day=? and ReferenceTable=?",
                            conn);
                        ReadCmd.Parameters.Add(pDay);
                        SQLiteParameter pRefTable = new SQLiteParameter();
                        pRefTable.Value = ReferenceTable;
                        ReadCmd.Parameters.Add(pRefTable);
                        SQLiteDataReader reader = ReadCmd.ExecuteReader();

                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                tempID = reader.GetInt32(0);
                                SQL = "update DailyID set FreeID=FreeID+1 where Day=? and ReferenceTable=?";
                            }
                        }
                        else
                        {
                            tempID = 1; // first execution, I am #1, but free is 2
                        }
                        PartitionID = tempID;
                        reader.Close();
                        // DEBUG
                        // MessageBox.Show("Updating to Date: " + pDay.Value + " ID: " + tempID );
                        SQLiteCommand InsertCmd = new SQLiteCommand(SQL, conn);
                        InsertCmd.Parameters.Add(pDay);
                        InsertCmd.Parameters.Add(pRefTable);
                        InsertCmd.ExecuteNonQuery();
                        // now Insert into appropriate table
                        // TODO: all fields
                        SQL = "insert into RelationsData "
                        + "(PartitionDate, PartitionID, SubjectID, VerbID, ObjectID, Adverb, IndirectObject, Active, Created, LastUpdate)"
                        + " values (?, ?, ?, ?, ?, ?, ?, 1, ?, ?)";
                        // note - reusing
                        InsertCmd = new SQLiteCommand(SQL, conn);
                        InsertCmd.Parameters.Add(pDay);
                        SQLiteParameter pID = new SQLiteParameter();
                        pID.Value = tempID;
                        InsertCmd.Parameters.Add(pID);
                        InsertCmd.Parameters.Add(pSID);
                        InsertCmd.Parameters.Add(pVID);
                        if (ObjectID != null)
                        {
                            pOID.Value = ObjectID;
                            pAdv.Value = Adverb;
                            pOI.Value = IndirectObject;
                        }
                        InsertCmd.Parameters.Add(pOID);
                        InsertCmd.Parameters.Add(pAdv);
                        InsertCmd.Parameters.Add(pOI);
                        SQLiteParameter pLastUpd = new SQLiteParameter();
                        pLastUpd.Value = tempD;
                        // twice: Created and LastUpd equal first time
                        InsertCmd.Parameters.Add(pLastUpd);
                        InsertCmd.Parameters.Add(pLastUpd);

                        int outrows = InsertCmd.ExecuteNonQuery();
                        if (outrows > 0)
                        {
                            SavedAfter = true;
                        }
                    }
                    ErrorMessage = "";
                    return;
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving relation to the database\n" + SQL
                + "\nDate: " + PartitionDate
                + "\nID: " + PartitionID
                + "\n" + ex.ToString();
                }
            } // lock
            return;
        }

        public static void AlbumDataSave(decimal PartitionDate, int PartitionID,
            string FromID, string FromName, string Description,
            string Location, string Link, int Count, string Privacy, string PrivacyValue,
            DateTime? Created, DateTime? Updated,
            string Path, string CoverPicture,
            string AlbumType,
            out bool Saved, out string ErrorMessage)
        {
            ErrorMessage = "";
            Saved = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // update
                    string SQL = "Update AlbumData set LastUpdate=?";
                    SQLiteParameter pLU = new SQLiteParameter();
                    SQLiteParameter pFromID = new SQLiteParameter();
                    SQLiteParameter pFromName = new SQLiteParameter();
                    SQLiteParameter pDescription = new SQLiteParameter();
                    SQLiteParameter pLocation = new SQLiteParameter();
                    SQLiteParameter pLink = new SQLiteParameter();
                    SQLiteParameter pCount = new SQLiteParameter();
                    SQLiteParameter pPrivacy = new SQLiteParameter();
                    SQLiteParameter pPrivacyValue = new SQLiteParameter();
                    SQLiteParameter pCreated = new SQLiteParameter();
                    SQLiteParameter pUpdated = new SQLiteParameter();
                    SQLiteParameter pPath = new SQLiteParameter();
                    SQLiteParameter pCoverPicture = new SQLiteParameter();
                    SQLiteParameter pAlbumType = new SQLiteParameter();

                    pLU.Value = DateTime.Now;
                    pFromID.Value = FromID;
                    pFromName.Value = FromName;
                    pDescription.Value = Description;
                    pLocation.Value = Location;
                    pLink.Value = Link;
                    pCount.Value = Count;
                    pPrivacy.Value = Privacy;
                    pPrivacyValue.Value = PrivacyValue;
                    pCreated.Value = Created;
                    pUpdated.Value = Updated;
                    pPath.Value = Path;
                    pCoverPicture.Value = CoverPicture;
                    pAlbumType.Value = AlbumType;

                    if (FromID != null) SQL += ", FromID=?";
                    if (FromName != null) SQL += ", FromName=?";
                    if (Description != null) SQL += ", Description=?";
                    if (Location != null) SQL += ", Location=?";
                    if (Link != null) SQL += ", Link=?";
                    SQL += ", PhotoCount=?";
                    if (Privacy != null) SQL += ", Privacy =?";
                    if (PrivacyValue != null) SQL += ", PrivacyValue =?";
                    if (Created != null) SQL += ", Created=?";
                    if (Updated != null) SQL += ", Updated=?";
                    if (Path != null) SQL += ", Path=?";
                    if (CoverPicture != null) SQL += ", CoverPicture=?";
                    if (AlbumType != null) SQL += ", AlbumType=?";

                    SQL += " where PartitionDate=? and PartitionID=?";
                    SQLiteParameter pPartitionDate = new SQLiteParameter();
                    pPartitionDate.Value = PartitionDate;
                    SQLiteParameter pPartitionID = new SQLiteParameter();
                    pPartitionID.Value = PartitionID;

                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);

                    UpdateCmd.Parameters.Add(pLU);
                    if (FromID != null) UpdateCmd.Parameters.Add(pFromID);
                    if (FromName != null) UpdateCmd.Parameters.Add(pFromName);
                    if (Description != null) UpdateCmd.Parameters.Add(pDescription);
                    if (Location != null) UpdateCmd.Parameters.Add(pLocation);
                    if (Link != null) UpdateCmd.Parameters.Add(pLink);
                    UpdateCmd.Parameters.Add(pCount);
                    if (Privacy != null) UpdateCmd.Parameters.Add(pPrivacy);
                    if (PrivacyValue != null) UpdateCmd.Parameters.Add(pPrivacyValue);
                    if (Created != null) UpdateCmd.Parameters.Add(pCreated);
                    if (Updated != null) UpdateCmd.Parameters.Add(pUpdated);
                    if (Path != null) UpdateCmd.Parameters.Add(pPath);
                    if (CoverPicture != null) UpdateCmd.Parameters.Add(pCoverPicture);
                    if (AlbumType != null) UpdateCmd.Parameters.Add(pAlbumType);

                    UpdateCmd.Parameters.Add(pPartitionDate);
                    UpdateCmd.Parameters.Add(pPartitionID);

                    int outrows = UpdateCmd.ExecuteNonQuery();
                    if (outrows > 0)
                    {
                        Saved = true;
                        // Adding FTS records
                        SQL = "insert into FTSAlbumData (AlbumID,Name,Description,Created) select B.AlbumID,A.Name,B.Description, B.Created from Entities A, AlbumData B where A.ID = B.AlbumID and PartitionDate=? and PartitionID=?;";
                        SQLiteCommand FTSCmd = new SQLiteCommand(SQL, conn);
                        FTSCmd.Parameters.Add(pPartitionDate);
                        FTSCmd.Parameters.Add(pPartitionID);

                        outrows = FTSCmd.ExecuteNonQuery();
                        if (outrows == 0)
                        {
                            //System.Windows.Forms.MessageBox.Show("error inserting FTS AlbumData");
                        }
                    }
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving Album\n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show(ErrorMessage);
                }
            } // lock
            return;
        }

        public static void PhotoDataSave(decimal PartitionDate, int PartitionID,
            string FromID, string FromName, string Icon, string Source, string Link,
            int? Height, int? Width,
            DateTime? Created, DateTime? Updated,
            string Path, long? ParentID, string ParentSNID,
            out bool Saved, out string ErrorMessage)
        {
            ErrorMessage = "";
            Saved = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // update
                    string SQL = "Update PhotoData set LastUpdate=?";
                    SQLiteParameter pLU = new SQLiteParameter();
                    SQLiteParameter pFromID = new SQLiteParameter();
                    SQLiteParameter pFromName = new SQLiteParameter();
                    SQLiteParameter pIcon = new SQLiteParameter();
                    SQLiteParameter pSource = new SQLiteParameter();
                    SQLiteParameter pLink = new SQLiteParameter();
                    SQLiteParameter pHeight = new SQLiteParameter();
                    SQLiteParameter pWidth = new SQLiteParameter();
                    SQLiteParameter pCreated = new SQLiteParameter();
                    SQLiteParameter pUpdated = new SQLiteParameter();
                    SQLiteParameter pPath = new SQLiteParameter();
                    SQLiteParameter pPID = new SQLiteParameter();
                    SQLiteParameter pPSNID = new SQLiteParameter();

                    pLU.Value = DateTime.Now;
                    pFromID.Value = FromID;
                    pFromName.Value = FromName;
                    pIcon.Value = Icon;
                    pSource.Value = Source;
                    pLink.Value = Link;
                    pHeight.Value = Height;
                    pWidth.Value = Width;
                    pCreated.Value = Created;
                    pUpdated.Value = Updated;
                    pPath.Value = Path;
                    pPID.Value = ParentID;
                    pPSNID.Value = ParentSNID;

                    if (FromID != null) SQL += ", FromID=?";
                    if (FromName != null) SQL += ", FromName=?";
                    if (Icon != null) SQL += ", Icon=?";
                    if (Source != null) SQL += ", Source=?";
                    if (Link != null) SQL += ", Link=?";
                    if (Height != null) SQL += ", Height=?";
                    if (Width != null) SQL += ", Width=?";
                    if (Created != null) SQL += ", Created=?";
                    if (Updated != null) SQL += ", Updated=?";
                    if (Path != null) SQL += ", Path=?";
                    if (ParentID != null) SQL += ", ParentID=?";
                    if (ParentSNID != null) SQL += ", ParentSNID=?";

                    SQL += " where PartitionDate=? and PartitionID=?";
                    SQLiteParameter pPartitionDate = new SQLiteParameter();
                    pPartitionDate.Value = PartitionDate;
                    SQLiteParameter pPartitionID = new SQLiteParameter();
                    pPartitionID.Value = PartitionID;

                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);

                    UpdateCmd.Parameters.Add(pLU);
                    if (FromID != null) UpdateCmd.Parameters.Add(pFromID);
                    if (FromName != null) UpdateCmd.Parameters.Add(pFromName);
                    if (Icon != null) UpdateCmd.Parameters.Add(pIcon);
                    if (Source != null) UpdateCmd.Parameters.Add(pSource);
                    if (Link != null) UpdateCmd.Parameters.Add(pLink);
                    if (Height != null) UpdateCmd.Parameters.Add(pHeight);
                    if (Width != null) UpdateCmd.Parameters.Add(pWidth);
                    if (Created != null) UpdateCmd.Parameters.Add(pCreated);
                    if (Updated != null) UpdateCmd.Parameters.Add(pUpdated);
                    if (Path != null) UpdateCmd.Parameters.Add(pPath);
                    if (ParentID != null) UpdateCmd.Parameters.Add(pPID);
                    if (ParentSNID != null) UpdateCmd.Parameters.Add(pPSNID);

                    UpdateCmd.Parameters.Add(pPartitionDate);
                    UpdateCmd.Parameters.Add(pPartitionID);

                    int outrows = UpdateCmd.ExecuteNonQuery();
                    if (outrows > 0)
                    {
                        Saved = true;
                        // Adding FTS records
                        SQL = "insert into FTSPhotoData (PhotoID,Name,Created) select B.PhotoID,A.Name, B.Created from Entities A, PhotoData B where A.ID = B.PhotoID and PartitionDate=? and PartitionID=?;";
                        SQLiteCommand FTSCmd = new SQLiteCommand(SQL, conn);
                        FTSCmd.Parameters.Add(pPartitionDate);
                        FTSCmd.Parameters.Add(pPartitionID);

                        outrows = FTSCmd.ExecuteNonQuery();
                        if (outrows == 0)
                        {
                            //System.Windows.Forms.MessageBox.Show("error inserting FTS AlbumData");
                        }
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("didn't save photo " + PartitionDate + "," + PartitionID);
                    }
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving Photo\n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show(ErrorMessage);
                }
            } // lock
            return;
        }

        public static void TagDataSave(
            string PersonSNID, string PhotoSNID,
            float? X, float? Y,
            DateTime? Created, DateTime? Updated,
            out bool Saved, out string ErrorMessage)
        {
            decimal PartitionDate;
            int PartitionID = -1;
            bool Exists = false;

            ErrorMessage = "";
            Saved = false;
            DateTime tempD = DateTime.Now;
            PartitionDate = tempD.Year * 10000 + tempD.Month * 100 + tempD.Day;

            lock (obj)
            {
                try
                {
                    GetConn();
                    SQLiteCommand ExistsCmd = new SQLiteCommand(
                    "select PartitionDate, PartitionID from TagData where PersonSNID=? and PhotoSNID=? and SocialNetwork=? and Active=1",
                            conn);
                    SQLiteParameter pSNID = new SQLiteParameter();
                    pSNID.Value = PersonSNID;
                    ExistsCmd.Parameters.Add(pSNID);
                    SQLiteParameter pSNID2 = new SQLiteParameter();
                    pSNID2.Value = PhotoSNID;
                    ExistsCmd.Parameters.Add(pSNID2);
                    SQLiteParameter pSN = new SQLiteParameter();
                    pSN.Value = SocialNetwork.FACEBOOK;
                    ExistsCmd.Parameters.Add(pSN);
                    SQLiteDataReader reader = ExistsCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            PartitionDate = reader.GetDecimal(0);
                            PartitionID = reader.GetInt32(1);
                            Exists = true;
                        }
                    }
                    reader.Close();

                    string SQL;
                    if (Exists)
                    {
                        SQL = "update TagData set SocialNetwork=?, PersonSNID=?, PhotoSNID=?, Active=1, "
                            + "X=?, Y=?, Created=?, Updated=?, LastUpdate=? where PartitionDate=? and PartitionID=?";
                    }
                    else
                    {
                        SQLiteCommand ReadCmd = new SQLiteCommand(
                            "select FreeID from DailyID  where Day=? and ReferenceTable=?",
                                conn);
                        SQLiteParameter pDay = new SQLiteParameter();
                        pDay.Value = PartitionDate;
                        ReadCmd.Parameters.Add(pDay);
                        SQLiteParameter pRefTable = new SQLiteParameter();
                        pRefTable.Value = GetReferenceTable("TagData", out ErrorMessage);
                        ReadCmd.Parameters.Add(pRefTable);
                        SQLiteDataReader reader2 = ReadCmd.ExecuteReader();
                        PartitionID = 1; // first execution, I am #1, but free is 2
                        SQL = "insert into DailyID (Day, ReferenceTable, FreeID) values(?, ?, 2)";
                        if (reader2.Read())
                        {
                            if (!reader2.IsDBNull(0))
                            {
                                PartitionID = reader2.GetInt32(0);
                                SQL = "update DailyID set FreeID=FreeID+1 where Day=? and ReferenceTable=?";
                            }
                        }
                        reader2.Close();
                        SQLiteCommand FreeIDCmd = new SQLiteCommand(SQL, conn);
                        FreeIDCmd.Parameters.Add(pDay);
                        FreeIDCmd.Parameters.Add(pRefTable);
                        FreeIDCmd.ExecuteNonQuery();

                        SQL = "Insert into TagData (SocialNetwork, PersonSNID, PhotoSNID, Active, "
                            + "X, Y, Created, Updated, LastUpdate, PartitionDate, PartitionID)"
                            + " values (?,?,?,1,?,?,?,?,?,?,?)";
                    }
                    SQLiteParameter pPrSNID = new SQLiteParameter();
                    SQLiteParameter pPhSNID = new SQLiteParameter();
                    SQLiteParameter pX = new SQLiteParameter();
                    SQLiteParameter pY = new SQLiteParameter();
                    SQLiteParameter pCreated = new SQLiteParameter();
                    SQLiteParameter pUpdated = new SQLiteParameter();
                    SQLiteParameter pLU = new SQLiteParameter();
                    SQLiteParameter pPartitionDate = new SQLiteParameter();
                    SQLiteParameter pPartitionID = new SQLiteParameter();

                    pSN.Value = SocialNetwork.FACEBOOK;
                    pPrSNID.Value = PersonSNID;
                    pPhSNID.Value = PhotoSNID;
                    pX.Value = X;
                    pY.Value = Y;
                    pCreated.Value = Created;
                    pUpdated.Value = Updated;
                    pLU.Value = DateTime.Now;
                    pPartitionDate.Value = PartitionDate;
                    pPartitionID.Value = PartitionID;

                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);

                    UpdateCmd.Parameters.Add(pSN);
                    UpdateCmd.Parameters.Add(pPrSNID);
                    UpdateCmd.Parameters.Add(pPhSNID);
                    UpdateCmd.Parameters.Add(pX);
                    UpdateCmd.Parameters.Add(pY);
                    UpdateCmd.Parameters.Add(pCreated);
                    UpdateCmd.Parameters.Add(pUpdated);
                    UpdateCmd.Parameters.Add(pLU);
                    UpdateCmd.Parameters.Add(pPartitionDate);
                    UpdateCmd.Parameters.Add(pPartitionID);

                    int outrows = UpdateCmd.ExecuteNonQuery();

                    if (outrows > 0)
                    {
                        Saved = true;
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("didn't save photo tag " + PartitionDate + "," + PartitionID);
                    }
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving Tag\n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show(ErrorMessage);
                }
            } // lock
            return;
        }

        public static void ActionDataSave(
            string WhoSNID, string WhatSNID,
            int Verb,
            out bool Saved, out string ErrorMessage,
            string Adverb = null)
        {
            decimal PartitionDate;
            int PartitionID = -1;
            bool Exists = false;

            ErrorMessage = "";
            Saved = false;
            DateTime tempD = DateTime.Now;
            PartitionDate = tempD.Year * 10000 + tempD.Month * 100 + tempD.Day;

            lock (obj)
            {
                try
                {
                    GetConn();
                    SQLiteCommand ExistsCmd = new SQLiteCommand(
                    "select PartitionDate, PartitionID from ActionData where WhoSNID=? and WhatSNID=? and SocialNetwork=? and ActionID=? and Active=1",
                            conn);
                    SQLiteParameter pWhoSNID = new SQLiteParameter();
                    pWhoSNID.Value = WhoSNID;
                    ExistsCmd.Parameters.Add(pWhoSNID);
                    SQLiteParameter pWhatSNID = new SQLiteParameter();
                    pWhatSNID.Value = WhatSNID;
                    ExistsCmd.Parameters.Add(pWhatSNID);
                    SQLiteParameter pSN = new SQLiteParameter();
                    pSN.Value = SocialNetwork.FACEBOOK;
                    ExistsCmd.Parameters.Add(pSN);
                    SQLiteParameter pVerb = new SQLiteParameter();
                    pVerb.Value = Verb;
                    ExistsCmd.Parameters.Add(pVerb);
                    SQLiteDataReader reader = ExistsCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            PartitionDate = reader.GetDecimal(0);
                            PartitionID = reader.GetInt32(1);
                            Exists = true;
                        }
                    }
                    reader.Close();

                    string SQL;
                    if (!Exists)
                    {
                        SQLiteCommand ReadCmd = new SQLiteCommand(
                            "select FreeID from DailyID  where Day=? and ReferenceTable=?",
                                conn);
                        SQLiteParameter pDay = new SQLiteParameter();
                        pDay.Value = PartitionDate;
                        ReadCmd.Parameters.Add(pDay);
                        SQLiteParameter pRefTable = new SQLiteParameter();
                        pRefTable.Value = GetReferenceTable("ActionData", out ErrorMessage);
                        ReadCmd.Parameters.Add(pRefTable);
                        SQLiteDataReader reader2 = ReadCmd.ExecuteReader();
                        PartitionID = 1; // first execution, I am #1, but free is 2
                        SQL = "insert into DailyID (Day, ReferenceTable, FreeID) values(?, ?, 2)";
                        if (reader2.Read())
                        {
                            if (!reader2.IsDBNull(0))
                            {
                                PartitionID = reader2.GetInt32(0);
                                SQL = "update DailyID set FreeID=FreeID+1 where Day=? and ReferenceTable=?";
                            }
                        }
                        reader2.Close();
                        SQLiteCommand FreeIDCmd = new SQLiteCommand(SQL, conn);
                        FreeIDCmd.Parameters.Add(pDay);
                        FreeIDCmd.Parameters.Add(pRefTable);
                        FreeIDCmd.ExecuteNonQuery();

                        SQL = "Insert into ActionData (SocialNetwork, WhoSNID, WhatSNID, ActionID, Adverb, Active, "
                            + "LastUpdate, PartitionDate, PartitionID)"
                            + " values (?,?,?,?,?,1,?,?,?)";
                        SQLiteParameter pLU = new SQLiteParameter();
                        SQLiteParameter pPartitionDate = new SQLiteParameter();
                        SQLiteParameter pPartitionID = new SQLiteParameter();

                        pLU.Value = DateTime.Now;
                        pPartitionDate.Value = PartitionDate;
                        pPartitionID.Value = PartitionID;

                        SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);

                        UpdateCmd.Parameters.Add(pSN);
                        UpdateCmd.Parameters.Add(pWhoSNID);
                        UpdateCmd.Parameters.Add(pWhatSNID);
                        UpdateCmd.Parameters.Add(pVerb);
                        SQLiteParameter pAdverb = new SQLiteParameter();
                        pAdverb.Value = Adverb;
                        UpdateCmd.Parameters.Add(pAdverb);
                        UpdateCmd.Parameters.Add(pLU);
                        UpdateCmd.Parameters.Add(pPartitionDate);
                        UpdateCmd.Parameters.Add(pPartitionID);

                        int outrows = UpdateCmd.ExecuteNonQuery();

                        if (outrows > 0)
                        {
                            Saved = true;
                        }
                        else
                        {
                            //System.Windows.Forms.MessageBox.Show("didn't save action " + PartitionDate + "," + PartitionID);
                        }
                    }
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error saving Action\n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show(ErrorMessage);
                }
            } // lock
            return;
        }

        public static bool UpdatePhoto(long PhotoID, string Path,
            out string ErrorMessage)
        {
            ErrorMessage = "";
            bool Saved = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // update
                    string SQL = "Update PhotoData set Path=? where PhotoID=?";
                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pID = new SQLiteParameter();
                    SQLiteParameter pPic = new SQLiteParameter();
                    pID.Value = PhotoID;
                    pPic.Value = Path;
                    UpdateCmd.Parameters.Add(pPic);
                    UpdateCmd.Parameters.Add(pID);
                    UpdateCmd.ExecuteNonQuery();
                    Saved = true;
                    ErrorMessage = "";
                } // try
                catch (Exception ex)
                {
                    ErrorMessage = "Error updating photo path\n" + ex.ToString();
                }
            } // lock
            return Saved;
        }

        public static ArrayList GetAccounts(out string ErrorMessage)
        {
            ArrayList temp = new ArrayList();
            ErrorMessage = "";

            lock (obj)
            {
                try
                {
                    GetConn();
                    // try to read
                    SQLiteCommand CheckCmd = new SQLiteCommand("select AccountID, SocialNetwork, SNID, Name, Email, URL, BackupLevel from SNAccounts", conn);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        temp.Add(new SNAccount(reader.GetInt32(0), reader.GetInt32(1),
                        reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt16(6)));
                    }
                    reader.Close();
                    ErrorMessage = "";


                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error getting accounts from the database\n" + ex.ToString();
                    return null;
                }
            } // lock
            return temp;
        }

        /// <summary>
        /// Method that deletes an account by ID
        /// </summary>
        public static bool DeleteAccount(int ID, out string ErrorMessage)
        {
            ErrorMessage = "";
            bool result = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // try to read
                    string SQL = "delete from SNAccounts where AccountID=?";
                    SQLiteCommand DeleteCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pID = new SQLiteParameter();
                    pID.Value = ID;
                    DeleteCmd.Parameters.Add(pID);
                    int outrows = DeleteCmd.ExecuteNonQuery();
                    if (outrows > 0)
                    {
                        result = true;
                        //MessageBox.Show("row " + ID + " deleted: " + outrows + "\n" + SQL);
                    }
                    else
                    {
                        ErrorMessage = "0 rows deleted";
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.ToString();
                    return false;
                }
            } // lock
            return result;
        }

        /// <summary>
        /// Method that saves an account
        /// </summary>
        public static bool SaveAccount(int PersonID, string Name, string Email, int SocialNetwork,
            string SNID, string URL, int BackupLevel, DateTime BackupStartTime, out string ErrorMessage)
        {
            ErrorMessage = "";
            bool result = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // check duplicates
                    SQLiteCommand CheckCmd = new SQLiteCommand("select AccountID from SNAccounts where SocialNetwork=? and SNID=?", conn);
                    SQLiteParameter pSocialNetwork = new SQLiteParameter();
                    pSocialNetwork.Value = SocialNetwork;
                    CheckCmd.Parameters.Add(pSocialNetwork);
                    SQLiteParameter pSNID = new SQLiteParameter();
                    pSNID.Value = SNID;
                    CheckCmd.Parameters.Add(pSNID);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ErrorMessage = "The account already exists";
                        return false;
                    }
                    reader.Close();

                    string SQL = "insert into SNAccounts (Name, Email, SocialNetwork, SNID, URL, BackupLevel, BackupStartTime, PersonID) values (?,?,?,?,?,?,?,?)";
                    SQLiteCommand InsertCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pName = new SQLiteParameter();
                    pName.Value = Name;
                    InsertCmd.Parameters.Add(pName);
                    SQLiteParameter pEmail = new SQLiteParameter();
                    pEmail.Value = Email;
                    InsertCmd.Parameters.Add(pEmail);
                    InsertCmd.Parameters.Add(pSocialNetwork);
                    InsertCmd.Parameters.Add(pSNID);
                    SQLiteParameter pURL = new SQLiteParameter();
                    pURL.Value = URL;
                    InsertCmd.Parameters.Add(pURL);
                    SQLiteParameter pLevel = new SQLiteParameter();
                    pLevel.Value = BackupLevel;
                    InsertCmd.Parameters.Add(pLevel);
                    SQLiteParameter pBackupStartTime = new SQLiteParameter();
                    pBackupStartTime.Value = BackupStartTime;
                    InsertCmd.Parameters.Add(pBackupStartTime);
                    SQLiteParameter pID = new SQLiteParameter();
                    pID.Value = PersonID;
                    InsertCmd.Parameters.Add(pID);
                    int outrows = InsertCmd.ExecuteNonQuery();
                    if (outrows > 0)
                    {
                        result = true;
                    }
                    else
                    {
                        ErrorMessage = "0 rows inserted";
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.ToString();
                    return false;
                }
            } // lock
            return result;
        }

        /// <summary>
        /// Method that gets data for a request
        /// </summary>
        public static bool GetAsyncReq(int id, out string ReqType, out int Priority,
            out long? ParentID, out string ParentSNID,
            out DateTime Created, out DateTime Updated,
            out string ReqURL, out int State, out string Filename,
            out bool AddToken, out bool AddDateRange,
            out string ErrorMessage)
        {
            ErrorMessage = "";
            ReqType = "";
            ReqURL = "";
            Priority = 0;
            ParentID = null;
            ParentSNID = "";
            Created = DateTime.Now;
            Updated = DateTime.Now;
            Filename = "";
            State = 0;
            AddToken = false;
            AddDateRange = false;

            lock (obj)
            {
                try
                {
                    GetConn();
                    // try to read
                    SQLiteCommand CheckCmd = new SQLiteCommand("select RequestType, RequestString, Priority, ParentID, ParentSNID, Created, Updated, State, Filename, AddToken, AddDateRange from RequestsQueue where ID=?", conn);
                    SQLiteParameter pID = new SQLiteParameter();
                    pID.Value = id;
                    CheckCmd.Parameters.Add(pID);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ReqType = reader.GetString(0);
                        ReqURL = reader.GetString(1);
                        Priority = reader.GetInt32(2);
                        if (!reader.IsDBNull(3))
                        {
                            ParentID = reader.GetInt32(3);
                        }
                        else
                        {
                            ParentID = null;
                        }
                        ParentSNID = reader.GetString(4);
                        Created = reader.GetDateTime(5);
                        Updated = reader.GetDateTime(6);
                        State = reader.GetInt32(7);
                        Filename = reader.GetString(8);
                        AddToken = reader.GetBoolean(9);
                        AddDateRange = reader.GetBoolean(10);
                    }
                    reader.Close();
                    ErrorMessage = "";
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error getting request from the database\n" + ex.ToString();
                    return false;
                }
            } // lock
            return true;
        }

        /// <summary>
        /// Method that gets request N request IDs by state, ordered by priority
        /// </summary>
        public static ArrayList GetRequests(int N, int State, out string ErrorMessage, int MinPriority = 0)
        {
            ArrayList temp = new ArrayList();
            ErrorMessage = "";

            lock (obj)
            {
                try
                {
                    GetConn();
                    string SQL = "select ID from RequestsQueue where State=? and Priority>=? order by Priority desc, ID asc limit " + N;
                    SQLiteCommand CheckCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pState = new SQLiteParameter();
                    pState.Value = State;
                    CheckCmd.Parameters.Add(pState);
                    SQLiteParameter pPriority = new SQLiteParameter();
                    pPriority.Value = MinPriority;
                    CheckCmd.Parameters.Add(pPriority);
                    //MessageBox.Show("Query: " + SQL + ", State: " + State + ", N: " + N);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        temp.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                    ErrorMessage = "";
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error getting requests from the database\n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show("Error: " + ErrorMessage);
                    return null;
                }
            } // lock
            return temp;
        }

        /// <summary>
        /// Method that gets number of requests per state
        /// </summary>
        public static bool GetNRequestsPerState(int MinPriority, out int[] NState, out string ErrorMessage)
        {
            lock (obj)
            {
                NState = new int[AsyncReqQueue.TODELETE + 1];
                for (int i = 0; i < AsyncReqQueue.TODELETE + 1; i++) NState[i] = 0;

                ErrorMessage = "";
                try
                {
                    GetConn();
                    string SQL = "select State, count(*) from RequestsQueue where Priority>=? group by State";
                    SQLiteCommand CheckCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pPriority = new SQLiteParameter();
                    pPriority.Value = MinPriority;
                    CheckCmd.Parameters.Add(pPriority);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int State = reader.GetInt32(0);
                        int Count = reader.GetInt32(1);
                        NState[State] = Count;
                    }
                    reader.Close();
                    ErrorMessage = "";
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error getting requests from the database\n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show("Error: " + ErrorMessage);
                    return false;
                }
            } // lock
            return true;
        }

        /// <summary>
        /// Record the start of a backup
        /// </summary>
        public static bool StartBackup(out int BackupNo)
        {
            BackupNo = 0;

            lock (obj)
            {
                try
                {
                    GetConn();
                    string SQL = "insert into Backups (StartTime, Active) values (?,1)";
                    SQLiteCommand InsertCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pStart = new SQLiteParameter();
                    pStart.Value = DateTime.Now;
                    InsertCmd.Parameters.Add(pStart);
                    InsertCmd.ExecuteNonQuery();
                    SQL = "select ID from Backups where StartTime=?";
                    SQLiteCommand CheckCmd = new SQLiteCommand(SQL, conn);
                    CheckCmd.Parameters.Add(pStart);
                    SQLiteDataReader reader = CheckCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        BackupNo = reader.GetInt32(0);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    string ErrorMessage = "Error creating backup \n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show("Error: " + ErrorMessage);
                    return false;
                }
            } // lock
            return true;
        }

        /// <summary>
        /// Record the end of a backup
        /// </summary>
        public static bool EndBackup()
        {
            lock (obj)
            {
                try
                {
                    GetConn();
                    string SQL = "update Backups set EndTime = ?, Active = 0 where Active = 1";
                    SQLiteCommand UpdateCmd = new SQLiteCommand(SQL, conn);
                    SQLiteParameter pEnd = new SQLiteParameter();
                    pEnd.Value = DateTime.Now;
                    UpdateCmd.Parameters.Add(pEnd);
                    UpdateCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    string ErrorMessage = "Error updating backup \n" + ex.ToString();
                    //System.Windows.Forms.MessageBox.Show("Error: " + ErrorMessage);
                    return false;
                }
            } // lock
            return true;
        }
    }
}
