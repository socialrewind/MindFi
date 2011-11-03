using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MBBeta2
{
    /// <summary>
    /// Handles exceptions on MB WPF project
    /// </summary>

    public class MBError
    {
        //**************** Constructors
        #region Constructors
        public MBError()
        {
        }

        public MBError(object FromWindow, string FromAction, int ActionType)
        {
            Window = FromWindow;
            ActionWhenError = FromAction;
            //Decide what to do with error
            switch (ActionType)
            {
                case 1:
                    DoException();
                    break;
                case 2:
                    DoMessage();
                    break;
            }
        }

        //TODO: Handle errors of all types

        #endregion

        //**************** Attributes
        #region Attributes

        object Window;
        string ActionWhenError;


        #endregion

        //**************** Methods
        #region Methods

        void DoException()
        {
            throw new Exception("Error doing: " + ActionWhenError);
        }

        void DoMessage()
        {
            MessageBox.Show(ActionWhenError);
        }


        #endregion

    }
}
