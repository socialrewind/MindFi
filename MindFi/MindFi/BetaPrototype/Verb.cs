using System;

namespace MyBackup
{
    /// <summary>
    /// Simple class that describe verbs/actions
    /// TODO: Make localizable (easy translation or swiching language)
    /// </summary>
    public class Verb:JSONParser
    {
        public const int FRIENDOF = 1;
        public const int SIGNIFICANTOTHERWITH = 2;
        public const int WORKAT = 3;
        public const int STUDYAT = 4;
	public const int SPEAK = 5;
	public const int LIVESAT = 6;
	public const int ORIGINALLYFROM = 7;
	public const int FANOF = 8;
	public const int TAG = 9;
	public const int LIKE = 10;
	public const int COMMENT = 11;
	public const int SENTTO = 12;
	
        // TODO: manage time, language
	// implement a function "verbalize", that applies the pattern as stored in the 
	// database. E.g.
	// Verb #1 Time Present Lang EN-US: %s and %s are now friends
	// Verb #1 Time Present Lang ES-MX: %s y %s son amigos
	// Verb #3 Time Past Lang EN-US: %s worked at %s

        public Verb(int id, string name):base("")
        {
            ID = id;
            Name = name;
        }
    }
}
