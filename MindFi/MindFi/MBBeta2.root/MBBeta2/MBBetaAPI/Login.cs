﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

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
            db = new DBConnector(conn);
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
            using (SQLiteConnection conn = new SQLiteConnection(db.ConnString))
            {
                try
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand("select Username FROM Config", conn);
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
                        command = new SQLiteCommand("select AccountID FROM SNAccounts", conn);
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            level = 3;
                        }
                        reader.Close();
                    }
                    conn.Close();
                }
                catch
                {
                    level = 0;
                }

            }

            return level;
        }

        #endregion
    }
}