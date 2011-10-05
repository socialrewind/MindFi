using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyBackup
{
    public partial class frmWizard : Form
    {
        int CurrentStep = 1;
        int PendingRequests = 0;
        int PendingSecondaryRequests;
        int InitialRequests = 0;
        int PlannedRequests = 0;
        int ProcessedRequests = 0;
        int CurrentNumErrors;
        const int SecondsToWaitPerStep = 10;
        int CurrentSecondsToWait = -1;
        DateTime startBackup;

        bool CurrentStepReady = false;
        bool CurrentStepResponse = false;
        bool MoreDataToRequest;

        string CurrentDetails;
        string CurrentError;

        // Data to keep
        private FBCollection friendsNotParsed;
        private FBCollection friendsParsing;
        private FBCollection friendsParsed;
        private ArrayList friendsPictures = new ArrayList();
        private FBCollection wall;

        MyBackupProfile current = frmMain.currentProfile;
/*
        private FBPhotoAlbums albumsToDownload;
        private FBFriendLists friendlistsToDownload;
        FBInbox inbox = null;
        //FBInbox outbox = null;
        FBEvents events = null;
        FBPhotoAlbums albums = null;
        FBFriendLists friendlists = null;
*/

        // sync variables
        private static AutoResetEvent waitingForFriends = new AutoResetEvent(false);
        
        /// <summary>
        /// For statistics, total number of friends found so far
        /// </summary>
        public int NumFriends { get; set; }
        /// <summary>
        /// For statistics, number of friends processed by the current step
        /// </summary>
        public int NumFriendsCurrentStep { get; set; }
        public int TotalInboxMessages { get; set; }
        //public int TotalOutboxMessages { get; set; }
        public int TotalWallPosts { get; set; }
        public int TotalEvents { get; set; }
        public int CurrentAlbum { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalPhotos { get; set; }
        public int CurrentList { get; set; }
        public int TotalLists { get; set; }

        private string ProfilePhotoDestinationDir; // = "C:\\Temp\\mybackup\\fb\\ProfilePics\\";
        private string AlbumDestinationDir; // = "C:\\Temp\\mybackup\\fb\\Albums";
        // TODO: test effects of fine tuning this variable. Use 1 for Debug, particularly for the parsers
        // it works now even with a higher value because friends are only saved at the end; this helps on performance but causes side effects on not having the data yet
        const int MAXCONCURRENTFRIENDS = 1;
        // USE 1 for debugging issues that are not related to concurrency
        const int SIZETOGETPERPAGE = 200; // 20;
        //const int SIZETOGETPERPAGE = 1; // debug


        #region "Initialization"
        public frmWizard()
        {
            InitializeComponent();
        }

        private void frmWizard_Load(object sender, EventArgs e)
        {
	    // TODO: read from configuration later, and/or select
	    DateTime temp = DateTime.Now;
	    string tempD = temp.Year.ToString();
	    if ( temp.Month < 10 ) tempD += "0";
	    tempD += temp.Month;
	    if ( temp.Day < 10 ) tempD += "0";
	    tempD += temp.Day;

	    ProfilePhotoDestinationDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) 
		+ "\\MyBackupTests\\" + tempD + "\\fb\\ProfilePics\\";
	    AlbumDestinationDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) 
		+ "\\MyBackupTests\\" + tempD + "\\fb\\Albums\\";

            btnPrevious.Enabled = false;
            btnPrevious.Visible = false;
            btnTryAgain.Enabled = false;
            btnTryAgain.Visible = false;
            btnNext.Enabled = true;
            richTextErrors.Visible = false;
            lblGlobalProgress.Visible = false;
            lblCurrentAction.Visible = false;
            progressCurrent.Visible = false;
            progressGlobal.Visible = false;
            DoStep();
        }

        #endregion

        #region "General interface"

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnTryAgain.Visible = true;
            btnTryAgain.Enabled = true;
            MoreDataToRequest = false;
            CurrentError += "Cancelled by user";
            CurrentNumErrors++;
            timer1.Enabled = false;
        }

        private void btnTryAgain_Click(object sender, EventArgs e)
        {
            switch (CurrentStep)
            {
                case 1:
                    MessageBox.Show("Cannot try again/continue yet on this step");
                    break;
                default:
                    btnTryAgain.Enabled = false;
                    DoStep();
                    break;
            }
        }

        private void NextAction()
        {
            btnNext.Enabled = false;
            if (CurrentStepReady)
            {
                CurrentStep++;
                // to get data faster for testing purposes
                //if (CurrentStep > 1 && CurrentStep < 11)
                //    CurrentStep = 11;
                if (CurrentStep == 2)
                {
                    startBackup = DateTime.Now;
                }

                PrepareStep();
                Refresh();
                DoStep();
            }
            Refresh();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            NextAction();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
            }
            Refresh();
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkDetails_CheckedChanged(object sender, EventArgs e)
        {
            richTextDetails.Visible = chkDetails.Checked;
            lblPendingRequests.Visible = chkDetails.Checked;
            lblPlannedRequests.Visible = chkDetails.Checked;
            lblTotalRequests.Visible = chkDetails.Checked;
            lblProcessedRequests.Visible = chkDetails.Checked;
        }

        private void ShowInfo()
        {
            lblPendingRequests.Text = "Pending requests: " + PendingRequests.ToString();
            lblPlannedRequests.Text = "Planned requests: " + PlannedRequests.ToString();
            lblProcessedRequests.Text = "Processed requests: " + ProcessedRequests.ToString();
            lblTotalRequests.Text = "Total requests: " + InitialRequests.ToString();
            if (CurrentStep > 1)
            {
                TimeSpan diff = (TimeSpan)(DateTime.Now - startBackup);
                // TODO: Check if TotalHours is appropriate, truncating to integer
                lblTime.Text = "Backup time " + diff.Hours + "h : " + diff.Minutes + "m : " + diff.Seconds + "s";
            }
        }
        #endregion

        #region "Interface per step"
        private void DoStep()
        {
            switch (CurrentStep)
            {
                case 1: // Information
                    lblInformation.Text = "This wizard will help you get our data for the current social network account";
                    richTextErrors.Text = CurrentError;
                    CurrentStepReady = true;
                    btnNext.Enabled = true;
                    lblTime.Visible = false;
                    break;
                case 2: // Get-show my data
                    richTextErrors.Visible = true;
                    chkDetails.Checked = true;
                    chkDetails.Visible = true;
                    RequestMyInfo();
                    MoreDataToRequest = false;
                    timer1.Interval = 100;
                    timer1.Enabled = true;
                    lblTime.Visible = true;
                    break;
                case 3: // Get my list of friends
                    RequestFriends();
                    MoreDataToRequest = false;
                    timer1.Interval = 100;
                    timer1.Enabled = true;
                    break;
                case 4: // get all general information for friends in the list
                    CurrentStepResponse = false;
                    RequestFriendsInfo();
                    timer1.Interval = 1000;
                    timer1.Enabled = true;
                    break;
		case 5:
                    RequestMyWall();
                    timer1.Interval = 1000;
                    timer1.Enabled = true;
                    btnNext.Enabled = false;
                    break;
		default:
                    lblInformation.Text = "End of the wizard so far. Wait until Requests Pending are 0.";
                    CurrentStepReady = true;
                    btnNext.Enabled = false;
                    btnNext.Visible = false;
                    btnFinish.Visible = true;
                    btnFinish.Enabled = true;
                    timer1.Enabled = false;
                    break;
            }
        }

        private void PrepareStep()
        {
            CurrentDetails = "";
            CurrentError = "";
            CurrentNumErrors = 0;
            richTextErrors.Text = "";
            richTextDetails.Text = "";

            CurrentStepReady = false;
            CurrentStepResponse = false;
            btnPrevious.Visible = true;
            btnTryAgain.Visible = false;
            if (CurrentStep > 1)
            {
                lblGlobalProgress.Text = "Step " + CurrentStep + " of 13";
                lblGlobalProgress.Visible = true;
                // assuming 7% each step, as they are 13
                if (CurrentStep <= 13)
                {
                    progressGlobal.Value = (CurrentStep - 1) * 7;
                }
                else
                {
                    progressGlobal.Value = 100;
                }
                progressGlobal.Visible = true;

                progressCurrent.Value = 0;
                progressCurrent.Visible = true;
            }
            switch (CurrentStep)
            {
                case 1: // Information
                    lblInformation.Text = "This wizard will help you get our data for the current social network account";
                    CurrentStepReady = true;
                    btnNext.Enabled = true;
                    break;
                case 2: // Get-show my data
			/* while getting profile pic
                    PendingRequests = 1;
                    PlannedRequests = 0;
                    InitialRequests = 1;
			*/
                    PendingRequests = 0;
                    PlannedRequests = 0;
                    InitialRequests = 0;
                    ProcessedRequests = 0;
                    lblInformation.Text = "Step #2: Get my data";
                    break;
                case 3: // Get my list of friends
                    lblInformation.Text = "Step #3: Get my friend list";
                    PendingRequests = 1;
                    PlannedRequests = 0;
                    InitialRequests = 1;
                    ProcessedRequests = 0;
                    break;
                case 4: // get all general information for friends in the list
                    lblInformation.Text = "Step #4: Get my friends info";
                    InitialRequests = NumFriends * 2;
                    PlannedRequests = NumFriends * 2;
                    PendingRequests = 0;
                    break;
		case 5: // Get my wall
                    PendingRequests = 0;
                    PendingSecondaryRequests = 0;
                    ProcessedRequests = 0;
                    PlannedRequests = 1; 
                    InitialRequests = 1;
                    lblInformation.Text = "Step #5: Getting my wall posts";
                    break;                
                default:
                    lblInformation.Text = "End of the wizard so far. Wait until Requests Pending are 0.";
                    CurrentStepReady = true;
                    btnNext.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Process the first timer, which updates the progress bars and calls successive requests when info is still expected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            ShowInfo();

            // switch (CurrentStep)
            // {
            // }


            richTextDetails.Text = CurrentDetails;
            if (CurrentStepResponse == true)
            {
                if (CurrentError != "")
                {
                    richTextErrors.Text = CurrentError;
                    richTextErrors.Enabled = true;
                    btnTryAgain.Visible = true;
                    btnTryAgain.Enabled = true;
                }
            }

            if ( ( PendingRequests == 0 && !MoreDataToRequest ) || CurrentStepReady )
            {
                if (PendingSecondaryRequests == 0)
                {
                    CurrentStepReady = true;
                    lblCurrentAction.Text = "Done!";
                    lblCurrentAction.Visible = true;
                    btnNext.Enabled = true;
                }
                else
                {
                    CurrentStepReady = true;
                    lblCurrentAction.Text = "Done with " + PendingSecondaryRequests + " secondary requests";
                    lblCurrentAction.Visible = true;
                    btnNext.Enabled = true;
                }
            }

            if (MoreDataToRequest)
            {
                DoStep();
            }

            if (!MoreDataToRequest)
            {

                switch (CurrentStep)
                {
                    case 4: // saving friends
                        if (PlannedRequests == 0 && friendsParsed.CurrentNumber == NumFriends)
                        {
                            CurrentStepReady = true;
                            if (CurrentNumErrors > 0)
                            {
                                lblCurrentAction.Text = "Done (with " + CurrentNumErrors + " errors)";
                            }
                            else
                            {
                                lblCurrentAction.Text = "Done (with possible errors or pending data)";
                            }
                            lblCurrentAction.Visible = true;
                            btnNext.Enabled = true;
                        }
                        break;
                }

                // autonext button
                if (CurrentStepReady && CurrentStep > 1)
                {

                    //if (current.autoNextButton)
                    {
                        if (CurrentSecondsToWait < 0)
                        {
                            CurrentSecondsToWait = SecondsToWaitPerStep;
                            timer2.Enabled = true;
                            timer2.Interval = 1000;
                        }
                    }
                }
            }

            // Calculate progress
            switch (CurrentStep)
            {
                default:
                    if (CurrentStepReady)
                    {
                        progressCurrent.Value = 100;
                    }
                    else
                    {
                        if (InitialRequests >= 0 && PlannedRequests >= 0 && PendingRequests >= 0)
                        {
                            progressCurrent.Value = (InitialRequests - PlannedRequests - PendingRequests) * 100 / InitialRequests;
                        }
                    }
                    break;
            }


            Refresh();
        }

        /// <summary>
        /// Process the second timer, which is the countdown to autopress "Next"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (current.autoNextButton)
            {
                CurrentSecondsToWait--;
                if (CurrentSecondsToWait < 0)
                {
                    btnNext.Text = "Next";
                    timer2.Enabled = false;
                    CurrentSecondsToWait = -1;
                    NextAction();
                }
                else
                {
                    btnNext.Text = "Next (" + CurrentSecondsToWait + ")";
                }
            }
        }

        #endregion

        #region "Request methods"
        /// <summary>
        /// Step #2
        /// </summary>
        private void RequestMyInfo()
        {
            if (!current.backupMyInformation)
            {
                lblCurrentAction.Text = "My information will not be backed up";
                lblCurrentAction.Visible = true;
                CurrentStepReady = true;
                return;
            }

               
            // TODO: Don't repeat the step if it was successful

            // Information
            lblCurrentAction.Text = "Getting my profile picture";
            lblCurrentAction.Visible = true;
            // create root directory if needed
            if (!System.IO.Directory.Exists(ProfilePhotoDestinationDir))
            {
                System.IO.Directory.CreateDirectory(ProfilePhotoDestinationDir);
            }
            // process
	    // TODO: Get My Profile Picture: NEED MY ID, SO GET CURRENT

	    System.Console.WriteLine("getting profile pic: " + current.fbProfile.SNID);
	    // TODO: relate requests but not send yet
	    AsyncReqQueue apiReq = FBAPI.ProfilePic(current.fbProfile.SNID,
		ProfilePhotoDestinationDir + current.fbProfile.ID + ".jpg", 
		ProcessFriendPic, current.fbProfile.ID, current.fbProfile.SNID);
		apiReq.Queue();
        }

        /// <summary>
        /// Step #3
        /// </summary>
        private void RequestFriends()
        {

            if (!current.backupFriendsInfo)
            {
                lblCurrentAction.Text = "Friends information will not be backed up";
                lblCurrentAction.Visible = true;
                PendingRequests = 0;
                PlannedRequests = 0;
                ProcessedRequests = 0;
                InitialRequests = 0;
                CurrentStepReady = true;
                return;
            }

            // Information
            lblCurrentAction.Text = "Getting a list of all my friends";
            lblCurrentAction.Visible = true;
            // process
            //FBAPI.Friends(SIZETOGETPERPAGE, ProcessFriends);
            AsyncReqQueue apiReq = FBAPI.Friends(3000, ProcessFriends);
	    apiReq.Queue();
        }

        /// <summary>
        /// Step #4
        /// </summary>
        private void RequestFriendsInfo()
        {

            if (!current.backupFriendsInfo)
            {
                lblCurrentAction.Text = "Friends information will not be backed up";
                lblCurrentAction.Visible = true;
                PendingRequests = 0;
                PlannedRequests = 0;
                ProcessedRequests = 0;
                InitialRequests = 0;
                CurrentStepReady = true;
                return;
            }


            // Information
            lblCurrentAction.Text = "Getting friends: " + NumFriendsCurrentStep + " of " + NumFriends;
            lblCurrentAction.Visible = true;
            MoreDataToRequest = true;

            if (friendsNotParsed != null && friendsNotParsed.items != null)
            {
                if (friendsParsing == null) friendsParsing = new FBCollection("FBPerson");
                if (friendsParsed == null) friendsParsed = new FBCollection("FBPerson");
                if (friendsNotParsed.CurrentNumber > 0)
                {
                    NumFriendsCurrentStep = friendsParsed.CurrentNumber;
                    for (int i = 0; i < MAXCONCURRENTFRIENDS; i++)
                    {
                        // get a friend
                        FBPerson currentFriend = (FBPerson)friendsNotParsed.Dequeue();
                        if (currentFriend != null)
                        {
                            friendsParsing.Queue(currentFriend);
                            PlannedRequests--;
                            PendingRequests++;
                            System.Console.WriteLine("getting profile: " + currentFriend.ID);
                            // FBAPI.Profile(currentFriend.ID, ProcessOneFriend);
            	            AsyncReqQueue apiReq = FBAPI.Profile(currentFriend.SNID, ProcessOneFriend);
	    		    apiReq.Queue();
			    PlannedRequests--;
			    PendingRequests++;
			    System.Console.WriteLine("getting profile pic: " + currentFriend.SNID);
			    apiReq = FBAPI.ProfilePic(currentFriend.SNID,
				ProfilePhotoDestinationDir + currentFriend.ID + ".jpg", 
				ProcessFriendPic, currentFriend.ID, currentFriend.SNID);
			    apiReq.Queue();
			    return;
                         }
                    }
                    // timer call
                    waitingForFriends.WaitOne();
                }

                if (friendsNotParsed.CurrentNumber == 0)
                {
                    NumFriendsCurrentStep = friendsParsed.CurrentNumber;
                    MoreDataToRequest = false;                    
                }
            }
        }

        /// <summary>
        /// Step #5
        /// </summary>
        private void RequestMyWall()
        {
            if (!current.backupMyPosts)
            {
                lblCurrentAction.Text = "Posts will not be backed up";
                lblCurrentAction.Visible = true;
                PendingRequests = 0;
                PlannedRequests = 0;
                ProcessedRequests = 0;
                InitialRequests = 0;
                CurrentStepReady = true;
                return;
            }

            // Information
            lblCurrentAction.Text = "Getting my wall posts " + TotalWallPosts + " / ???";
            lblCurrentAction.Visible = true;

            // check if there are enough posts up to the date...
            if (!CurrentStepResponse)
            {
                // don't try again unless it is a retry / todo
                if (PendingRequests == 0)
                {
                    PendingRequests++;
                    InitialRequests++;
                    AsyncReqQueue apiReq = FBAPI.Wall("me", SIZETOGETPERPAGE, ProcessWall);
		    apiReq.Queue();
		}
            }
            else
            {
                if (wall != null)
                {
                    // only one request at a time, this is not thread safe
                    if (PendingRequests == 0 && MoreDataToRequest && wall.Next != null)
                    {
                        PendingRequests++;
                        InitialRequests++;
                        AsyncReqQueue apiReq = FBAPI.MoreData(wall.Next, SIZETOGETPERPAGE, ProcessWall);
			apiReq.Queue();
                    }
                    else
                    {
                        // check errors
                        if (PendingRequests != 0 && !MoreDataToRequest)
                        {
                            CurrentError += "\nSome requests pending";
                            CurrentNumErrors++;
                        }
                    }
                }
                else
                {
                    // valid to have a null wall here
                    // check errors
                    if (PendingRequests != 0 && !MoreDataToRequest)
                    {
                        CurrentError += "\nSome requests pending";
                        CurrentNumErrors++;
                    }
                }
            }
        }


        #endregion

        #region "Response/Callback methods"

        /// <summary>
        /// Step 1: Get my profile picture
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool ProcessMe(int hwnd, bool result, string response, Int64? parent = null)
        {
            // response contains the filename or an error
            CurrentDetails = response;
            PendingRequests--;
            ProcessedRequests++;
MessageBox.Show("saving my own profile pic data: " + response);
            if (response.IndexOf(".jpg") > 0)
            {

                current.fbProfile.ProfilePic = response;
                string error = "";
                current.fbProfile.Save(out error);
                CurrentError += error;
                if (error != "")
                {
                    CurrentNumErrors++;
                }
            }
            else
            {
                CurrentError += response;
                CurrentNumErrors++;
            }
            CurrentStepResponse = true;
            return true;
        }

        private bool GenericProcess(int hwnd, bool result, string response, JSONParser temp, bool save = true, Int64? parent = null)
        {
            if (result)
            {
                string error;
                CurrentDetails = response;
                CurrentError = temp.lastError;
                if (save)
                {
                    temp.Save(out error);
                    CurrentError += error;
                    if (error != "")
                    {
                        CurrentNumErrors++;
                    }
                }
            }
            else
            {
                CurrentError += "ERROR: " + response;
                CurrentNumErrors++;
            }
            CurrentStepResponse = true;
            PendingRequests--;
            ProcessedRequests++;
            return true;
        }

        /// <summary>
        /// Step 2: get my friend list
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool ProcessFriends(int hwnd, bool result, string response, Int64? parent = null)
        {
            FBCollection friends = null;
            if (result)
            {
                CurrentDetails = response;
                friends = new FBCollection(response, "FBPerson");
		friends.Distance = 1;
                // get basic data
                friends.Parse();
		// save all children...
		string error;
		friends.Save(out error);
            }
            friendsNotParsed = friends;
            NumFriends = friends.CurrentNumber;
            // don't save friends yet - better later, once parsed all info, not just IDs
            return GenericProcess(hwnd, result, response, friends, false);
        }

        /// <summary>
        /// Step 3 : get friend data
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool ProcessOneFriend(int hwnd, bool result, string response, Int64? parent = null)
        {
            PendingRequests--;
            CurrentStepResponse = true;

            FBPerson currentFriend = new FBPerson(response, 1);
            CurrentDetails = response;
            currentFriend.Parse();
	    string error;
	    currentFriend.Save(out error);
            System.Console.WriteLine("got profile: " + currentFriend.ID + "\n" + error);
            // queue and remove
            friendsParsed.Queue(currentFriend);
            friendsParsing.Remove(currentFriend.SNID);
            // signal
            waitingForFriends.Set();
            ProcessedRequests++;
            return true;
        }

        /// <summary>
        /// Also in Step 3: Get friend profile picture
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool ProcessFriendPic(int hwnd, bool result, string response, long? parent = null)
        {
	    string errorData = "";

            PendingRequests--;
            if (result)
            {
                CurrentStepResponse = true;
                if (parent != null)
                {
                    // response contains the filename
                    // add to list of profile pics that are ready, consolidate until everything is finished
                    System.Console.Write("got profile pic: ");
                    System.Console.WriteLine(parent);
                    if (parent != null && response.IndexOf("jpg") > 0)
                    {
		        DBLayer.UpdateProfilePic((long) parent, response, out errorData);
                        ProcessedRequests++;
			if ( errorData == "" ) return true;
                    }
                }
            }

            // fails to process the pic, probably because of an exception
            CurrentError += errorData + "\n" + response;
            CurrentNumErrors++;
            return false;
        }

        /// <summary>
        /// Step #5
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="result"></param>
        /// <param name="response"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool ProcessWall(int hwnd, bool result, string response, Int64? parent = null)
        {

            wall = null;
            if (result)
            {
                CurrentDetails = response;
                wall = new FBCollection(response, "FBPost");
                wall.Parse();

                if (wall.items != null)
                {
                    TotalWallPosts += wall.CurrentNumber;
                }

                MoreDataToRequest = (wall.Next != null);

                string error;
                wall.Save(out error);
// TODO: Get likes
/*
                if (wall.items != null)
                {
                    foreach (FBPost currentPost in wall.items)
                    {
                        PendingSecondaryRequests++;
                        FBAPI.Likes(currentPost.ID, SIZETOGETPERPAGE, ProcessLikesInPost, currentPost.PostIdentity);
                    }
                }
*/
                CurrentError = error;
                if (error != "")
                {
                    CurrentNumErrors++;
                }
            }
	    return GenericProcess(hwnd, result, response, wall, false);
        }

        #endregion

    }

    #region "Helper classes"

    /// <summary>
    /// Moved from the former AsyncFBBackup.cs, contains the association between photo ID and path it is saved
    /// </summary>
    class friendsPics
    {
        public Int64? ID;
        public string PicturePath;

        public friendsPics(Int64? id, string path)
        {
            ID = id;
            PicturePath = path;
        }

    }

    #endregion
}
