using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MyBackup
{
    /// <summary>
    /// Form for adding a social network account
    /// </summary>
    public partial class frmAddAccount : Form
    {
	private ArrayList m_snlist;
        private SocialNetwork m_sn;
        private FBLogin login;
        private bool loggedIn;
	private string meData;
        private FBPerson me;
	private AsyncReqQueue apiReq;
	private bool MyDataShown = false;
	public static MyBackupProfile currentProfile;
	private bool success;

	public frmAddAccount()
	{
            InitializeComponent();
	}

        private void frmAddAccount_Load(object sender, EventArgs e)
        {
	    LoadDB();
	    lblStatus.Text = "Select your social network and login on it to configure the account";
	}

	private void LoadDB()
	{
            string err;
            m_snlist = DBLayer.GetSNList(out err);
            if (m_snlist == null)
            {
                lblStatus.Text = "Error loading data: " + err;
                return;
            }
	    // clean list of Social Networks
	    cmbSocialNetworks.DataSource = null;
	    cmbSocialNetworks.Items.Clear();
            cmbSocialNetworks.DataSource = m_snlist;
            if (m_snlist.Count > 0)
            {
                cmbSocialNetworks.DisplayMember = "Name";
                cmbSocialNetworks.ValueMember = "ID";
            }
	    // clean list of Profiles
	    cmbProfiles.DataSource = null;
	    cmbProfiles.Items.Clear();
	    // TODO: Improve SNProfile class to describe download pattern
	    ArrayList m_pList = new ArrayList();
	    m_pList.Add ( new SNProfile(1,"Basic") );
	    m_pList.Add ( new SNProfile(2,"Extended") );
	    m_pList.Add ( new SNProfile(3,"Stalker") );
	    cmbProfiles.DataSource = m_pList;
            if (m_pList.Count > 0)
            {
                cmbProfiles.DisplayMember = "Name";
                cmbProfiles.ValueMember = "ID";
            }
	    // clean list of Frequency
	    cmbFrequency.DataSource = null;
	    cmbFrequency.Items.Clear();
	    // TODO: Add appropriate list of options depending on unit, change smartly
	    // TODO: Get frequency options from Db
	    ArrayList m_FList = new ArrayList();
	    m_FList.Add ( new Frequency(1,"minutes", 1) );
	    m_FList.Add ( new Frequency(2,"hours", 1) );
	    m_FList.Add ( new Frequency(3,"days", 1) );
	    cmbFrequency.DataSource = m_FList;
            if (m_FList.Count > 0)
            {
                cmbFrequency.DisplayMember = "Name";
                cmbFrequency.ValueMember = "ID";
            }
	    // clean list of Frequency values
	    cmbFrequencyValue.DataSource = null;
	    cmbFrequencyValue.Items.Clear();
	    for ( int i = 1; i <= 90 ; i++ )
	    {
	    	cmbFrequencyValue.Items.Add( i );
	    }
	    cmbFrequencyValue.SelectedIndex = 4; // desired - 1
	}

        private void SNUpdate()
        {
            m_sn = (SocialNetwork)cmbSocialNetworks.SelectedItem;
            // m_profile.socialNetworkID = m_sn.SNID;
            btnLoginSN.Text = "Login into " + m_sn.Name;
            btnLoginSN.Enabled = true;
        }

        private void cmbSocialNetworks_SelectedIndexChanged(object sender, EventArgs e)
        {
            SNUpdate();
        }

        private void cmbSocialNetworks_Click(object sender, EventArgs e)
        {
            SNUpdate();
        }

        private void btnLoginSN_Click(object sender, EventArgs e)
        {
            txtURL.Text = "";
            txtAlias.Text = "";
            // prevent from double click
            btnLoginSN.Enabled = false;
            // TODO: other Social Networks
            switch (m_sn.ID)
            {
                case 1: // Facebook
                    loggedIn = false;
                    lblStatus.Text = "Logging in, you will be asked for permissions in the application 'OldWView'";
                    login = new FBLogin();
                    step2Timer.Enabled = true;
                    step2Timer.Interval = 1000;
		    login.Login();
                    break;
                default:
                    lblStatus.Text = "Login is not yet implemented for that social network";
                    MessageBox.Show(lblStatus.Text);
                    break;
            }
        }

        private void step2Timer_Tick(object sender, EventArgs e)
        {
            // part 1: log in
            if (!loggedIn)
            {
                // TODO: other Social Networks
                switch (m_sn.ID)
                {
                    case 1: // Facebook
                        if (FBLogin.loggedIn)
                        {
                            loggedIn = true;
                            lblStatus.Text = "Logged in";
                            meData = "";
                            // call FB API to get my data
                            apiReq = FBAPI.Me(PaintMe);
                            apiReq.Queue();
                            apiReq.Send();
                            step2Timer.Enabled = true;
                        }
                        else
                        {
                            // re-enable
                            step2Timer.Enabled = true;
                        }
                        Refresh();
                        break;
                    default:
                        lblStatus.Text = "Login is not implemented for that social network in MyBackup Alpha";
                        MessageBox.Show(lblStatus.Text);
                        step2Timer.Enabled = false;
                        break;
                }
            }
            else
            {
                // once logged in, just poll for the data
                // TODO: check if reenabling, depending on time for the data to come back
                lblStatus.Text = "Logged in\nData: " + meData;
                if (me == null || !me.Parsed )
                {
                    // re-enable
                    step2Timer.Enabled = true;
                meData = meData + "waiting for parse...";
                }
                else
                {
                    if (me.UserName !=null && me.UserName != "")
                    {
                        txtAlias.Text = me.UserName;
                    }
                    else
                    {
                        txtAlias.Text = me.SNID;
                    }
                    txtURL.Text = me.Link;
		    labelSNID.Text = me.SNID;
                    step2Timer.Enabled = false;
		    if ( !MyDataShown )
		    {
			MyDataShown = true;			
//                        MessageBox.Show("Education history (#): " + me.Education.Count);
//                        MessageBox.Show("Work history (#): " + me.Work.Count);
		    }
                }

            }
        }

        public bool PaintMe(int hwnd, bool result, string response, Int64? parent, string parentSNID)
        {
	    // TODO: Process async request object reference 
            // update object which is refreshed asynchronously
            if (result)
            {
                // distance to Me: 0
                //string error;
                me = new FBPerson(response, 0);
                me.Parse();
		meData = response + "\n" + me.lastError;
		string errorData = "";
//		me.Save(out errorData);
		meData += "\n" + errorData;
		
            }
            else
            {
                meData = "ERROR: " + response;
            }
            return true;
        }

        private void txtAlias_TextChanged(object sender, EventArgs e)
        {
            if (txtURL.Text != "" && txtAlias.Text != "")
            {
                btnSave.Enabled = true;
            }
        }

        private void txtURL_TextChanged(object sender, EventArgs e)
        {
            if (txtURL.Text != "" && txtAlias.Text != "")
            {
                btnSave.Enabled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string errorData = "";

//	    me.Save(out errorData);
            meData += errorData;
            if (errorData != "")
            {
                if (MessageBox.Show(errorData, "Error while saving, are you sure to exit?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    Close();
                }
            }

	    currentProfile = new MyBackupProfile();
            currentProfile.fbProfile = me;
            currentProfile.userName = txtAlias.Text;
            currentProfile.socialNetworkURL = txtURL.Text;
	    if ( !DBLayer.SaveAccount(txtAlias.Text, me.EMail, m_sn.ID, me.SNID, me.Link, out errorData) )
	    {
		lblStatus.Text = errorData;
                MessageBox.Show("Error while saving:\n" + errorData);
		return;
	    }
            lblStatus.Text = "Data saved correctly";
            Refresh();
	    success = true;
	    Close();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (login != null)
            {
                login.LogOut();
                txtAlias.Text = "";
                txtURL.Text = "";
                btnLoginSN.Enabled = true;
                btnSave.Enabled = false;
                me = null;
            }
            else
            {
                login = new FBLogin();
                login.LogOut();
            }
        }

	private void frmAddAccount_Closing(object sender, CancelEventArgs e)
	{
	    if (success)
	    {
		this.DialogResult = DialogResult.OK;
	    }
	    else
	    {
		this.DialogResult = DialogResult.Cancel;
	    }
	}

    }

}