using System;
using System.Collections;

namespace MyBackup
{
    public class MyBackupProfile
    {
        public const int BASIC = 1;
        public const int EXTENDED = 2;
        public const int STALKER = 3;

        #region "Properties"
        // TODO: Array of profiles for supporting multiple accounts at the same time, verifying logged in time, etc.
        public string SocialNetworkAccountID { get; set; }
        public string userName { get; set; }
        public int? socialNetworkID { get; set; }
        public string socialNetworkURL { get; set; }
        public int currentBackupLevel { get; set; }
        public DateTime backupStartDate { get; set; }

        // information in social networks
        // TODO: abstract profile for different SNs
        public FBPerson fbProfile { get; set; }
        #endregion

        #region "Methods"
        public MyBackupProfile()
        {
            currentBackupLevel = BASIC;
        }
        #endregion
    }
}