using System;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Simple class that allows to abstract the Social Network accounts
    /// </summary>
    public class SNAccount
    {
        public const int BASIC = 1;
        public const int EXTENDED = 2;
        public const int STALKER = 3;

        public bool Remove { get; set; }
        public int ID { get; set; }
        public int SocialNetwork { get; set; }
        public string SNID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string URL { get; set; }
        public int currentBackupLevel { get; set; }

        public SNAccount(int id, int sn, string snid, string name, string email, string url, int level)
        {
            ID = id;
            SocialNetwork = sn;
            SNID = snid;
            Name = name;
            Email = email;
            URL = url;
            currentBackupLevel = level;
            Remove = false;
        }
    }
}
