using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MBBetaAPI
{
    public partial class Person
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
                    string message = "Reading Person from DB" + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
            }

        }


        void GetFromConnectedDB(SQLiteConnection conn)
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

                //Read Person
                command = new SQLiteCommand("select SocialNetwork, SNID, ProfilePic, Link, FirstName, MiddleName, LastName, BirthDay, BirthMonth, BirthYear, About, Bio, Quotes, RelationshipStatus, Distance from  PersonData where PersonID = @ID", conn);
                command.Parameters.Add(new SQLiteParameter("ID", ID));

                SQLiteDataReader reader2 = command.ExecuteReader();
                while (reader2.Read())
                {
                    
                    GotData = true;

                    //Read Distance (parameter 14)
                    Distance = reader2.GetInt32(14);
                    if (Distance < 2)
                    {

                        //SocialNetwork, SNID, ProfilePic, Link, FirstName, MiddleName, LastName
                        SN = reader2.GetInt32(0);
                        SNID = reader2.GetInt64(1);
                        if (reader2.IsDBNull(2))
                            ProfilePic = "Images/nophoto.jpg";
                        else
                            ProfilePic = reader2.GetString(2);
                        SNLink = new Uri(reader2.GetString(3));
                        FirstName = reader2.GetString(4);
                        if (reader2.IsDBNull(5))
                            MiddleName = "";
                        else
                            MiddleName = reader2.GetString(5);
                        LastName = reader2.GetString(6);

                        //BirthDay, BirthMonth, BirthYear, About, Bio, Quotes

                        if (reader2.IsDBNull(7))
                            BirthDay = 1;
                        else
                            BirthDay = reader2.GetInt32(7);
                        if (reader2.IsDBNull(8))
                            BirthMonth = 1;
                        else
                            BirthMonth = reader2.GetInt32(8);
                        if (reader2.IsDBNull(9))
                            BirthYear = 1900;
                        else
                            BirthYear = reader2.GetInt32(9);
                        if (reader2.IsDBNull(10))
                            About = "";
                        else
                            About = reader2.GetString(10);
                        if (reader2.IsDBNull(11))
                            Bio = "";
                        else
                            Bio = reader2.GetString(11);
                        if (reader2.IsDBNull(12))
                            Quotes = "";
                        else
                            Quotes = reader2.GetString(12);

                        //RelationshipStatus, Distance

                        if (reader2.IsDBNull(13))
                            RelationshipStatus = "";
                        else
                            RelationshipStatus = reader2.GetString(13);

                        //Distance is 14
                    }
                    
                }

                reader2.Close();

            }
            catch
                (Exception ex)
            {
                APIError error = new APIError(this, ex.Message + " ID:  " + ID.ToString(), 1);
            }




            if (!GotData)
                throw new Exception("No data available for the person");
        }


        //Returns a list of organizations related to a person
        //Verb: WORKAT(3) / STUDYAT (4)
        List<RelatedOrganization> GetRelatedOrganizationsFromDB(DBConnector db, int Verb)
        {
            var Orgs = new List<RelatedOrganization>();

            using (SQLiteConnection conn = new SQLiteConnection(db.ConnString))
            {
                try
                {
                    conn.Open();
                    //Read Entities
                    SQLiteCommand command = new SQLiteCommand("select ObjectID, Adverb, IndirectObject from RelationsData where SubjectID = @ID AND VerbID = @Verb", conn);
                    command.Parameters.Add(new SQLiteParameter("ID", ID));
                    command.Parameters.Add(new SQLiteParameter("Verb", Verb));

                    SQLiteDataReader reader = command.ExecuteReader();
                    int i = 0;
                    while (reader.Read())
                    {                        
                        Orgs.Add(new RelatedOrganization(reader.GetInt32(0)));
                        if (!reader.IsDBNull(1))
                            Orgs[i].Adverb = reader.GetString(1);
                        if(!reader.IsDBNull(2))
                            Orgs[i].IndirectObject = reader.GetString(2);
                        i++;
                    }

                    reader.Close();

                    conn.Close();
                }
                catch (Exception ex)
                {
                    string message = "Reading List of Organization for Person from DB: " + Type + " : " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
            }


            //Fill base data needed for organization, not available in Relations Table
            foreach (RelatedOrganization RO in Orgs)
            {
                Organization O = new Organization(db, RO.ID);
                RO.Name = O.Name;
                RO.SN = O.SN;
                RO.SNID = O.SNID;                
            }

            return Orgs;
        }


        PersonLight GetSignificantOtherFromDB(DBConnector db)
        {

            int RelatedID = 0;
            PersonLight SO = null;

            using (SQLiteConnection conn = new SQLiteConnection(db.ConnString))
            {
                try
                {
                    conn.Open();
                    //Read Action Data where ActionID = 2 (In relation to...)
                    SQLiteCommand command = new SQLiteCommand("select ObjectID from RelationsData where VerbID=2 AND SubjectID=@ID", conn);
                    command.Parameters.Add(new SQLiteParameter("ID", ID));

                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var value = reader.GetValue(0);
                        RelatedID = Convert.ToInt32(value);
                    }

                    reader.Close();

                    conn.Close();
                }
                catch (Exception ex)
                {
                    string message = "Reading Significant Other for Person from DB: " + Type + " : " + ex.Message.ToString();
                    APIError error = new APIError(this, message, 1);
                }
            }

            if (RelatedID != 0)
            {
                SO = new PersonLight(db, RelatedID);
            }

            return SO;
        }

        #endregion
    }
}
