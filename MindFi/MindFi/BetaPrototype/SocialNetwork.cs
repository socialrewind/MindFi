using System;

namespace MyBackup
{
    /// <summary>
    /// Simple class that allows to list the existing Social Networks and communicate with them
    /// </summary>
    public class SocialNetwork
    {
        public const int FACEBOOK = 1;
        public const int TWITTER = 2;
        public const int LINKEDIN = 3;
        public const int GOOGLE_PLUS = 4;

        public int ID { get; set; }
        public string Name { get; set; }
        // TODO: Implement in database
        public string URL { get; set; }
        public string IconPath { get; set; }

        public SocialNetwork(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}
