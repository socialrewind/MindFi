namespace DBUtil
{
    partial class DBUtil
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblDBName = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.lblPwd = new System.Windows.Forms.Label();
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.btnOpenDB = new System.Windows.Forms.Button();
            this.txtSQL = new System.Windows.Forms.TextBox();
            this.btnDoQuery = new System.Windows.Forms.Button();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblDBName
            // 
            this.lblDBName.Location = new System.Drawing.Point(16, 16);
            this.lblDBName.Name = "lblDBName";
            this.lblDBName.Size = new System.Drawing.Size(450, 16);
            this.lblDBName.TabIndex = 1;
            this.lblDBName.Text = "Select a database...";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(480, 16);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(288, 24);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblPwd
            // 
            this.lblPwd.Location = new System.Drawing.Point(16, 64);
            this.lblPwd.Name = "lblPwd";
            this.lblPwd.Size = new System.Drawing.Size(64, 16);
            this.lblPwd.TabIndex = 1;
            this.lblPwd.Text = "Password:";
            // 
            // txtPwd
            // 
            this.txtPwd.Location = new System.Drawing.Point(96, 64);
            this.txtPwd.Name = "txtPwd";
            this.txtPwd.PasswordChar = '*';
            this.txtPwd.Size = new System.Drawing.Size(207, 20);
            this.txtPwd.TabIndex = 2;
            // 
            // btnOpenDB
            // 
            this.btnOpenDB.Location = new System.Drawing.Point(320, 64);
            this.btnOpenDB.Name = "btnOpenDB";
            this.btnOpenDB.Size = new System.Drawing.Size(288, 24);
            this.btnOpenDB.TabIndex = 3;
            this.btnOpenDB.Text = "Open Database";
            this.btnOpenDB.UseVisualStyleBackColor = true;
            this.btnOpenDB.Click += new System.EventHandler(this.btnOpenDB_Click);
            // 
            // txtSQL
            // 
            this.txtSQL.Location = new System.Drawing.Point(96, 112);
            this.txtSQL.Multiline = true;
            this.txtSQL.Name = "txtSQL";
            this.txtSQL.Size = new System.Drawing.Size(600, 64);
            this.txtSQL.TabIndex = 4;
            // 
            // btnDoQuery
            // 
            this.btnDoQuery.Location = new System.Drawing.Point(96, 196);
            this.btnDoQuery.Name = "btnDoQuery";
            this.btnDoQuery.Size = new System.Drawing.Size(288, 24);
            this.btnDoQuery.TabIndex = 5;
            this.btnDoQuery.Text = "Query";
            this.btnDoQuery.UseVisualStyleBackColor = true;
            this.btnDoQuery.Click += new System.EventHandler(this.btnDoQuery_Click);
            // 
            // txtResults
            // 
            this.txtResults.Location = new System.Drawing.Point(96, 228);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.Size = new System.Drawing.Size(600, 300);
            this.txtResults.TabIndex = 6;
            // 
            // DBUtil
            // 
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.lblDBName);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblPwd);
            this.Controls.Add(this.txtPwd);
            this.Controls.Add(this.btnOpenDB);
            this.Controls.Add(this.txtSQL);
            this.Controls.Add(this.btnDoQuery);
            this.Controls.Add(this.txtResults);
            this.Name = "DBUtil";
            this.Text = "DB Util";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblDBName;
	private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblPwd;
        private System.Windows.Forms.TextBox txtPwd;
        private System.Windows.Forms.Button btnOpenDB;
        private System.Windows.Forms.TextBox txtSQL;
        private System.Windows.Forms.Button btnDoQuery;
        private System.Windows.Forms.TextBox txtResults;
                
        #endregion
    }
}
