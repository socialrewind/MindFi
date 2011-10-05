using System;
using System.Collections;
using System.Data.SQLite;
using System.Windows.Forms;
using System.IO;

namespace MyBackup
{
    /// <summary>
    /// Class that contains methods to query the SQLite database
    /// </summary>
    public class DBQuery
    {
	public static volatile Object QueryLock = new Object();
	private static volatile SQLiteConnection conn = null;
	
	public static void GetConn()
	{
	    lock ( QueryLock )
	    {
		if ( conn == null )
		{
	    	    conn = new SQLiteConnection(DBLayer.ConnString);
		    conn.Open();
		}
	    }
	
	}

        /// <summary>
        /// Gets total number of contacts in MyBackup
        /// </summary>
        public static int TotalContacts(out string ErrorMessage)
        {
	    return TotalRows("PersonData", out ErrorMessage);
	}

        /// <summary>
        /// Gets total number of contacts in MyBackup
        /// </summary>
        public static int TotalPosts(out string ErrorMessage)
        {
	    return TotalRows("PostData", out ErrorMessage);
	}

        /// <summary>
        /// Gets total number of rows in table
        /// </summary>
        private static int TotalRows(string table, out string ErrorMessage)
	{
	  int total = 0;

	  lock ( QueryLock )
	  {
            try
            {
	        GetConn();
		// TODO: don't include my own records
		SQLiteCommand CheckCmd = new SQLiteCommand("select count(*) from " + table, conn);
                SQLiteDataReader reader = CheckCmd.ExecuteReader();
                while (reader.Read())
                {
		    total = reader.GetInt32(0);
                }
                reader.Close();
		ErrorMessage = "";
            } catch (Exception ex)
            {
                ErrorMessage = "Error connecting to the database: " + ex.ToString() + "\n";
            }
            return total;
	  }
        }

        /// <summary>
        /// Gets number of requests in specific state
        /// </summary>
        public static int ReqsPerState(int State, out string ErrorMessage)
	{
	  int total = 0;

	  lock ( QueryLock )
	  {
            try
            {
	        GetConn();
		SQLiteCommand CheckCmd = new SQLiteCommand("select count(*) from RequestsQueue where State=?", conn);
                SQLiteParameter pState = new SQLiteParameter();
		pState.Value = State;
		CheckCmd.Parameters.Add(pState);
                SQLiteDataReader reader = CheckCmd.ExecuteReader();
		while (reader.Read())
                {
		    total = reader.GetInt32(0);
                }
                reader.Close();
		ErrorMessage = "";
            } catch (Exception ex)
            {
                ErrorMessage = "Error connecting to the database: " + ex.ToString() + "\n";
            }
            return total;
	  }
        }
    }

}