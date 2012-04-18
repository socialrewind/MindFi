using System;

namespace MBBetaAPI.AgentAPI
{
    /// <summary>
    /// Simple class that allows to abstract the Social Network accounts and keep Profile data
    /// </summary>
    public class SNAccount
    {
        /// <summary>
        /// Backup levels
        /// </summary>
        //public const int BASIC = 1;
        //public const int EXTENDED = 2;
        //public const int STALKER = 3;

        /// <summary>
        /// Identifier for the social network account in the database
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Current Social Network for the Account
        /// </summary>
        public int SocialNetwork { get; set; }
        /// <summary>
        /// Social Network ID associated to the Account
        /// </summary>
        public string SNID { get; set; }
        /// <summary>
        /// name to show associated to the Account in the Social Network
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// primary email associated to the Account in the Social Network
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// link to the Account in the Social Network        
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        /// Backup Level = default for how much information to backup
        /// </summary>
        //public int currentBackupLevel { get; set; }

        public bool BackupMyWall;
        public bool BackupMyNews;
        public bool BackupMyInbox;
        public bool BackupMyEvents;
        public bool BackupMyNotifications;
        public bool BackupFriendsInfo;
        public bool BackupFriendsFamily;
        public bool BackupFriendsPic;
        public bool BackupMyAlbums;
        public bool BackupMyPhotos;
        public bool BackupFriendsWall;
        public bool BackupFriendsEvents;
        public bool BackupFriendsAlbums;

        /*
        /// <summary>
        /// Date the execution of the account backup starts
        /// </summary>
        public DateTime BackupStartDate { get; set; }
        /// <summary>
        /// Date the execution of the account backup finishes
        /// </summary>
        public DateTime BackupEndDate { get; set; }
        /// <summary>
        /// Initial date to be backed up for the account (first time = as selected; next times = from the time backup was started)
        /// </summary>
        public DateTime BackupPeriodStart { get; set; }
        /// <summary>
        /// End date to be backed up for the account (First time = date the backup is launched; next times = new date incremental backup is launched
        /// </summary>
        public DateTime BackupPeriodEnd { get; set; }
        /// <summary>
        /// Current period that is being backed up (allows for week by week or other period-wise increments)
        /// </summary>
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        /// <summary>
        /// How often is incremental backup to be launched? (Number)
        /// </summary>
        public int BackupFrequency { get; set; }
        /// <summary>
        /// How often is incremental backup to be launched? (Units)
        /// </summary>
        public string BackupFrequencyUnit { get; set; }
        */


        /// <summary>
        /// Lock variable
        /// </summary>
        private static volatile Object obj = new Object();
        /// <summary>
        /// Current account = default being used
        /// </summary>
        private static SNAccount m_current = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sn"></param>
        /// <param name="snid"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="url"></param>
        /// <param name="level"></param>
        public SNAccount(int id, int sn, string snid, string name, string email, string url, 
                    bool bMyWall,
                    bool bMyNews,
                    bool bMyInbox,
                    bool bMyEvents,
                    bool bMyPhotos,
                    bool bFriendsEvents,
                    bool bFriendsAlbums,
                    bool bFriendsWall,
            // TODO: Remove unused parameters
                    DateTime backupPeriodStart, DateTime backupPeriodEnd )
        {
            ID = id;
            SocialNetwork = sn;
            SNID = snid;
            Name = name;
            Email = email;
            URL = url;
            // TODO: Remove comments
            /*
            //currentBackupLevel = level;
            BackupPeriodStart = backupPeriodStart;
            BackupPeriodEnd = backupPeriodEnd;
            // make sure they are initialized for preventing bugs
            BackupStartDate = DateTime.Now;
            CurrentPeriodStart = backupPeriodStart;
            CurrentPeriodEnd = backupPeriodEnd;
             */
            SRAPI.SRBackup.BackupPeriodSelectedStartDate = backupPeriodStart;
            SRAPI.SRBackup.BackupPeriodSelectedEndDate = backupPeriodEnd;
            BackupMyWall = bMyWall;
            BackupMyNews = bMyNews;
            BackupMyInbox = bMyInbox;
            BackupMyEvents = bMyEvents;
            BackupMyNotifications = bMyNews; // TODO: Specific Control
            BackupFriendsInfo = true; // TODO: Specific Control
            BackupFriendsFamily = true; // TODO: Specific Control
            BackupFriendsPic = true;
            BackupMyAlbums = bMyPhotos;
            BackupMyPhotos = false; // TODO: distinct control?

            BackupFriendsWall = bFriendsWall;
            BackupFriendsEvents = bFriendsEvents;
            BackupFriendsAlbums = bFriendsAlbums;
            
            AsyncReqQueue.BackupMyWall = bMyWall;
            AsyncReqQueue.BackupMyNews = bMyNews;
            AsyncReqQueue.BackupMyInbox = bMyInbox;
            AsyncReqQueue.BackupMyEvents = bMyEvents;
            AsyncReqQueue.BackupMyPhotos = bMyPhotos;
            AsyncReqQueue.BackupFriendsEvents = bFriendsEvents;
            AsyncReqQueue.BackupFriendsAlbums = bFriendsAlbums;
            AsyncReqQueue.BackupFriendsWall = bFriendsWall;

        }

        /// <summary>
        /// Returns the singleton profile
        /// </summary>
        public static SNAccount CurrentProfile
        {
            get { return m_current; }
        }

        /// <summary>
        /// Updates the singleton
        /// </summary>
        /// <param name="current">New social network account to set as default</param>
        public static void UpdateCurrent(SNAccount current)
        {
            lock (obj)
            {
                m_current = current;
            }
        }
    }
}
