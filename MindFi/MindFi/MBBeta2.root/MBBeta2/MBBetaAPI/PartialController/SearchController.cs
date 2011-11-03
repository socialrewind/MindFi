﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MBBetaAPI
{
    public partial class Search
    {
        //**************** Constructors
        #region Constructors

        /// <summary>
        /// Performs a basic search, using only a text as parameter
        /// </summary>
        /// <param name="db"></param>
        /// <param name="SearchTextParam"></param>
        public Search(DBConnector db, string SearchTextParam)
        {
            SearchText = SearchTextParam;
            CreateEmptySearch();
            SearchMatchingPostIDs(db, DateTime.MinValue, DateTime.MinValue);
            SearchMatchingMessageIDs(db, DateTime.MinValue, DateTime.MinValue);
            SearchMatchingAlbumIDs(db, DateTime.MinValue, DateTime.MinValue);
            SearchMatchingPhotoIDs(db, DateTime.MinValue, DateTime.MinValue);
            SearchMatchingEventIDs(db, DateTime.MinValue, DateTime.MinValue);

            //TODO: Include LinkedIn and Twitter Searches
        }

        /// <summary>
        /// Performs advanced search, using SearchOptions and Datas as filters, additional to text search
        /// </summary>
        /// <param name="db"></param>
        /// <param name="SearchTextParam"></param>
        /// <param name="SearchOption"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        public Search(DBConnector db, string SearchTextParam, BitArray SearchOption, DateTime StartDate, DateTime EndDate)
        {

            SearchText = SearchTextParam;
            CreateEmptySearch();
            //FBPosts
            if (SearchOption[0] == true)
                SearchMatchingPostIDs(db, StartDate, EndDate);
            //FBAlbums
            if (SearchOption[1] == true)
                SearchMatchingAlbumIDs(db, StartDate, EndDate);
            //FBPhotos
            if (SearchOption[2] == true)
                SearchMatchingPhotoIDs(db, StartDate, EndDate);
            //FBMessages
            if (SearchOption[3] == true)
                SearchMatchingMessageIDs(db, StartDate, EndDate);
            //FBEvents
            if (SearchOption[4] == true)
                SearchMatchingEventIDs(db, StartDate, EndDate);


            //TODO:Include LinkedIn and Twitter Searches

        }


        #endregion

        //**************** Attributes
        #region Attributes

        //Input
        public string SearchText { get; set; }

        //Output
        public List<int> MatchingPostIDs { get; private set; }
        public List<int> MatchingAlbumIDs { get; private set; }
        public List<int> MatchingPhotoIDs { get; private set; }
        public List<int> MatchingMessageIDs { get; private set; }
        public List<int> MatchingEventIDs { get; private set; }

        #endregion

        //**************** Methods
        #region Methods

        void CreateEmptySearch()
        {
            MatchingEventIDs = new List<int>();
            MatchingPostIDs = new List<int>();
            MatchingMessageIDs = new List<int>();
            MatchingAlbumIDs = new List<int>();
            MatchingPhotoIDs = new List<int>();
        }

        void SearchMatchingPostIDs(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            APIError error;

            if (!SearchMatchingPostIDsFromDB(db, StartDate, EndDate))
                error = new APIError(this, "Search: Error getting results for Posts", 1);
        }


        void SearchMatchingMessageIDs(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            APIError error;

            if (!SearchMatchingMessageIDsFromDB(db, StartDate, EndDate))
            {
                error = new APIError(this, "Search: Error getting results for Messages", 1);
            }
        }


        void SearchMatchingAlbumIDs(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            APIError error;

            if (!SearchMatchingAlbumIDsFromDB(db, StartDate, EndDate))
                error = new APIError(this, "Search: Error getting results for Photo Albums", 1);
        }

        void SearchMatchingPhotoIDs(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            APIError error;

            if (!SearchMatchingPhotoIDsFromDB(db, StartDate, EndDate))
                error = new APIError(this, "Search: Error getting results for Photos", 1);
        }




        void SearchMatchingEventIDs(DBConnector db, DateTime StartDate, DateTime EndDate)
        {
            APIError error;

            if (!SearchMatchingEventIDsFromDB(db, StartDate, EndDate))
                error = new APIError(this, "Search: Error getting results for Events", 1);
        }


        //TODO: Complete Methods for LinkedIn and Twitter

        #endregion
    }
}
