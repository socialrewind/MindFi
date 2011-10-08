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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDashboard));
            this.labelWelcome = new System.Windows.Forms.Label();
            this.btnOnline = new System.Windows.Forms.Button();
            this.labelStats = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.processTimer = new System.Windows.Forms.Timer(this.components);
            this.btnFBAcccount = new System.Windows.Forms.Button();
            this.btnLinkedInAccount = new System.Windows.Forms.Button();
            this.btnTwitterAccount = new System.Windows.Forms.Button();
            this.radioButtonFB1 = new System.Windows.Forms.RadioButton();
            this.radioButtonFB2 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkLabelFB2 = new System.Windows.Forms.LinkLabel();
            this.linkLabelFB1 = new System.Windows.Forms.LinkLabel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.linkLabelTw2 = new System.Windows.Forms.LinkLabel();
            this.linkLabelTw1 = new System.Windows.Forms.LinkLabel();
            this.radioButtonTw1 = new System.Windows.Forms.RadioButton();
            this.radioButtonTw2 = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.linkLabelLI2 = new System.Windows.Forms.LinkLabel();
            this.linkLabelLI1 = new System.Windows.Forms.LinkLabel();
            this.radioButtonLI1 = new System.Windows.Forms.RadioButton();
            this.radioButtonLI2 = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelWelcome
            // 
            this.labelWelcome.AutoSize = true;
            this.labelWelcome.Location = new System.Drawing.Point(16, 16);
            this.labelWelcome.Name = "labelWelcome";
            this.labelWelcome.Size = new System.Drawing.Size(70, 13);
            this.labelWelcome.TabIndex = 1;
            this.labelWelcome.Text = "Not logged in";
            // 
            // btnOnline
            // 
            this.btnOnline.Enabled = false;
            this.btnOnline.Location = new System.Drawing.Point(211, 266);
            this.btnOnline.Name = "btnOnline";
            this.btnOnline.Size = new System.Drawing.Size(189, 37);
            this.btnOnline.TabIndex = 5;
            this.btnOnline.Text = "Go Online/Login";
            this.btnOnline.UseVisualStyleBackColor = true;
            this.btnOnline.Click += new System.EventHandler(this.btnOnline_Click);
            // 
            // labelStats
            // 
            this.labelStats.AutoSize = true;
            this.labelStats.Location = new System.Drawing.Point(16, 320);
            this.labelStats.Name = "labelStats";
            this.labelStats.Size = new System.Drawing.Size(90, 13);
            this.labelStats.TabIndex = 1;
            this.labelStats.Text = "Statistics pending";
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(16, 352);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(25, 13);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "Info";
            // 
            // processTimer
            // 
            this.processTimer.Tick += new System.EventHandler(this.processTimer_Tick);
            // 
            // btnFBAcccount
            // 
            this.btnFBAcccount.Image = ((System.Drawing.Image)(resources.GetObject("btnFBAcccount.Image")));
            this.btnFBAcccount.Location = new System.Drawing.Point(16, 50);
            this.btnFBAcccount.Name = "btnFBAcccount";
            this.btnFBAcccount.Size = new System.Drawing.Size(70, 70);
            this.btnFBAcccount.TabIndex = 7;
            this.btnFBAcccount.UseVisualStyleBackColor = true;
            this.btnFBAcccount.Click += new System.EventHandler(this.btnFBAcccount_Click);
            // 
            // btnLinkedInAccount
            // 
            this.btnLinkedInAccount.Image = ((System.Drawing.Image)(resources.GetObject("btnLinkedInAccount.Image")));
            this.btnLinkedInAccount.Location = new System.Drawing.Point(441, 50);
            this.btnLinkedInAccount.Name = "btnLinkedInAccount";
            this.btnLinkedInAccount.Size = new System.Drawing.Size(70, 70);
            this.btnLinkedInAccount.TabIndex = 8;
            this.btnLinkedInAccount.UseVisualStyleBackColor = true;
            this.btnLinkedInAccount.Click += new System.EventHandler(this.btnLinkedInAccount_Click_1);
            // 
            // btnTwitterAccount
            // 
            this.btnTwitterAccount.Image = ((System.Drawing.Image)(resources.GetObject("btnTwitterAccount.Image")));
            this.btnTwitterAccount.Location = new System.Drawing.Point(244, 50);
            this.btnTwitterAccount.Name = "btnTwitterAccount";
            this.btnTwitterAccount.Size = new System.Drawing.Size(70, 70);
            this.btnTwitterAccount.TabIndex = 8;
            this.btnTwitterAccount.UseVisualStyleBackColor = true;
            this.btnTwitterAccount.Click += new System.EventHandler(this.btnTwitterAccount_Click_1);
            // 
            // radioButtonFB1
            // 
            this.radioButtonFB1.AutoSize = true;
            this.radioButtonFB1.Checked = true;
            this.radioButtonFB1.Location = new System.Drawing.Point(0, 28);
            this.radioButtonFB1.Name = "radioButtonFB1";
            this.radioButtonFB1.Size = new System.Drawing.Size(14, 13);
            this.radioButtonFB1.TabIndex = 9;
            this.radioButtonFB1.TabStop = true;
            this.radioButtonFB1.UseVisualStyleBackColor = true;
            // 
            // radioButtonFB2
            // 
            this.radioButtonFB2.AutoSize = true;
            this.radioButtonFB2.Location = new System.Drawing.Point(0, 64);
            this.radioButtonFB2.Name = "radioButtonFB2";
            this.radioButtonFB2.Size = new System.Drawing.Size(14, 13);
            this.radioButtonFB2.TabIndex = 10;
            this.radioButtonFB2.UseVisualStyleBackColor = true;
            this.radioButtonFB2.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.linkLabelFB2);
            this.groupBox1.Controls.Add(this.linkLabelFB1);
            this.groupBox1.Controls.Add(this.radioButtonFB1);
            this.groupBox1.Controls.Add(this.radioButtonFB2);
            this.groupBox1.Location = new System.Drawing.Point(16, 136);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(202, 124);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Facebook accounts";
            // 
            // linkLabelFB2
            // 
            this.linkLabelFB2.AutoSize = true;
            this.linkLabelFB2.Location = new System.Drawing.Point(19, 64);
            this.linkLabelFB2.Name = "linkLabelFB2";
            this.linkLabelFB2.Size = new System.Drawing.Size(68, 13);
            this.linkLabelFB2.TabIndex = 11;
            this.linkLabelFB2.TabStop = true;
            this.linkLabelFB2.Text = "linkLabelFB2";
            this.linkLabelFB2.Visible = false;
            // 
            // linkLabelFB1
            // 
            this.linkLabelFB1.AutoSize = true;
            this.linkLabelFB1.Location = new System.Drawing.Point(19, 30);
            this.linkLabelFB1.Name = "linkLabelFB1";
            this.linkLabelFB1.Size = new System.Drawing.Size(33, 13);
            this.linkLabelFB1.TabIndex = 11;
            this.linkLabelFB1.TabStop = true;
            this.linkLabelFB1.Text = "None";
            this.linkLabelFB1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelFB1_LinkClicked);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.linkLabelTw2);
            this.groupBox2.Controls.Add(this.linkLabelTw1);
            this.groupBox2.Controls.Add(this.radioButtonTw1);
            this.groupBox2.Controls.Add(this.radioButtonTw2);
            this.groupBox2.Location = new System.Drawing.Point(244, 136);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(171, 124);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Twitter accounts";
            // 
            // linkLabelTw2
            // 
            this.linkLabelTw2.AutoSize = true;
            this.linkLabelTw2.Location = new System.Drawing.Point(17, 64);
            this.linkLabelTw2.Name = "linkLabelTw2";
            this.linkLabelTw2.Size = new System.Drawing.Size(70, 13);
            this.linkLabelTw2.TabIndex = 11;
            this.linkLabelTw2.TabStop = true;
            this.linkLabelTw2.Text = "linkLabelTw2";
            this.linkLabelTw2.Visible = false;
            // 
            // linkLabelTw1
            // 
            this.linkLabelTw1.AutoSize = true;
            this.linkLabelTw1.Location = new System.Drawing.Point(17, 30);
            this.linkLabelTw1.Name = "linkLabelTw1";
            this.linkLabelTw1.Size = new System.Drawing.Size(33, 13);
            this.linkLabelTw1.TabIndex = 11;
            this.linkLabelTw1.TabStop = true;
            this.linkLabelTw1.Text = "None";
            this.linkLabelTw1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelTw1_LinkClicked);
            // 
            // radioButtonTw1
            // 
            this.radioButtonTw1.AutoSize = true;
            this.radioButtonTw1.Checked = true;
            this.radioButtonTw1.Location = new System.Drawing.Point(0, 28);
            this.radioButtonTw1.Name = "radioButtonTw1";
            this.radioButtonTw1.Size = new System.Drawing.Size(14, 13);
            this.radioButtonTw1.TabIndex = 9;
            this.radioButtonTw1.TabStop = true;
            this.radioButtonTw1.UseVisualStyleBackColor = true;
            // 
            // radioButtonTw2
            // 
            this.radioButtonTw2.AutoSize = true;
            this.radioButtonTw2.Location = new System.Drawing.Point(0, 64);
            this.radioButtonTw2.Name = "radioButtonTw2";
            this.radioButtonTw2.Size = new System.Drawing.Size(14, 13);
            this.radioButtonTw2.TabIndex = 10;
            this.radioButtonTw2.UseVisualStyleBackColor = true;
            this.radioButtonTw2.Visible = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.linkLabelLI2);
            this.groupBox3.Controls.Add(this.linkLabelLI1);
            this.groupBox3.Controls.Add(this.radioButtonLI1);
            this.groupBox3.Controls.Add(this.radioButtonLI2);
            this.groupBox3.Location = new System.Drawing.Point(434, 136);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(181, 124);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "LinkedIn accounts";
            // 
            // linkLabelLI2
            // 
            this.linkLabelLI2.AutoSize = true;
            this.linkLabelLI2.Location = new System.Drawing.Point(19, 64);
            this.linkLabelLI2.Name = "linkLabelLI2";
            this.linkLabelLI2.Size = new System.Drawing.Size(64, 13);
            this.linkLabelLI2.TabIndex = 11;
            this.linkLabelLI2.TabStop = true;
            this.linkLabelLI2.Text = "linkLabelLI2";
            this.linkLabelLI2.Visible = false;
            // 
            // linkLabelLI1
            // 
            this.linkLabelLI1.AutoSize = true;
            this.linkLabelLI1.Location = new System.Drawing.Point(19, 30);
            this.linkLabelLI1.Name = "linkLabelLI1";
            this.linkLabelLI1.Size = new System.Drawing.Size(33, 13);
            this.linkLabelLI1.TabIndex = 11;
            this.linkLabelLI1.TabStop = true;
            this.linkLabelLI1.Text = "None";
            this.linkLabelLI1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLI1_LinkClicked);
            // 
            // radioButtonLI1
            // 
            this.radioButtonLI1.AutoSize = true;
            this.radioButtonLI1.Checked = true;
            this.radioButtonLI1.Location = new System.Drawing.Point(0, 28);
            this.radioButtonLI1.Name = "radioButtonLI1";
            this.radioButtonLI1.Size = new System.Drawing.Size(14, 13);
            this.radioButtonLI1.TabIndex = 9;
            this.radioButtonLI1.TabStop = true;
            this.radioButtonLI1.UseVisualStyleBackColor = true;
            // 
            // radioButtonLI2
            // 
            this.radioButtonLI2.AutoSize = true;
            this.radioButtonLI2.Location = new System.Drawing.Point(0, 64);
            this.radioButtonLI2.Name = "radioButtonLI2";
            this.radioButtonLI2.Size = new System.Drawing.Size(14, 13);
            this.radioButtonLI2.TabIndex = 10;
            this.radioButtonLI2.UseVisualStyleBackColor = true;
            this.radioButtonLI2.Visible = false;
            // 
            // frmDashboard
            // 
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnTwitterAccount);
            this.Controls.Add(this.btnLinkedInAccount);
            this.Controls.Add(this.btnFBAcccount);
            this.Controls.Add(this.labelWelcome);
            this.Controls.Add(this.btnOnline);
            this.Controls.Add(this.labelStats);
            this.Controls.Add(this.labelInfo);
            this.Name = "frmDashboard";
            this.Text = "My Backup Dashboard";
            this.Load += new System.EventHandler(this.frmDashboard_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label labelWelcome;
        private System.Windows.Forms.Button btnOnline;
        private System.Windows.Forms.Label labelStats;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Timer processTimer;

        private System.Windows.Forms.Button btnFBAcccount;
        private System.Windows.Forms.Button btnLinkedInAccount;
        private System.Windows.Forms.Button btnTwitterAccount;
        private System.Windows.Forms.RadioButton radioButtonFB1;
        private System.Windows.Forms.RadioButton radioButtonFB2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonTw1;
        private System.Windows.Forms.RadioButton radioButtonTw2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButtonLI1;
        private System.Windows.Forms.RadioButton radioButtonLI2;
        private System.Windows.Forms.LinkLabel linkLabelFB1;
        #endregion
        private System.Windows.Forms.LinkLabel linkLabelFB2;
        private System.Windows.Forms.LinkLabel linkLabelTw2;
        private System.Windows.Forms.LinkLabel linkLabelTw1;
        private System.Windows.Forms.LinkLabel linkLabelLI2;
        private System.Windows.Forms.LinkLabel linkLabelLI1;

    }
}