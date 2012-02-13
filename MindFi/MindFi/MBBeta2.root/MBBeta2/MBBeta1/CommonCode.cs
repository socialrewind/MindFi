using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MBBeta2
{
    public class CommonCode
    {

        //**************** Constructors
        #region Constructors

        public CommonCode()
        {
        }

        #endregion

        //**************** Attributes
        #region Attributes
        #endregion

        //**************** Methods
        #region Methods

        void BlurWindow(Window w)
        {
            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect();
            blur.Radius = 2;
            w.Effect = blur;
        }

        void UnblurWindow(Window w)
        {
            w.Effect = null;
        }

        public void PositionNewWindow(Window Parent, Window Child)
        {
            BlurWindow(Parent);

            Child.Width = Parent.ActualWidth * 0.9;
            Child.Height = Parent.ActualHeight * 0.9;
            Child.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            Child.Owner = Parent;
            Child.ShowDialog();

            UnblurWindow(Parent);
        }

        #endregion

    }
}
