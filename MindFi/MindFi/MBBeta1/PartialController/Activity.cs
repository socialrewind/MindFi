using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{
    public partial class Activity
    {        

        //**************** Constructors
        #region Constructors

        public Activity()
        {
        }

        public Activity(DBConnector db, int IDParam)
        {
            IDParam = ID;
        }
    
        #endregion

        //**************** Attributes
        #region Attributes

        int ID;
        int ActorID;
        PersonLight Actor;
        string Verb;        //TODO: Process via localization engine. Use string as parameter
        int ObjectID;



        #endregion

        //**************** Methods
        #region Methods
        #endregion


    }
}
