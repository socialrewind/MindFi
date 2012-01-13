using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using MBBetaAPI.AgentAPI;

namespace MBBetaAPI
{
    public class Entity
    {
        //**************** Constructors
        #region Constructors

        public Entity()
        {
        }

        public Entity(int IDParam)
        {
            ID = IDParam;
            GetFromDB();
        }

        #endregion

        //**************** Attributes
        #region Attributes
        public int ID { get; set; }
        public string Name { get; set; }
        public string Type { get; private set; }
        bool Active { get; set; }

        #endregion

        //**************** Methods
        #region Methods

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
                catch
                {
                    APIError error = new APIError(this, "Reading Entity from DB", 1);
                }
                finally
                {
                    DBLayer.DatabaseInUse = false;
                }
            }

        }


        public void GetFromConnectedDB(SQLiteConnection conn)
        {

            SQLiteCommand command = new SQLiteCommand("select Name, Type, Active from Entities where ID = @ID", conn);
            command.Parameters.Add(new SQLiteParameter("ID", ID));

            bool GotData = false;

            try
            {

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GotData = true;
                    Name = reader.GetString(0);
                    Type = reader.GetString(1);
                    Active = reader.GetBoolean(2);

                }
                reader.Close();
            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message, 1);
            }

            if (!GotData)
                throw new Exception("No data available for the entity");
        }

        #endregion
    }
}
