using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Configuration;

namespace MyBackup
{
    /// <summary>
    /// Login form for opening/creating a My Backup database
    /// </summary>

    public partial class frmLogin : Form
    {
        private bool createDB = false;
        private bool success = false;
        public int currentStep = -1;

        /// <summary>
        /// Form constructor
        /// </summary>
        public frmLogin()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialization, centers the form when opened
        /// </summary>
        private void frmLogin_Load(object sender, EventArgs e)
        {
            // center
            int X = (Screen.PrimaryScreen.WorkingArea.Size.Width
            - this.Size.Width) / 2;
            int Y = (Screen.PrimaryScreen.WorkingArea.Size.Height
            - this.Size.Height) / 2;
            this.Location = new System.Drawing.Point(X, Y);
            // get last opened from configuration file, if it exists
            lblName.Text = DBConfig.GetLastDBFromFile();
        }

        /// <summary>
        /// Checks for all text changes until condition allows for enabling Login button
        /// </summary>
        private void txtUser_TextChanged(object sender, EventArgs e)
        {
            bool temp = txtUser.Text != "" && txtPwd.Text != "" & lblName.Text != "";
            btnLogin.Enabled = temp;
        }

        /// <summary>
        /// User interface for creating a new database
        /// </summary>
        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            DateTime temp = DateTime.Now;
            string tempD = temp.Year.ToString();
            if (temp.Month < 10) tempD += "0";
            tempD += temp.Month;
            if (temp.Day < 10) tempD += "0";
            tempD += temp.Day;

            string MyDocBase = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\MyBackupTests\\";

            DialogResult res;
            MyDocBase += tempD;
            saveFileDialog1.InitialDirectory = MyDocBase;
            saveFileDialog1.FileName = MyDocBase+ "\\" + tempD + ".db";
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.Filter = "Database files (*.db)|*.db|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;

            this.saveFileDialog1.Title = "Select the name for the new database";
            res = saveFileDialog1.ShowDialog();
            if (res == DialogResult.Cancel)
            {
                return;
            }
            lblName.Text = saveFileDialog1.FileName;
            createDB = true;
        }

        /// <summary>
        /// User interface for opening an existing database
        /// </summary>
        private void btnOpen_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Title = "Select the name of the existing database";
            openFileDialog1.Filter = "Database files (*.db)|*.db|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            DialogResult res = this.openFileDialog1.ShowDialog();
            if (res == DialogResult.Cancel)
            {
                return;
            }
            lblName.Text = openFileDialog1.FileName;
            createDB = false;
        }

        /// <summary>
        /// User interface for entering the application
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // try to create / open the database with appropriate user/password
            if (createDB)
            {
                if (DBLayer.CreateDB(lblName.Text, txtUser.Text, txtPwd.Text))
                {
                    success = true;
                    MessageBox.Show("New database succesfully created");
                }
                else
                {
                    MessageBox.Show("Failed to create database");
                }
            }
            else
            {
                DBLayer.ConnString = "Data Source=" + lblName.Text + ";Password=" + txtPwd.Text;
                success = true;
            }
            // on success, save to config file and update login info to dashboard/main
            if (success)
            {
                try
                {
                    DBLayer.GetConn();
                    // TODO: Validate user is correct
                    string user;
                    string error;
                    DBLayer.GetConfig(out user, out currentStep, out error);
                    if (user != txtUser.Text)
                    {
                        MessageBox.Show("Cannot open the database " + lblName.Text + "\nReview your login/password\n\nError: " + error);
                        success = false;
                        return;
                    }
                    // save in config file: last user, last DB
                    System.Configuration.Configuration config =
                    ConfigurationManager.OpenExeConfiguration(
                        ConfigurationUserLevel.None);
                    DBConfig custSection = new DBConfig();
                    custSection.lastDB = lblName.Text;
                    if (config.Sections["Database"] == null)
                    {
                        config.Sections.Add("Database", custSection);
                    }
                    custSection = config.GetSection("Database") as DBConfig;
                    custSection.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Full);
                    /* MessageBox.Show("Saved configuration for " + custSection.SectionInformation.Name
                    + " at " + config.FilePath + " lastDB: " + custSection.lastDB); */
                    this.DialogResult = DialogResult.OK;
                    frmDashboard.LoginName = txtUser.Text;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot open the database " + lblName.Text + "\nReview your login/password\n\nError: " + ex.ToString());
                    success = false;
                }
            }
        }

        private void frmLogin_Closing(object sender, CancelEventArgs e)
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