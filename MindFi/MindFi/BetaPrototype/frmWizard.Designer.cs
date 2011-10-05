namespace MyBackup
{
    partial class frmWizard
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
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnTryAgain = new System.Windows.Forms.Button();
            this.lblInformation = new System.Windows.Forms.Label();
            this.progressCurrent = new System.Windows.Forms.ProgressBar();
            this.lblCurrentAction = new System.Windows.Forms.Label();
            this.progressGlobal = new System.Windows.Forms.ProgressBar();
            this.lblGlobalProgress = new System.Windows.Forms.Label();
            this.chkDetails = new System.Windows.Forms.CheckBox();
            this.richTextDetails = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lblPendingRequests = new System.Windows.Forms.Label();
            this.lblPlannedRequests = new System.Windows.Forms.Label();
            this.lblTotalRequests = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblProcessedRequests = new System.Windows.Forms.Label();
            this.richTextErrors = new System.Windows.Forms.RichTextBox();
            this.btnFinish = new System.Windows.Forms.Button();
            this.lblTime = new System.Windows.Forms.Label();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnPrevious
            // 
            this.btnPrevious.Location = new System.Drawing.Point(123, 448);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(75, 23);
            this.btnPrevious.TabIndex = 0;
            this.btnPrevious.Text = "Previous";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(580, 448);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 0;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnTryAgain
            // 
            this.btnTryAgain.Location = new System.Drawing.Point(420, 448);
            this.btnTryAgain.Name = "btnTryAgain";
            this.btnTryAgain.Size = new System.Drawing.Size(125, 23);
            this.btnTryAgain.TabIndex = 0;
            this.btnTryAgain.Text = "Try Again / Continue";
            this.btnTryAgain.UseVisualStyleBackColor = true;
            this.btnTryAgain.Visible = false;
            this.btnTryAgain.Click += new System.EventHandler(this.btnTryAgain_Click);
            // 
            // lblInformation
            // 
            this.lblInformation.AutoSize = true;
            this.lblInformation.Location = new System.Drawing.Point(123, 29);
            this.lblInformation.Name = "lblInformation";
            this.lblInformation.Size = new System.Drawing.Size(59, 13);
            this.lblInformation.TabIndex = 1;
            this.lblInformation.Text = "Information";
            // 
            // progressCurrent
            // 
            this.progressCurrent.Location = new System.Drawing.Point(126, 168);
            this.progressCurrent.Name = "progressCurrent";
            this.progressCurrent.Size = new System.Drawing.Size(552, 23);
            this.progressCurrent.TabIndex = 2;
            // 
            // lblCurrentAction
            // 
            this.lblCurrentAction.AutoSize = true;
            this.lblCurrentAction.Location = new System.Drawing.Point(123, 123);
            this.lblCurrentAction.Name = "lblCurrentAction";
            this.lblCurrentAction.Size = new System.Drawing.Size(74, 13);
            this.lblCurrentAction.TabIndex = 3;
            this.lblCurrentAction.Text = "Current Action";
            // 
            // progressGlobal
            // 
            this.progressGlobal.Location = new System.Drawing.Point(126, 401);
            this.progressGlobal.Name = "progressGlobal";
            this.progressGlobal.Size = new System.Drawing.Size(552, 23);
            this.progressGlobal.TabIndex = 2;
            // 
            // lblGlobalProgress
            // 
            this.lblGlobalProgress.AutoSize = true;
            this.lblGlobalProgress.Location = new System.Drawing.Point(126, 382);
            this.lblGlobalProgress.Name = "lblGlobalProgress";
            this.lblGlobalProgress.Size = new System.Drawing.Size(81, 13);
            this.lblGlobalProgress.TabIndex = 5;
            this.lblGlobalProgress.Text = "Global Progress";
            // 
            // chkDetails
            // 
            this.chkDetails.AutoSize = true;
            this.chkDetails.Location = new System.Drawing.Point(592, 123);
            this.chkDetails.Name = "chkDetails";
            this.chkDetails.Size = new System.Drawing.Size(86, 17);
            this.chkDetails.TabIndex = 6;
            this.chkDetails.Text = "Show details";
            this.chkDetails.UseVisualStyleBackColor = true;
            this.chkDetails.Visible = false;
            this.chkDetails.CheckedChanged += new System.EventHandler(this.chkDetails_CheckedChanged);
            // 
            // richTextDetails
            // 
            this.richTextDetails.Location = new System.Drawing.Point(129, 240);
            this.richTextDetails.Name = "richTextDetails";
            this.richTextDetails.Size = new System.Drawing.Size(549, 140);
            this.richTextDetails.TabIndex = 7;
            this.richTextDetails.Text = "";
            this.richTextDetails.Visible = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lblPendingRequests
            // 
            this.lblPendingRequests.AutoSize = true;
            this.lblPendingRequests.Location = new System.Drawing.Point(123, 152);
            this.lblPendingRequests.Name = "lblPendingRequests";
            this.lblPendingRequests.Size = new System.Drawing.Size(89, 13);
            this.lblPendingRequests.TabIndex = 8;
            this.lblPendingRequests.Text = "Pending requests";
            this.lblPendingRequests.Visible = false;
            // 
            // lblPlannedRequests
            // 
            this.lblPlannedRequests.AutoSize = true;
            this.lblPlannedRequests.Location = new System.Drawing.Point(252, 152);
            this.lblPlannedRequests.Name = "lblPlannedRequests";
            this.lblPlannedRequests.Size = new System.Drawing.Size(92, 13);
            this.lblPlannedRequests.TabIndex = 9;
            this.lblPlannedRequests.Text = "Planned requests:";
            this.lblPlannedRequests.Visible = false;
            // 
            // lblTotalRequests
            // 
            this.lblTotalRequests.AutoSize = true;
            this.lblTotalRequests.Location = new System.Drawing.Point(540, 152);
            this.lblTotalRequests.Name = "lblTotalRequests";
            this.lblTotalRequests.Size = new System.Drawing.Size(77, 13);
            this.lblTotalRequests.TabIndex = 9;
            this.lblTotalRequests.Text = "Total requests:";
            this.lblTotalRequests.Visible = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(269, 448);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(102, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Pause / Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblProcessedRequests
            // 
            this.lblProcessedRequests.AutoSize = true;
            this.lblProcessedRequests.Location = new System.Drawing.Point(374, 152);
            this.lblProcessedRequests.Name = "lblProcessedRequests";
            this.lblProcessedRequests.Size = new System.Drawing.Size(103, 13);
            this.lblProcessedRequests.TabIndex = 9;
            this.lblProcessedRequests.Text = "Processed requests:";
            this.lblProcessedRequests.Visible = false;
            // 
            // richTextErrors
            // 
            this.richTextErrors.ForeColor = System.Drawing.Color.Red;
            this.richTextErrors.Location = new System.Drawing.Point(129, 197);
            this.richTextErrors.Name = "richTextErrors";
            this.richTextErrors.Size = new System.Drawing.Size(549, 37);
            this.richTextErrors.TabIndex = 11;
            this.richTextErrors.Text = "";
            // 
            // btnFinish
            // 
            this.btnFinish.Location = new System.Drawing.Point(580, 448);
            this.btnFinish.Name = "btnFinish";
            this.btnFinish.Size = new System.Drawing.Size(98, 23);
            this.btnFinish.TabIndex = 0;
            this.btnFinish.Text = "Finish";
            this.btnFinish.UseVisualStyleBackColor = true;
            this.btnFinish.Visible = false;
            this.btnFinish.Click += new System.EventHandler(this.btnFinish_Click);
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(548, 29);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(69, 13);
            this.lblTime.TabIndex = 50;
            this.lblTime.Text = "Backup time:";
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // frmWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 525);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.richTextErrors);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblTotalRequests);
            this.Controls.Add(this.lblProcessedRequests);
            this.Controls.Add(this.lblPlannedRequests);
            this.Controls.Add(this.lblPendingRequests);
            this.Controls.Add(this.richTextDetails);
            this.Controls.Add(this.chkDetails);
            this.Controls.Add(this.lblGlobalProgress);
            this.Controls.Add(this.lblCurrentAction);
            this.Controls.Add(this.progressGlobal);
            this.Controls.Add(this.progressCurrent);
            this.Controls.Add(this.lblInformation);
            this.Controls.Add(this.btnTryAgain);
            this.Controls.Add(this.btnFinish);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrevious);
            this.Name = "frmWizard";
            this.Text = "Backup Wizard";
            this.Load += new System.EventHandler(this.frmWizard_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnTryAgain;
        private System.Windows.Forms.Label lblInformation;
        private System.Windows.Forms.ProgressBar progressCurrent;
        private System.Windows.Forms.Label lblCurrentAction;
        private System.Windows.Forms.ProgressBar progressGlobal;
        private System.Windows.Forms.Label lblGlobalProgress;
        private System.Windows.Forms.CheckBox chkDetails;
        private System.Windows.Forms.RichTextBox richTextDetails;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblPendingRequests;
        private System.Windows.Forms.Label lblPlannedRequests;
        private System.Windows.Forms.Label lblTotalRequests;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblProcessedRequests;
        private System.Windows.Forms.RichTextBox richTextErrors;
        private System.Windows.Forms.Button btnFinish;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Timer timer2;
    }
}