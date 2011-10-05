namespace MyBackup
{
    partial class frmDashboard
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
            this.components = new System.ComponentModel.Container();
            this.ClientSize = new System.Drawing.Size(640, 480);

            this.labelWelcome = new System.Windows.Forms.Label();
	    this.btnLoginFB = new System.Windows.Forms.Button();
	    this.btnRefresh = new System.Windows.Forms.Button();
            this.accountsGrid = new System.Windows.Forms.DataGrid();
	    this.btnAddAccount = new System.Windows.Forms.Button();
	    this.btnDeleteAccount = new System.Windows.Forms.Button();
            this.labelStats = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
	    this.processTimer = new System.Windows.Forms.Timer(this.components);
                              
            this.SuspendLayout();

            // 
            // labelWelcome
            // 
            this.labelWelcome.AutoSize = true;
            this.labelWelcome.Location = new System.Drawing.Point(16, 16);
            this.labelWelcome.Name = "labelWelcome";
            this.labelWelcome.Size = new System.Drawing.Size(96, 16);
            this.labelWelcome.TabIndex = 1;
            this.labelWelcome.Text = "Not logged in";
            // 
            // btnLoginFB
            // 
            this.btnLoginFB.Enabled = true;
            this.btnLoginFB.Location = new System.Drawing.Point(16, 288);
            this.btnLoginFB.Name = "btnLoginFB";
            this.btnLoginFB.Size = new System.Drawing.Size(96, 24);
            this.btnLoginFB.TabIndex = 5;
            this.btnLoginFB.Text = "Facebook Login";
            this.btnLoginFB.UseVisualStyleBackColor = true;
            this.btnLoginFB.Click += new System.EventHandler(this.btnLoginFB_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Enabled = true;
            this.btnRefresh.Location = new System.Drawing.Point(300, 288);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(96, 24);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // accountsGrid
            // 
	    this.accountsGrid.Location = new System.Drawing.Point(16, 48);
	    this.accountsGrid.Size = new System.Drawing.Size(600, 200);
	    this.accountsGrid.CaptionText = "Social Network Accounts";
            // 
            // btnAddAccount
            // 
            this.btnAddAccount.Enabled = true;
            this.btnAddAccount.Location = new System.Drawing.Point(16, 256);
            this.btnAddAccount.Name = "btnAddAccount";
            this.btnAddAccount.Size = new System.Drawing.Size(256, 24);
            this.btnAddAccount.TabIndex = 5;
            this.btnAddAccount.Text = "Add Social Network Account";
            this.btnAddAccount.UseVisualStyleBackColor = true;
            this.btnAddAccount.Click += new System.EventHandler(this.btnAddAccount_Click);
            // 
            // btnDeleteAccount
            // 
            this.btnDeleteAccount.Enabled = true;
            this.btnDeleteAccount.Location = new System.Drawing.Point(300, 256);
            this.btnDeleteAccount.Name = "btnDeleteAccount";
            this.btnDeleteAccount.Size = new System.Drawing.Size(256, 24);
            this.btnDeleteAccount.TabIndex = 5;
            this.btnDeleteAccount.Text = "Delete selected Accounts";
            this.btnDeleteAccount.UseVisualStyleBackColor = true;
            this.btnDeleteAccount.Click += new System.EventHandler(this.btnDeleteAccount_Click);
            // 
            // labelStats
            // 
            this.labelStats.AutoSize = true;
            this.labelStats.Location = new System.Drawing.Point(16, 320);
            this.labelStats.Name = "labelStats";
            this.labelStats.Size = new System.Drawing.Size(300, 200);
            this.labelStats.TabIndex = 1;
            this.labelStats.Text = "Statistics pending";
	    // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(16, 400);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(300, 200);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "Info";

            // 
            // processTimer
            // 
            this.processTimer.Tick += new System.EventHandler(this.processTimer_Tick);

	    // frmDashboard
            this.Controls.Add(this.labelWelcome);
            this.Controls.Add(this.btnLoginFB);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.accountsGrid);
            this.Controls.Add(this.btnAddAccount);
            this.Controls.Add(this.btnDeleteAccount);
            this.Controls.Add(this.labelStats);
            this.Controls.Add(this.labelInfo);
            this.Name = "frmDashboard";
            this.Text = "My Backup Dashboard";
            this.Load += new System.EventHandler(this.frmDashboard_Load);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label labelWelcome;
        private System.Windows.Forms.Button btnLoginFB;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.DataGrid accountsGrid;
        private System.Windows.Forms.Button btnAddAccount;
        private System.Windows.Forms.Button btnDeleteAccount;
        private System.Windows.Forms.Label labelStats;
	private System.Windows.Forms.Label labelInfo;
	private System.Windows.Forms.Timer processTimer;
                
        #endregion

    }
}