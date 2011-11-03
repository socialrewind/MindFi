using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MBBetaAPI
{
    public partial class SNSocialGroup
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
            using (SQLiteConnection conn = new SQLiteConnection(db.ConnString))
            {
                try
                {
                    conn.Open();
                    GetFromConnectedDB(conn);
                    conn.Close();
                }
                catch (Exception ex)
                {
                    string message = "Reading Social Groups from DB" + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
            }

        }


        public void GetFromConnectedDB(SQLiteConnection conn)
        {


            bool GotData = false;

            try
            {

                //Read Entity
                SQLiteCommand command = new SQLiteCommand("select Name from Entities where ID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    Name = reader.GetString(0);
                }

                reader.Close();

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }




            if (!GotData)
                throw new Exception("No data available for the social group");
        }


        void GetMembers(DBConnector db)
        {
            using (SQLiteConnection conn = new SQLiteConnection(db.ConnString))
            {

                MemberIDs = new List<int>();
                MemberNames = new List<string>();

                try
                {
                    conn.Open();

                    SQLiteCommand command = new SQLiteCommand("select A.ID, A.Name from Entities A, RelationsData B where A.ID = B.SubjectID AND B.ObjectID=@ID", conn);
                    command.Parameters.Add(new SQLiteParameter("ID", ID));

                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        MemberIDs.Add(reader.GetInt32(0));
                        MemberNames.Add(reader.GetString(1));
                    }

                    reader.Close();

                    conn.Close();

                }
                catch
                    (Exception ex)
                {
                    APIError error = new APIError(this, "ERROR: Reading Members in Social Group" + ex.Message + " ID:  " + ID.ToString(), 1);
                }
            }

        }



        #endregion
    }
}
