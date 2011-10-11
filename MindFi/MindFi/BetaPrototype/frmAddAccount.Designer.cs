namespace MyBackup
{
    partial class frmAddAccount
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
            this.labelSN = new System.Windows.Forms.Label();
            this.labelID = new System.Windows.Forms.Label();
            this.labelSNID = new System.Windows.Forms.Label();
            this.labelAlias = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoginSN = new System.Windows.Forms.Button();
            this.labelURL = new System.Windows.Forms.Label();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.txtAlias = new System.Windows.Forms.TextBox();
            this.cmbSocialNetworks = new System.Windows.Forms.ComboBox();
            this.step2Timer = new System.Windows.Forms.Timer(this.components);
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblProfiles = new System.Windows.Forms.Label();
            this.lblFrequency = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.cmbProfiles = new System.Windows.Forms.ComboBox();
            this.cmbFrequency = new System.Windows.Forms.ComboBox();
            this.cmbFrequencyValue = new System.Windows.Forms.ComboBox();
            this.dateTimeBackupRangeStart = new System.Windows.Forms.DateTimePicker();
            this.lblBackupStartTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelSN
            // 
            this.labelSN.AutoSize = true;
            this.labelSN.Location = new System.Drawing.Point(13, 16);
            this.labelSN.Name = "labelSN";
            this.labelSN.Size = new System.Drawing.Size(82, 13);
            this.labelSN.TabIndex = 2;
            this.labelSN.Text = "Social Network:";
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Location = new System.Drawing.Point(13, 164);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(21, 13);
            this.labelID.TabIndex = 2;
            this.labelID.Text = "ID:";
            // 
            // labelSNID
            // 
            this.labelSNID.AutoSize = true;
            this.labelSNID.Location = new System.Drawing.Point(104, 164);
            this.labelSNID.Name = "labelSNID";
            this.labelSNID.Size = new System.Drawing.Size(10, 13);
            this.labelSNID.TabIndex = 2;
            this.labelSNID.Text = "-";
            // 
            // labelAlias
            // 
            this.labelAlias.AutoSize = true;
            this.labelAlias.Location = new System.Drawing.Point(13, 90);
            this.labelAlias.Name = "labelAlias";
            this.labelAlias.Size = new System.Drawing.Size(32, 13);
            this.labelAlias.TabIndex = 3;
            this.labelAlias.Text = "Alias:";
            // 
            // btnSave
            // 
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(48, 345);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnLoginSN
            // 
            this.btnLoginSN.Enabled = false;
            this.btnLoginSN.Location = new System.Drawing.Point(48, 48);
            this.btnLoginSN.Name = "btnLoginSN";
            this.btnLoginSN.Size = new System.Drawing.Size(150, 23);
            this.btnLoginSN.TabIndex = 2;
            this.btnLoginSN.Text = "Login";
            this.btnLoginSN.UseVisualStyleBackColor = true;
            this.btnLoginSN.Click += new System.EventHandler(this.btnLoginSN_Click);
            // 
            // labelURL
            // 
            this.labelURL.AutoSize = true;
            this.labelURL.Location = new System.Drawing.Point(13, 128);
            this.labelURL.Name = "labelURL";
            this.labelURL.Size = new System.Drawing.Size(57, 13);
            this.labelURL.TabIndex = 5;
            this.labelURL.Text = "Your URL:";
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(104, 128);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(207, 20);
            this.txtURL.TabIndex = 4;
            this.txtURL.TextChanged += new System.EventHandler(this.txtURL_TextChanged);
            // 
            // txtAlias
            // 
            this.txtAlias.Location = new System.Drawing.Point(104, 90);
            this.txtAlias.Name = "txtAlias";
            this.txtAlias.Size = new System.Drawing.Size(207, 20);
            this.txtAlias.TabIndex = 4;
            this.txtAlias.TextChanged += new System.EventHandler(this.txtAlias_TextChanged);
            // 
            // cmbSocialNetworks
            // 
            this.cmbSocialNetworks.Enabled = false;
            this.cmbSocialNetworks.FormattingEnabled = true;
            this.cmbSocialNetworks.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.cmbSocialNetworks.Location = new System.Drawing.Point(104, 16);
            this.cmbSocialNetworks.Name = "cmbSocialNetworks";
            this.cmbSocialNetworks.Size = new System.Drawing.Size(207, 21);
            this.cmbSocialNetworks.TabIndex = 1;
            this.cmbSocialNetworks.SelectedIndexChanged += new System.EventHandler(this.cmbSocialNetworks_SelectedIndexChanged);
            this.cmbSocialNetworks.Click += new System.EventHandler(this.cmbSocialNetworks_Click);
            // 
            // step2Timer
            // 
            this.step2Timer.Tick += new System.EventHandler(this.step2Timer_Tick);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(13, 393);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(47, 13);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "lblStatus";
            this.lblStatus.Click += new System.EventHandler(this.lblStatus_Click);
            // 
            // lblProfiles
            // 
            this.lblProfiles.AutoSize = true;
            this.lblProfiles.Location = new System.Drawing.Point(13, 208);
            this.lblProfiles.Name = "lblProfiles";
            this.lblProfiles.Size = new System.Drawing.Size(79, 13);
            this.lblProfiles.TabIndex = 10;
            this.lblProfiles.Text = "Backup Profile:";
            // 
            // lblFrequency
            // 
            this.lblFrequency.AutoSize = true;
            this.lblFrequency.Location = new System.Drawing.Point(13, 240);
            this.lblFrequency.Name = "lblFrequency";
            this.lblFrequency.Size = new System.Drawing.Size(100, 13);
            this.lblFrequency.TabIndex = 10;
            this.lblFrequency.Text = "Backup Frequency:";
            // 
            // btnLogout
            // 
            this.btnLogout.Location = new System.Drawing.Point(230, 48);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(75, 23);
            this.btnLogout.TabIndex = 3;
            this.btnLogout.Text = "Logout";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // cmbProfiles
            // 
            this.cmbProfiles.FormattingEnabled = true;
            this.cmbProfiles.Location = new System.Drawing.Point(104, 208);
            this.cmbProfiles.Name = "cmbProfiles";
            this.cmbProfiles.Size = new System.Drawing.Size(207, 21);
            this.cmbProfiles.TabIndex = 6;
            // 
            // cmbFrequency
            // 
            this.cmbFrequency.FormattingEnabled = true;
            this.cmbFrequency.Location = new System.Drawing.Point(160, 240);
            this.cmbFrequency.Name = "cmbFrequency";
            this.cmbFrequency.Size = new System.Drawing.Size(151, 21);
            this.cmbFrequency.TabIndex = 8;
            // 
            // cmbFrequencyValue
            // 
            this.cmbFrequencyValue.FormattingEnabled = true;
            this.cmbFrequencyValue.Location = new System.Drawing.Point(120, 240);
            this.cmbFrequencyValue.Name = "cmbFrequencyValue";
            this.cmbFrequencyValue.Size = new System.Drawing.Size(32, 21);
            this.cmbFrequencyValue.TabIndex = 7;
            // 
            // dateTimeBackupRangeStart
            // 
            this.dateTimeBackupRangeStart.Location = new System.Drawing.Point(120, 279);
            this.dateTimeBackupRangeStart.Name = "dateTimeBackupRangeStart";
            this.dateTimeBackupRangeStart.Size = new System.Drawing.Size(200, 20);
            this.dateTimeBackupRangeStart.TabIndex = 11;
            // 
            // lblBackupStartTime
            // 
            this.lblBackupStartTime.AutoSize = true;
            this.lblBackupStartTime.Location = new System.Drawing.Point(13, 279);
            this.lblBackupStartTime.Name = "lblBackupStartTime";
            this.lblBackupStartTime.Size = new System.Drawing.Size(108, 13);
            this.lblBackupStartTime.TabIndex = 12;
            this.lblBackupStartTime.Text = "Backup data starting:";
            // 
            // frmAddAccount
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.lblBackupStartTime);
            this.Controls.Add(this.dateTimeBackupRangeStart);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblProfiles);
            this.Controls.Add(this.lblFrequency);
            this.Controls.Add(this.cmbSocialNetworks);
            this.Controls.Add(this.cmbProfiles);
            this.Controls.Add(this.cmbFrequency);
            this.Controls.Add(this.cmbFrequencyValue);
            this.Controls.Add(this.txtAlias);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.labelURL);
            this.Controls.Add(this.btnLoginSN);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.labelAlias);
            this.Controls.Add(this.labelSNID);
            this.Controls.Add(this.labelID);
            this.Controls.Add(this.labelSN);
            this.Name = "frmAddAccount";
            this.Text = "Social Network Account";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmAddAccount_Closing);
            this.Load += new System.EventHandler(this.frmAddAccount_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSN;
        private System.Windows.Forms.Label labelID;
        private System.Windows.Forms.Label labelSNID;
        private System.Windows.Forms.Label labelAlias;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoginSN;
        private System.Windows.Forms.Label labelURL;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.TextBox txtAlias;
        private System.Windows.Forms.ComboBox cmbSocialNetworks;
        private System.Windows.Forms.ComboBox cmbProfiles;
        private System.Windows.Forms.ComboBox cmbFrequency;
        private System.Windows.Forms.ComboBox cmbFrequencyValue;
        private System.Windows.Forms.Timer step2Timer;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblProfiles;
        private System.Windows.Forms.Label lblFrequency;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.DateTimePicker dateTimeBackupRangeStart;
        private System.Windows.Forms.Label lblBackupStartTime;
        
    }
}