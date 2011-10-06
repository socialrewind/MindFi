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
        public int SocialNetworkAccountID { get; set; }
        public string userName { get; set; }
//        public string databaseLocation { get; set; }
//        public string connString { get; set; }
        public int? socialNetworkID { get; set; }
        public string socialNetworkURL { get; set; }
        public int currentBackupLevel { get; set; }

        // information in social networks
        // TODO: abstract profile for different SNs
        public FBPerson fbProfile { get; set; }

        /*
        // My information
        public bool backupMyInformation { get; set; }
        public bool backupMyAlbums { get; set; }
        public bool backupMyPhotos { get; set; }
        public bool backupMyPosts { get; set; }
        public bool backupMyEvents { get; set; }
        public bool backupMyInbox { get; set; }
        public DateTime startTime { get; set; }
        public DateTime? lastBackup { get; set; }

        // Contact information
        public bool backupFriendsInfo { get; set; }
        public bool backupFriendsPhotos { get; set; }
        public bool backupFriendsPosts { get; set; }
        public bool backupFriendsEvents { get; set; }
        public ArrayList whichFriends { get; set; }

        // Indirect contact information
        public bool backupOthersInfo { get; set; }
        public bool backupOthersPhotos { get; set; }
        public bool backupOthersPosts { get; set; }
        public ArrayList whichOthers { get; set; }

        // Interface flags
        public bool autoNextButton { get; set; }
         * */

        #endregion

        #region "Methods"
        public MyBackupProfile()
        {
            currentBackupLevel = BASIC;
        }
        #endregion
    }
}