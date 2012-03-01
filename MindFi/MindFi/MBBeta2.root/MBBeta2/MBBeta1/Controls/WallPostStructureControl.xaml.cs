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

//Localization
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;
using System.Globalization;

namespace MBBeta2.Controls
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


        WallPostStructure CurrentWPS;


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

        private void LikeBt_Click(object sender, RoutedEventArgs e)
        {

            WallPostStructure wps = WallPostStructureListIC.SelectedItem as WallPostStructure;

            MessageBox.Show("Comment on " + wps.ParentPost.SNID);


        }

        private void CommentBt_Click(object sender, RoutedEventArgs e)
        {
            CurrentWPS = WallPostStructureListIC.SelectedItem as WallPostStructure;
            CommentsPopUp.IsOpen = true;
            Keyboard.Focus(AddCommentTB);
        }

        private void AddCommentTB_GotFocus(object sender, RoutedEventArgs e)
        {
            AddCommentTB.Text = "";
        }

        private void AddCommentTB_LostFocus(object sender, RoutedEventArgs e)
        {
            LocTextExtension loc = new LocTextExtension("MBBeta2:WPStrings:WriteAComment");
            loc.SetBinding(AddCommentTB, TextBox.TextProperty);
            CommentsPopUp.IsOpen = false;
        }

        private void AddCommentTB_KeyDown(object sender, KeyEventArgs e)
        {

            WallPostStructure wps = WallPostStructureListIC.SelectedItem as WallPostStructure;

            if (e.Key == Key.Enter)
            {
                

                //The code to post an status goes here
                

                MessageBox.Show("Post a comment code goes here... Post#: " + CurrentWPS.ParentPost.SNID);

                CommentsPopUp.IsOpen = false;

                e.Handled = true;
            }
            if (e.Key == Key.Escape)
            {
                CommentsPopUp.IsOpen = false;
                e.Handled = true;
            }
        }

        private void CloseCommentBt_Click(object sender, RoutedEventArgs e)
        {
            CommentsPopUp.IsOpen = false;
        }
    }
}
