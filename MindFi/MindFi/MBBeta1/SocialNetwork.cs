using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MBBetaAPI
{
    public class SocialNetwork
    {

        #region Constructors
        //************* Constructors

        public SocialNetwork(int IDParam)
        {
            ID = IDParam;
        }

        public SocialNetwork(DBConnector db, int IDParam)
        {
            ID = IDParam;
            GetFromDB(db);
        }


        #endregion

        #region Attributes
        //************* Attributes
        int ID;
        public string Name;
        Uri MainPage;
        string ProfilePicsPath;

        #endregion


        #region DB MEthods
        //************ Methods

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
                catch
                {
                    APIError error = new APIError(this, "Reading SN from DB", 1);
                }
            }

        }


        public void GetFromConnectedDB(SQLiteConnection conn)
        {

            SQLiteCommand command = new SQLiteCommand("select ID, SNName from SocialNetworks where ID = @ID", conn);
            command.Parameters.Add(new SQLiteParameter("ID", ID));

            bool GotData = false;

            try
            {

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    Name = reader.GetString(1);
                }
                reader.Close();
            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message, 1);
            }
            
            if (!GotData)
                throw new Exception("No data available for the social network");
        }




        #endregion

    }
}
