using System;

namespace MyBackup
{
    /// <summary>
    /// Simple class that abstracts the possible frequency configuration
    /// </summary>
    public class Frequency
    {
        public int ID { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        
        public Frequency(int id, string name, int count)
        {
            ID = id;
            Name = name;
	    Count = count;
        }
    }
}
