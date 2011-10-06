using System;
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


        //Returns 0 for success
        // >0 means error code
        // 1: Wrong user password or damaged file
        // 2: DB error
        public int ValidateLogin()
        {
            int success = 1;

            ////Connect to DB
            using (SQLiteConnection conn = new SQLiteConnection(db.ConnString))
            {
                try
                {
                    conn.Open();
                    SQLiteCommand command = new SQLiteCommand("select name FROM sqlite_master WHERE name='Entities'", conn);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        success = 0;
                    }
                    conn.Close();
                }
                catch
                {
                    success = 1;
                }
            }

            //Check user

            return success;
        }

        #endregion
    }
}
