namespace MyBackup
{
    partial class frmMain
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

            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnLoginSN = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.txtAlias = new System.Windows.Forms.TextBox();
            this.cmbSocialNetworks = new System.Windows.Forms.ComboBox();
            this.lblName = new System.Windows.Forms.Label();
            this.step2Timer = new System.Windows.Forms.Timer(this.components);
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();

            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Social Network:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Alias:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Enabled = true;
            this.btnBrowse.Location = new System.Drawing.Point(230, 30);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnOK
            // 
            this.btnOK.Enabled = false;
            this.btnOK.Location = new System.Drawing.Point(139, 212);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            // 
            // btnLoginSN
            // 
            this.btnLoginSN.Enabled = false;
            this.btnLoginSN.Location = new System.Drawing.Point(48, 91);
            this.btnLoginSN.Name = "btnLoginSN";
            this.btnLoginSN.Size = new System.Drawing.Size(263, 23);
            this.btnLoginSN.TabIndex = 2;
            this.btnLoginSN.Text = "Login into Social Network";
            this.btnLoginSN.UseVisualStyleBackColor = true;
            this.btnLoginSN.Click += new System.EventHandler(this.btnLoginSN_Click);

            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 171);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Your URL:";
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(104, 168);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(207, 20);
            this.txtURL.TabIndex = 4;
            this.txtURL.TextChanged += new System.EventHandler(this.txtURL_TextChanged);

            // 
            // txtAlias
            // 
            this.txtAlias.Location = new System.Drawing.Point(104, 130);
            this.txtAlias.Name = "txtAlias";
            this.txtAlias.Size = new System.Drawing.Size(207, 20);
            this.txtAlias.TabIndex = 3;
            this.txtAlias.TextChanged += new System.EventHandler(this.txtAlias_TextChanged);

            // 
            // cmbSocialNetworks
            // 
            this.cmbSocialNetworks.FormattingEnabled = true;
            this.cmbSocialNetworks.Location = new System.Drawing.Point(104, 64);
            this.cmbSocialNetworks.Name = "cmbSocialNetworks";
            this.cmbSocialNetworks.Size = new System.Drawing.Size(207, 21);
            this.cmbSocialNetworks.TabIndex = 1;
            this.cmbSocialNetworks.SelectedIndexChanged += new System.EventHandler(this.cmbSocialNetworks_SelectedIndexChanged);
            this.cmbSocialNetworks.Click += new System.EventHandler(this.cmbSocialNetworks_Click);

            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(104, 12);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(45, 13);
            this.lblName.TabIndex = 9;
            this.lblName.Text = "";
            // 
            // step2Timer
            // 
            this.step2Timer.Tick += new System.EventHandler(this.step2Timer_Tick);

            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(13, 256);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(47, 13);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "lblStatus";
            // 
            // btnLogout
            // 
            this.btnLogout.Location = new System.Drawing.Point(139, 299);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(75, 23);
            this.btnLogout.TabIndex = 11;
            this.btnLogout.Text = "Force logout";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

	    // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "20110712.db";

            // 
            // frmMain
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.cmbSocialNetworks);
            this.Controls.Add(this.txtAlias);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnLoginSN);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "frmMain";
            this.Text = "Step 1: Associating a Social Network Account";
            this.Load += new System.EventHandler(this.frmMain_Load);

            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnLoginSN;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.TextBox txtAlias;
        private System.Windows.Forms.ComboBox cmbSocialNetworks;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Timer step2Timer;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnLogout;
	private System.Windows.Forms.SaveFileDialog openFileDialog1;
        
    }
}