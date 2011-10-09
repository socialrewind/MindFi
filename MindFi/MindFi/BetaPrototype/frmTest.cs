using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Reflection;

namespace MyBackup
{
    public partial class frmTest : Form
    {
        const string password = "hola";
        string connString;
        bool firstTime = true;

        string SQL;
        SQLiteCommand command;
        SQLiteConnection conn;
        SQLiteDataReader reader;
        JSONParser currentRecord;

        public frmTest()
        {
            InitializeComponent();
        }

        private void frmTest_Load(object sender, EventArgs e)
        {
        }

        private void DoQuery()
        {
            if (firstTime)
            {
                connString = "Data Source=" + openFileDialog1.FileName + ";Password=" + txtPwd.Text;
                conn = new SQLiteConnection(connString);
                conn.Open();
                firstTime = false;
            }
            // TODO: add state selection
            SQL = "select [ID],[RequestType],[ResponseValue] from RequestsQueue";
            command = new SQLiteCommand(SQL, conn);
            reader = command.ExecuteReader();
            lblNumRecords.Text = reader.RecordsAffected.ToString();
            if (reader.Read())
            {
                FillData();
            }
        }

        private void FillData()
        {
            lblRecordType.Text = reader.GetString(1);
            if ( !reader.IsDBNull(2))
            {
                richTextBox1.Text = reader.GetString(2);
            }
            else
            {
                richTextBox1.Text = "";
            }
        }

        private void frmTest_FormClosed(object sender, FormClosedEventArgs e)
        {
            reader.Close();
            conn.Close();
        }

        private void btnGet_Click(object sender, EventArgs e)
        {
            if (firstTime)
            {
                DoQuery();
            }
            if (reader.Read())
            {
                FillData();
            }
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            lblError.Text = "Parsing";

            switch (lblRecordType.Text)
            {
                case "MyBackupAgents.FBWall":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBPost");
                    break;
                case "MyBackupAgents.FBEvents":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBEvent");
                    break;
                case "MyBackupAgents.FBInbox":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBMessage");
                    break;
                case "MyBackupAgents.FBPhotos":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBPhoto", null);
                    break;
                case "MyBackupAgents.FBPhotoAlbums":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBAlbum");
                    break;
                case "MyBackupAgents.FBFriendLists":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBPerson");
                    break;
                /*case "MyBackupAgents.FBFriendList":
                    currentRecord = new FBFriendList(richTextBox1.Text, "FBPerson");
                    break;*/
                case "FBPerson":
                    currentRecord = new FBPerson(richTextBox1.Text, 0, null);
                    break;
                case "FBFriends":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBPerson");
                    break;
                case "FBInbox":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBMessage");
                    break;
                case "FBWall":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBPost");
                    break;
                case "FBAlbums":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBAlbum");
                    break;
                case "FBPhotos":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBPhoto");
                    break;
                case "FBLikes":
                    currentRecord = new FBLikes(richTextBox1.Text);
                    break;
                case "FBEvents":
                    currentRecord = new FBCollection(richTextBox1.Text, "FBEvent");
                    break;
            }

            if (currentRecord != null)
            {
                currentRecord.Parse();
                lblError.Text = "Parsed: " + currentRecord.lastError;
            }
        }

        /*
                private void btnSave_Click(object sender, EventArgs e)
                {
                    if (currentRecord != null)
                    {
                        string error = "Disabled";
                        reader.Close();
                        // currentRecord.Save(conn, out error);
                        DoQuery();
                        lblError.Text = "Saved: " + error;
                    }

                }
        */

        private void btnWatch_Click(object sender, EventArgs e)
        {
            if (currentRecord == null)
                return;

            string Info = ObjectInfo(currentRecord);
            FBCollection children = currentRecord as FBCollection;
            if (children != null)
            {
                foreach (FBObject item in children.items)
                {
                    Info += "\nChild: " + FBInfo(item);
                }
            }
            MessageBox.Show(Info);

        }

        private string ObjectInfo(object obj)
        {
            FieldInfo[] myFieldInfo;
            //Type myType = typeof(currentRecord);
            Type myType = obj.GetType();
            myFieldInfo = myType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance
                | BindingFlags.Public);

            string Info = "";
            Info = "The fields of " + "FieldInfoClass are \n";

            for (int i = 0; i < myFieldInfo.Length; i++)
            {
                Object valueObj = myFieldInfo[i].GetValue(obj);
                string value;
                if (valueObj != null)
                    value = valueObj.ToString();
                else
                    value = "";

                if (value != "")
                {
                    Info += " Name            : " + myFieldInfo[i].Name;
                    /*
                            Info += "Declaring Type  : " + myFieldInfo[i].DeclaringType;
                            Info += "IsPublic        : " + myFieldInfo[i].IsPublic;
                            Info += "MemberType      : " + myFieldInfo[i].MemberType;
                            Info += "FieldType       : " + myFieldInfo[i].FieldType;
                            Info += "IsFamily        : " + myFieldInfo[i].IsFamily;
                    */
                    Info += " Value            : " + value; // myFieldInfo[i].GetValue(obj);
                }
            }
            return Info;
        }

        private string FBInfo(FBObject obj)
        {
            string Info = " - Name: " + obj.Name;
            Info += " - SNID: " + obj.SNID;
            return Info;
        }

        /// <summary>
        /// User interface for opening an existing database
        /// </summary>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Title = "Select the name of the existing database";
            openFileDialog1.Filter = "Database files (*.db)|*.db|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            DialogResult res = this.openFileDialog1.ShowDialog();
            if (res == DialogResult.Cancel)
            {
                return;
            }
            lblDBName.Text = openFileDialog1.FileName;
        }

        /// <summary>
        /// Establish DB connection
        /// </summary>
        private void btnOpenDB_Click(object sender, EventArgs e)
        {
            try
            {
                conn = new SQLiteConnection("Data Source=" + lblDBName.Text + ";Password=" + txtPwd.Text);
                conn.Open();
                this.richTextBox1.Text = "Database opened successfully";
            }
            catch (Exception ex)
            {
                this.richTextBox1.Text = ex.ToString();
            }
        }

    }
}
