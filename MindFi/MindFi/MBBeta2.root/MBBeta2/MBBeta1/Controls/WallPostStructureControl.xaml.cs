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
using MBBetaAPI.AgentAPI;

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
            CC = new CommonCode();
        }


        WallPostStructure CurrentWPS;
        CommonCode CC;


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
                // TODO: Localize
                MessageBox.Show("Error opening link");
            }
        }

        private void LikeBt_Click(object sender, RoutedEventArgs e)
        {

            WallPostStructure wps = WallPostStructureListIC.SelectedItem as WallPostStructure;

            FBAPI.UpdateLike(wps.ParentPost.SNID, ProcessUpdateLike);
            // TODO: Refresh UI to like/unlike
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

                FBAPI.AddComment(CurrentWPS.ParentPost.SNID, AddCommentTB.Text, ProcessUpdateLike);
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

        /// <summary>
        /// process update status response
        /// </summary>
        /// <param name="hwnd">who is calling the callback</param>
        /// <param name="result">was the request successful?</param>
        /// <param name="response">JSON person data</param>
        /// <param name="parent">CHECK: Reference to the user ID</param>
        /// <param name="parentSNID">CHECK: Reference to the user SNID</param>
        /// <returns>Request vas processed true/false</returns>
        public bool ProcessUpdateLike(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (result)
            {
                //FBPost statusObj = new FBPost(response);
                //statusObj.Parse();
                //statusObj.Message = this.PostNewStatusTB.Text;
                //statusObj.PostType = "status";
                //statusObj.ApplicationName = "Social Rewind";
                //string errorData;
                //statusObj.Save(out errorData);
                //if (errorData != "")
                //{
                //    ErrorMessage = errorData;
                //}
                /*
                FBAPI.UpdateLike(statusObj.SNID, ProcessNull);
                FBAPI.AddComment(statusObj.SNID, "test of a comment", ProcessNull);
                 */

                return true;
            }
            return false;
        }

        private void PersonBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            WallPostStructure wps = WallPostStructureListIC.SelectedItem as WallPostStructure;
            Person p = new Person(wps.ParentPost.InternalFromID);
            Window mainWindow = System.Windows.Window.GetWindow(this);


            DetailCard DetailCardWindow = new DetailCard(p,mainWindow);
            PersonWrapper dataContext = new PersonWrapper(p);
            DetailCardWindow.DataContext = dataContext;
            CC.PositionNewWindow(mainWindow, DetailCardWindow);

            this.Cursor = Cursors.Arrow;
        }

    }
}
