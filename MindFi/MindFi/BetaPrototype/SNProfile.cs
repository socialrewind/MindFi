using System;

namespace MyBackup
{
    /// <summary>
    /// Simple class that abstracts the download profile/frequency for a social network
    /// </summary>
    public class SNProfile
    {
        public int ID { get; set; }
        public string Name { get; set; }
	// TODO: Add default values
	// int Frequency;
	// string FrequencyUnits;
	// bool GetMyPhotos; // the ones I own
	// bool GetTaggedPhotos; // the ones I am tagged at
	// bool GetFriendsPhotos; // the ones friend owns
	// bool GetFriendsTaggedPhotos; // friend is tagged at
	// bool GetMyWall;
	// bool GetFriendsWall;
	// bool GetFriendsInfo;
        // bool GetOthersPhotos;
	// bool GetOthersWall;
	// bool GetOthersInfo;
        
        public SNProfile(int id, string name )
        {
            ID = id;
            Name = name;
        }
    }
}
