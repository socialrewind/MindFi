using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBBetaAPI
{

    /// <summary>
    /// Handles exceptions on API code
    /// </summary>
    public class APIError
    {

        #region Constructors
        public APIError()
        {
        }

        public APIError(object FromClass, string FromAction, int ActionType)
        {
            o = FromClass;
            ActionWhenError = FromAction;
            //Decide what to do with error
            switch (ActionType)
            {
                case 1:
                    DoException();
                    break;
                case 2:
                    DoLog();
                    break;
            }
        }

        //TODO: Handle errors of all types

        #endregion

        //**************** Attributes
        #region Attributes

        object o;
        string ActionWhenError;


        #endregion

        //**************** Methods
        #region Methods

        void DoException()
        {
            throw new Exception("Error doing: " + ActionWhenError);
        }

        void DoLog()
        {
            //MessageBox.Show(ActionWhenError);
        }


        #endregion

    }
}
