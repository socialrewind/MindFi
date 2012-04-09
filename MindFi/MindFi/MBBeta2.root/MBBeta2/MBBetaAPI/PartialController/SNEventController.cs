using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    public partial class SNEvent : Entity
    {
        //**************** Constructors
        #region Constructors
        public SNEvent()
        {
        }

        public SNEvent(int IDParam)
        {
            ID = IDParam;
            // TODO: Check why an entity is associated to the event
            Entity A = new Entity(ID);
            Name = A.Name;
            GetFromDB();
            GetAttendeesFromDB();
        }

        #endregion

        //**************** Attributes
        #region Attributes
        public int SN { get; set; }
        public Int64 SNID { get; set; }
        
        //Attributes
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime Date { get; set; }

        //People Attending
        public List<PersonLight> Attending { get; set; }
        public List<string> AttendingNames { get; set; }
        //People MayBeAttending
        public List<PersonLight> MayBeAttending { get; set; }
        public List<string> MayBeAttendingNames { get; set; }
        //People NotAttending
        public List<PersonLight> NotAttending { get; set; }
        public List<string> NotAttendingNames { get; set; }
        //People NotRSVP
        public List<PersonLight> UnknownRSVP { get; set; }
        public List<string> UnknownRSVPNames { get; set; }
        //Organizers of the event
        public string OrganizerText { get; set; }
        public List<string> OrganizerNames { get; set; }

        #endregion

        //**************** Methods
        #region Methods
        #endregion
    }
}
