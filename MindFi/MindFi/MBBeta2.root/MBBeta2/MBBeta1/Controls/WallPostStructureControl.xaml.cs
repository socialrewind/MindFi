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
            Button LikeBt = sender as Button;

            WallPostStructure wps = WallPostStructureListIC.SelectedItem as WallPostStructure;


            //Going from Like to Unlike
            if (wps.ParentPost.ILiked == true)
            {
                LocTextExtension loc = new LocTextExtension("MBBeta2:WPStrings:Like");
                loc.SetBinding(LikeBt, Button.ContentProperty);
                wps.ParentPost.ILiked = false;
            }
            else 
            {
                //Going from unlike to like
                LocTextExtension loc = new LocTextExtension("MBBeta2:WPStrings:Unlike");
                loc.SetBinding(LikeBt, Button.ContentProperty);
                wps.ParentPost.ILiked = true;
            }

            if (wps.ParentPost.ILiked == true)
            {
                //MessageBox.Show("Process like");
                FBAPI.UpdateLike(wps.ParentPost.SNID, AsyncReqQueue.ProcessLike);
            }
            else
            {
                //MessageBox.Show("Process unlike");
                FBAPI.UpdateUnlike(wps.ParentPost.SNID, AsyncReqQueue.ProcessUnlike);
            }
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

        private WallPost BuildFlashPost(string Message)
        {
            PersonLight Me = new PersonLight(1);

            WallPost NewPost = new WallPost();
            NewPost.Date = DateTime.Today;
            NewPost.Message = Message;
            NewPost.FromName = Me.Name;
            NewPost.FromID = Me.SNID.ToString();
            NewPost.WallName = Me.Name;
            NewPost.FromPhotoPath = Me.ProfilePic;

            return NewPost;
        }

        private void AddCommentTB_KeyDown(object sender, KeyEventArgs e)
        {

            WallPostStructure wps = WallPostStructureListIC.SelectedItem as WallPostStructure;

            if (e.Key == Key.Enter)
            {

                FBAPI.AddComment(CurrentWPS.ParentPost.SNID, AddCommentTB.Text, ProcessUpdateComment);
                //Build a new in-memory post. Valid only for showing immediately after posting. Next time window is refreshed it should be read from DB
                CurrentWPS.ChildPosts.Add(BuildFlashPost(AddCommentTB.Text));
                //Check if it's first post. If it is, enable comments list by setting comments count > 0
                if (CurrentWPS.ChildPosts.Count() == 1)
                {
                    CurrentWPS.ParentPost.CommentsCount = 1;
                }

                //Close Popup
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
        public bool ProcessUpdateComment(int hwnd, bool result, string response, long? parent = null, string parentSNID = "")
        {
            if (result)
            {
                FBPost statusObj = new FBPost(response);
                statusObj.Parse();
                // TODO: Find the parentID / parentSNID in the database, once it is saved
                bool Saved;
                string error;
                DBLayer.PostDataUpdateSNID(parent.Value, statusObj.SNID, out Saved, out error);
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

                return true;
            }
            return false;
        }

        private void PersonBt_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            string photo = "";
            string photo2 = "";

            WallPostStructure wps = WallPostStructureListIC.SelectedItem as WallPostStructure;
            Person p = new Person(wps.ParentPost.InternalFromID);
            //Check if photo is equal before and after Detail Card is open
            photo = wps.ParentPost.FromPhotoPath;

            Window mainWindow = System.Windows.Window.GetWindow(this);

            DetailCard DetailCardWindow = new DetailCard(p,mainWindow);
            PersonWrapper dataContext = new PersonWrapper(p);
            DetailCardWindow.DataContext = dataContext;
            CC.PositionNewWindow(mainWindow, DetailCardWindow);

            photo2 = p.ProfilePic;
            if (photo != photo2)
                wps.ParentPost.FromPhotoPath = p.ProfilePic;

            this.Cursor = Cursors.Arrow;
        }

    }
}
