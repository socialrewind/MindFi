using System.Configuration;

namespace MyBackup
{
    public class DBConfig : ConfigurationSection
    {
	public DBConfig()
 	{
	}

	[ConfigurationProperty("LastDB", DefaultValue = "",
        IsRequired = false, IsKey = false)]
	public string lastDB 
	{
	    get
            { 
                return (string)this["LastDB"];
            }
            set
            { 
                this["LastDB"] = value;
            }
	}

	public static string GetLastDBFromFile()
	{
	    // get last opened from configuration file, if it exists
	    System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(
		ConfigurationUserLevel.None);
	    DBConfig custSection = new DBConfig();
	    custSection = config.GetSection("Database") as DBConfig;
	    if ( custSection != null )
	    {
		return custSection.lastDB;
	    }
	    else
	    {
		return "";
	    }
	}

    }
}