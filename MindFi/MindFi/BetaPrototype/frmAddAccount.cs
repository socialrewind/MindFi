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
        //private FBPerson me;
        //private AsyncReqQueue apiReq;
        private bool MyDataShown = false;
        private bool success;
        private int currentSN;

        public frmAddAccount(int SN)
        {
            InitializeComponent();
            currentSN = SN;
        }

        private void frmAddAccount_Load(object sender, EventArgs e)
        {
            LoadDB();
            lblStatus.Text = "Select your social network and login on it to configure the account";
            DoLogin();
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
            m_pList.Add(new SNProfile(MyBackupProfile.BASIC, "Basic"));
            m_pList.Add(new SNProfile(MyBackupProfile.EXTENDED, "Extended"));
            m_pList.Add(new SNProfile(MyBackupProfile.STALKER, "Stalker"));
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
            m_FList.Add(new Frequency(1, "minutes", 1));
            m_FList.Add(new Frequency(2, "hours", 1));
            m_FList.Add(new Frequency(3, "days", 1));
            cmbFrequency.DataSource = m_FList;
            if (m_FList.Count > 0)
            {
                cmbFrequency.DisplayMember = "Name";
                cmbFrequency.ValueMember = "ID";
            }
            // clean list of Frequency values
            cmbFrequencyValue.DataSource = null;
            cmbFrequencyValue.Items.Clear();
            for (int i = 1; i <= 90; i++)
            {
                cmbFrequencyValue.Items.Add(i);
            }
            cmbFrequencyValue.SelectedIndex = 4; // desired - 1
            cmbSocialNetworks.SelectedValue = currentSN;
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
            DoLogin();
        }

        private void DoLogin()
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
                FBPerson me = FBLogin.Me;
                if (me == null || !me.Parsed)
                {
                    // re-enable
                    step2Timer.Enabled = true;
                    meData = meData + "waiting for parse...";
                }
                else
                {
                    step2Timer.Enabled = false;
                    meData = FBLogin.LastError;

                    if (me.UserName != null && me.UserName != "")
                    {
                        txtAlias.Text = me.UserName;
                    }
                    else
                    {
                        txtAlias.Text = me.SNID;
                    }
                    txtURL.Text = me.Link;
                    labelSNID.Text = me.SNID;
                    if (!MyDataShown)
                    {
                        MyDataShown = true;
                        //                        MessageBox.Show("Education history (#): " + me.Education.Count);
                        //                        MessageBox.Show("Work history (#): " + me.Work.Count);
                    }
                }
                lblStatus.Text = "Logged in\nData: " + meData;
            }
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
            /*
            
            //	    me.Save(out errorData);
            meData += errorData;
            if (errorData != "")
            {
                if (MessageBox.Show(errorData, "Error while saving, are you sure to exit?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    Close();
                }
            }
            */

            // TODO: support multiple profiles
            // For now, verifying only if there is no currentProfile
            FBPerson me = FBLogin.Me;
            if (frmDashboard.currentProfile != null)
            {
                if (( frmDashboard.currentProfile.userName != me.Name
                    && frmDashboard.currentProfile.SocialNetworkAccountID != me.SNID
                    && frmDashboard.currentProfile.socialNetworkURL != me.Link ) ||
                    frmDashboard.currentProfile.socialNetworkID != (int)cmbSocialNetworks.SelectedValue
                    )
                {
                    MessageBox.Show("You tried to login with a different account (" + me.Name
                        + ") instead of the selected account (" + frmDashboard.currentProfile.userName
                        + "). Please correct your data; login cancelled.");
                    return;
                }
            }

            frmDashboard.currentProfile = new MyBackupProfile();
            frmDashboard.currentProfile.fbProfile = me;
            frmDashboard.currentProfile.userName = me.Name; // before: txtAlias.Text, check impact
            frmDashboard.currentProfile.socialNetworkURL = me.Link; // before: txtURL.Text
            frmDashboard.currentProfile.socialNetworkID = (int?)cmbSocialNetworks.SelectedValue;
            frmDashboard.currentProfile.SocialNetworkAccountID = me.SNID;
            if (cmbProfiles.SelectedItem != null)
            {
                frmDashboard.currentProfile.currentBackupLevel = (int)cmbProfiles.SelectedValue;
            }
            if (!DBLayer.SaveAccount(me.Name, me.EMail, m_sn.ID, me.SNID, me.Link, frmDashboard.currentProfile.currentBackupLevel, out errorData))
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
                // me = null;
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