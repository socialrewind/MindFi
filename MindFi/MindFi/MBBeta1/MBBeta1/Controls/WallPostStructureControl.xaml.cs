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

namespace MBBeta1.Controls
{
    /// <summary>
    /// Interaction logic for WallPostStructureControl.xaml
    /// </summary>
    public partial class WallPostStructureControl : UserControl
    {
        public WallPostStructureControl()
        {
            InitializeComponent();
        }


        public static DependencyProperty ParentPostDP = DependencyProperty.Register("ParentPost", typeof(WallPost), typeof(WallPostStructureControl));

        public WallPost ParentPost
        {
            get
            {
                return ((WallPost)GetValue(ParentPostDP));
            }
            set
            {
                SetValue(ParentPostDP, value);
            }
        }


        public static DependencyProperty NumberOfLikesDP = DependencyProperty.Register("NumberOfLikes", typeof(int), typeof(WallPostStructureControl));

        public int NumberOfLikes
        {
            get
            {
                return ((int)GetValue(NumberOfLikesDP));
            }
            set
            {
                SetValue(NumberOfLikesDP, value);
            }
        }


        public static DependencyProperty PostListDP = DependencyProperty.Register("PostList", typeof(List<WallPost>), typeof(WallPostStructureControl));

        public List<WallPost> PostList
        {
            get
            {
                return (List<WallPost>)GetValue(PostListDP);
            }
            set
            {
                SetValue(PostListDP, value);
            }
        }




        private void PostHyperLink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = e.OriginalSource as Hyperlink;
            try
            {
                System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri);
            }
            catch
            {
                MessageBox.Show("Error openning link");
            }
        }
    }
}
