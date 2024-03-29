﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MBBetaAPI
{

    /// <summary>
    /// Get detailed info about a person
    /// </summary>

    public partial class Person : Entity
    {
        //**************** Constructors
        #region Constructors

        public Person(int IDParam)
        {
            ID = IDParam;
            GetFromDB();
            DisplayName = FirstName + " " + LastName;
            GetSchools();
            GetCompanies();
            //GetSignificantOther();
        }


        #endregion

        //**************** Attributes
        #region Attributes
        public Int64 SNID { get; private set; }
        public int SN { get; private set; }
        public Uri SNLink { get; private set; }
        public string FirstName { get; private set; }
        public string MiddleName { get; private set; }
        public string LastName { get; private set; }
        public string DisplayName { get; private set; }
        public int BirthDay { get; private set; }
        public int BirthMonth { get; private set; }
        public int BirthYear { get; private set; }
        public string About { get; private set; }
        public string Bio {get; private set;}
        public string Quotes { get; private set; }
        public string ProfilePic { get; private set; }
        public int Distance { get; private set; }
        public int DataRequestID { get; private set; }
        public int DataRequestState { get; private set; }
        public string DataRequestType { get; private set; }
        public string DataResponseValue { get; private set; }
        public string ErrorMessage { get; private set; }

        //Extended attributes
        public List<RelatedOrganization> SchoolsList { get; private set; }
        public List<RelatedOrganization> CompaniesList { get; private set; }

        //Relationship Status
        public string RelationshipStatus {get; private set;}
        public string SignificantOtherName { get; private set; }
        public PersonLight SignificantOther { get; private set; }

        //Backup Options
        public bool BackupWall { get; set; }
        public bool BackupEvents { get; set; }
        public bool BackupPhotos { get; set; }
        public enum BackupOptions { Wall, Events, Photos };
        #endregion

        //**************** Methods
        #region Methods

        void GetSchools()
        {
           
            SchoolsList = GetRelatedOrganizationsFromDB(4);

        }

        void GetCompanies()
        {
            CompaniesList = GetRelatedOrganizationsFromDB(3);

        }

        void GetSignificantOther()
        {
            SignificantOther = GetSignificantOtherFromDB();
        }

        public void SetBackupOptions(int Option, bool val)
        {
            switch (Option)
            {
                case (int)Person.BackupOptions.Wall:
                    BackupWall = val;
                    break;
                case (int)Person.BackupOptions.Events:
                    BackupEvents = val;
                    break;
                case (int)Person.BackupOptions.Photos:
                    BackupPhotos = val;
                    break;
            }

        }

        // TODO: Return appropriately the options
        #endregion
    }
}
