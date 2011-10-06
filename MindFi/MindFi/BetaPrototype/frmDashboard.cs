using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace MyBackup
{
    /// <summary>
    /// Main form for the new agent interface
    /// </summary>
    public partial class frmDashboard : Form
    {
	/// <summary>
	/// User logged in
	/// </summary>
	public static string LoginName;

	#region "Download info"
        private string BaseDir;
	#endregion

	#region "Process control"
        private bool firstTime = true;
	#endregion

	/// <summary>
	/// Form constructor
	/// </summary>
	public frmDashboard()
	{
            InitializeComponent();
	    GetPaths();
	}

	private void GetPaths()
	{
	    string dbPath = DBConfig.GetLastDBFromFile();
	    
	    if ( dbPath != "" )
	    {
	        FileInfo baseFile = new FileInfo ( dbPath );
	        BaseDir = baseFile.DirectoryName;
	    }
	    else
	    {
	        BaseDir = "";
	    }
	    
	    if ( BaseDir == "" )
	    {
		DateTime temp = DateTime.Now;
		string tempD = temp.Year.ToString();
		if ( temp.Month < 10 ) tempD += "0";
		tempD += temp.Month;
		if ( temp.Day < 10 ) tempD += "0";
		tempD += temp.Day;
		AsyncReqQueue.ProfilePhotoDestinationDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) 
		    + "\\MyBackupTests\\" + tempD + "\\fb\\ProfilePics\\";
		AsyncReqQueue.AlbumDestinationDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) 
		    + "\\MyBackupTests\\" + tempD + "\\fb\\Albums\\";
	    }
	    else
	    {
		AsyncReqQueue.ProfilePhotoDestinationDir = BaseDir + "\\fb\\ProfilePics\\";
		AsyncReqQueue.AlbumDestinationDir = BaseDir + "\\fb\\Albums\\";
	    }
	    // TODO: Save directory to database
	    if (!Directory.Exists(AsyncReqQueue.ProfilePhotoDestinationDir))
            {
                Directory.CreateDirectory(AsyncReqQueue.ProfilePhotoDestinationDir);
            }
            if (!Directory.Exists(AsyncReqQueue.AlbumDestinationDir))
            {
                Directory.CreateDirectory(AsyncReqQueue.AlbumDestinationDir);
            }
	}

	/// <summary>
	/// When the form is opened, verify config and allow to call next forms appropriately
	/// </summary>
        private void frmDashboard_Load(object sender, EventArgs e)
        {
	    // "maximize" form
	    this.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom 
		| AnchorStyles.Left | AnchorStyles.Right);
	    frmLogin f1 = new frmLogin();
	    DialogResult res = f1.ShowDialog();
	//MessageBox.Show("res: " + res.ToString());
	    // processing using data from frmLogin
	    if ( res == DialogResult.OK )
	    {
		SetLoginText();
	    }
	    else
	    {
		Application.Exit();
		//this.labelWelcome.Text = "Not logged";
	    }
	    PopulateAccounts();
	}

	/// <summary>
	/// Generate text for all logged-in information
	/// </summary>
	private void SetLoginText()
	{
	    this.labelWelcome.Text = "Logged in MyBackup as: " + frmDashboard.LoginName;
	    if ( FBLogin.loggedIn )
	    {
		this.labelWelcome.Text += "\nLogged in Facebook as: " + FBLogin.LoginName; // + "\n" + FBLogin.accessToken;
	    }
	    else
	    {
		this.labelWelcome.Text += "\nNot logged in Facebook";
	    }
	}

	/// <summary>
	/// Processing event for adding a new social network account
	/// </summary>
        private void btnAddAccount_Click(object sender, EventArgs e)
        {
	    frmAddAccount temp = new frmAddAccount();
	    DialogResult res = temp.ShowDialog();
	    temp.Dispose();
	    if ( res == DialogResult.Cancel )
	    {
		MessageBox.Show("Account was not added");
	    	SetLoginText();
		this.Refresh();
		return;
	    }
	    // refresh data
	    PopulateAccounts();
        }

	/// <summary>
	/// Processing event for removing social network accounts
	/// </summary>
        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
	    DialogResult res = MessageBox.Show("Are you sure to delete the selected accounts?", 
		"Confirm delete", MessageBoxButtons.YesNo);
	    if ( res != DialogResult.Yes )
	    {
		return;
	    }

	    ArrayList temp = (ArrayList) this.accountsGrid.DataSource;
	    if ( temp != null )
	    {
		foreach ( SNAccount account in temp )
		{
		    if ( account.Remove )
		    {
			string error;
			if ( !DBLayer.DeleteAccount(account.ID, out error) )
			{
			    MessageBox.Show ( "Error deleting account: " + error );
			}
		    }
		}
	    }
	    // refresh data
	    PopulateAccounts();
        }

	/// <summary>
	/// Fills the data grid with the list of accounts in the database
	/// </summary>
        private void PopulateAccounts()
        {
	    string error;
	    this.accountsGrid.DataSource = null;
	    this.accountsGrid.DataSource = DBLayer.GetAccounts(out error);
	    if ( error != "" )
	    {
		MessageBox.Show("Error getting accounts:\n" + error );
	    }
	    UpdateForm();
	}

	/// <summary>
	/// Processing event for logging into Facebook
	/// </summary>
        private void btnLoginFB_Click(object sender, EventArgs e)
        {
	    FBLogin login = new FBLogin();
	    login.Login();
	    processTimer.Enabled = true;
	    processTimer.Interval = 5000;
                    
	    // wait or callback?
	    UpdateForm();
        }

	/// <summary>
	/// Processing event for refresh button
	/// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
	    UpdateForm();
        }

	/// <summary>
	/// General refresh for UI
	/// </summary>
        private void UpdateForm()
        {
	    SetLoginText();
	    this.Refresh();	    
        }

	/// <summary>
	/// General refresh for UI
	/// </summary>
        private void UpdateStats()
        {
	    string error = "";

	// TODO: accumulate errors
	    if ( error == "" )
	    {
		this.labelStats.Text = "Total friends: " + AsyncReqQueue.nFriends 
			+ ". Friends processed: " + AsyncReqQueue.nFriendsProcessed
			+ ". Friends pictures: " + AsyncReqQueue.nFriendsPictures
			+ ". Total Posts: " + AsyncReqQueue.nPosts 
			+ ". Total Messages: " + AsyncReqQueue.nMessages 
			+ ". Total Albums: " + AsyncReqQueue.nAlbums
			+ ". Total Photos: " + AsyncReqQueue.nPhotos;
		this.labelStats.Text += ".\nQueued: " + AsyncReqQueue.QueuedReqs;
		this.labelStats.Text += ". Sent: " + AsyncReqQueue.SentReqs;
		this.labelStats.Text += ". Retry: " + AsyncReqQueue.RetryReqs;
	    	this.labelStats.Text += ". Received: " + AsyncReqQueue.ReceivedReqs;
	    	this.labelStats.Text += ". Failed: " + AsyncReqQueue.FailedReqs;
	    	this.labelStats.Text += ". Parsed: " + AsyncReqQueue.ParsedReqs;
	    	this.labelStats.Text += ". No need parsing: " + AsyncReqQueue.NotParsedReqs;

	    }
	    else
	    {
		this.labelStats.Text = error;
	    }
        }

	/// <summary>
	/// Processing event for regular timer
	/// </summary>
        private void processTimer_Tick(object sender, EventArgs e)
        {
	    if ( FBLogin.Me != null )
	    {
		if (firstTime )
		{
		    firstTime = false;
//DEBUG
		    AsyncReqQueue.InitialRequests();
// DEBUG
//processTimer.Interval = 20000;
		}
		else
		{
		    string ErrorMessage;
		    AsyncReqQueue.PendingRequests(out ErrorMessage);
		    this.labelInfo.Text = ErrorMessage;
		}
	    }

	    // Get next step depending on the requests / step?
	    // Get statistics
	    UpdateStats();
	    UpdateForm();
	}

    }

}