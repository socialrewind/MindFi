using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MBBetaAPI;

namespace MBBeta2.Controls
{
    /// <summary>
    /// Interaction logic for FBLikeControl.xaml
    /// </summary>
    public partial class FBLikeControl : UserControl
    {
        public FBLikeControl()
        {
            InitializeComponent();
        }

        public static DependencyProperty NumberOfLikesDP = DependencyProperty.Register("NumberOfLikes", typeof(int), typeof(FBLikeControl));

        public int NumberOfLikes
        {
            get
            {
                return (int)GetValue(NumberOfLikesDP);
            }
            set
            {
                SetValue(NumberOfLikesDP, value);
            }
        }

        public static DependencyProperty LikesListDP = DependencyProperty.Register("LikesList", typeof(List<SNLike>), typeof(FBLikeControl));

        public List<SNLike> LikesList
        {
            get
            {
                return (List<SNLike>)GetValue(LikesListDP);
            }
            set
            {
                SetValue(LikesListDP, value);
            }
        }

        private void LikesBt_Click(object sender, RoutedEventArgs e)
        {
            NamesListPopUp.IsOpen = true;
            NamesListPopUp.StaysOpen = false;
        }

        
    }
}
