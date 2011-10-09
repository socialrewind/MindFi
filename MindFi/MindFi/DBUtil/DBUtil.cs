using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data.SQLite;

namespace DBUtil
{
    /// <summary>
    /// Main form for the DB interface
    /// </summary>
    public partial class DBUtil : Form
    {
        private static volatile SQLiteConnection conn = null;

        /// <summary>
        /// Form constructor
        /// </summary>
        public DBUtil()
        {
            InitializeComponent();
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
                txtResults.Text = "Database opened successfully";
            }
            catch (Exception ex)
            {
                txtResults.Text = ex.ToString();
            }
        }

        /// <summary>
        /// Do Query
        /// </summary>
        private void btnDoQuery_Click(object sender, EventArgs e)
        {
            try
            {
                SQLiteCommand GeneralCmd = new SQLiteCommand(txtSQL.Text, conn);
                if (txtSQL.Text.ToLower().Contains("select"))
                {
                    SQLiteDataReader reader = GeneralCmd.ExecuteReader();
                    txtResults.Text = "Query executed successfully\n";
                    bool FirstRow = true;
                    while (reader.Read())
                    {
                        if (FirstRow)
                        {
                            FirstRow = false;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                txtResults.Text += "\t" + reader.GetName(i);
                            }
                            txtResults.Text += "\n";
                        }
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (!reader.IsDBNull(i))
                            {
                                txtResults.Text += "\t" + reader[i];
                            }
                            else
                            {
                                txtResults.Text += "\t---";
                            }
                        }
                        txtResults.Text += "\n";
                    }
                    reader.Close();
                }
                else
                {
                    int outrows = GeneralCmd.ExecuteNonQuery();
                    txtResults.Text = "Command successfully executed\n" + outrows + " affected\n";
                }

            }
            catch (Exception ex)
            {
                txtResults.Text = ex.ToString();
            }
        }
    }

}
