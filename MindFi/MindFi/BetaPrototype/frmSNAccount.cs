using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace MyBackup
{
    /// <summary>
    /// Form for adding Social Network accounts
    /// </summary>

    public partial class frmSNAccount : Form
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

	/// <summary>
	/// Form constructor
	/// </summary>
	public frmSNAccount()
	{
            InitializeComponent();
	}

        private void frmSNAccount_Load(object sender, EventArgs e)
        {
	    DateTime temp = DateTime.Now;
	    string tempD = temp.Year.ToString();
	    if ( temp.Month < 10 ) tempD += "0";
	    tempD += temp.Month;
	    if ( temp.Day < 10 ) tempD += "0";
	    tempD += temp.Day;

	    string MyDocBase = Environment.GetFolderPath(Environment.SpecialFolder.Personal) 
		+ "\\MyBackupTests\\" + tempD + ".db";
	    this.lblName.Text = MyDocBase;
            this.openFileDialog1.FileName = MyDocBase;
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
	}

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            
            this.openFileDialog1.ShowDialog();

// TODO: Separate folder name to simplify what is shown
            lblName.Text = openFileDialog1.FileName;
// Create database
	    if ( DBLayer.CreateDB ( lblName.Text ) )
	    {
		// MessageBox.Show("Success");
		LoadDB();
	    } else {
		// MessageBox.Show("Failure");
	    }
	    
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
                        //MessageBox.Show("Me saved: " + apiReq.Saved);
			//MessageBox.Show("Me error: " + apiReq.ErrorMessage);
			//MessageBox.Show("Me state: " + apiReq.State);
			    apiReq.Queue();
                        //MessageBox.Show("Me saved: " + apiReq.Saved);
			//MessageBox.Show("Me error: " + apiReq.ErrorMessage);
			//MessageBox.Show("Me state: " + apiReq.State);
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

        public bool PaintMe(int hwnd, bool result, string response, Int64? parent)
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
		me.Save(out errorData);
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
                btnOK.Enabled = true;
            }
        }

        private void txtURL_TextChanged(object sender, EventArgs e)
        {
            if (txtURL.Text != "" && txtAlias.Text != "")
            {
                btnOK.Enabled = true;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string errorData;

	    me.Save(out errorData);
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

            lblStatus.Text = "Data saved correctly";
            Refresh();

	    frmSelect frm2 = new frmSelect();
	    frm2.ShowDialog();
	    frmWizard frm3 = new frmWizard();
	    frm3.ShowDialog();
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
                btnOK.Enabled = false;
                me = null;
            }
            else
            {
                login = new FBLogin();
                login.LogOut();
            }
        }
    }

}