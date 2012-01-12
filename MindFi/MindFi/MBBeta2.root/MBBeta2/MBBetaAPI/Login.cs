using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public class Login
    {
        //**************** Constructors
        #region Constructors
        public Login()
        {
        }

        public Login(string connParam, string userParam, string passwordParam)
        {
            user = userParam;
            password = passwordParam;
            conn = connParam;
            db = new DBConnector();
        }

        #endregion

        //**************** Attributes
        #region Attributes
        string conn;
        string user;
        string password;
        DBConnector db;

        #endregion

        //**************** Methods
        #region Methods


        //Returns profile completeness
        //0: Error reading DBFile (Wrong password)
        //1: DBFile present, but user does not match
        //2: DBFile present, user is valid, but no associated SN Accounts
        //3: User has associated at leat one SN accounts
        public int ValidateLogin()
        {
            int level = 0;
            string UserinDB = "";

            ////Connect to DB
            lock (DBLayer.obj)
            {
                DBLayer.DatabaseInUse = true;
                try
                {
                    DBLayer.GetConn();
                    SQLiteCommand command = new SQLiteCommand("select Username FROM Config", DBLayer.conn);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        UserinDB = reader.GetString(0);
                        level = 1;
                    }
                    reader.Close();
                    if (UserinDB == user)
                    {
                        level = 2;

                        //Test if there is any associated SN Account
                        command = new SQLiteCommand("select AccountID FROM SNAccounts", DBLayer.conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            level = 3;
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    level = 0;
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }

            }

            return level;
        }

        #endregion
    }
}
