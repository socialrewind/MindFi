namespace MyBackup
{
    partial class frmTest
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblNumRecords = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnGet = new System.Windows.Forms.Button();
            this.btnParse = new System.Windows.Forms.Button();
            this.btnWatch = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lblRecordType = new System.Windows.Forms.Label();
            this.lblError = new System.Windows.Forms.Label();
            this.lblDBName = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblPwd = new System.Windows.Forms.Label();
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.btnOpenDB = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Records to parse:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 129);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Record text:";
            // 
            // lblNumRecords
            // 
            this.lblNumRecords.AutoSize = true;
            this.lblNumRecords.Location = new System.Drawing.Point(152, 73);
            this.lblNumRecords.Name = "lblNumRecords";
            this.lblNumRecords.Size = new System.Drawing.Size(79, 13);
            this.lblNumRecords.TabIndex = 2;
            this.lblNumRecords.Text = "lblNumRecords";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(155, 126);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(516, 179);
            this.richTextBox1.TabIndex = 3;
            this.richTextBox1.Text = "";
            // 
            // btnGet
            // 
            this.btnGet.Location = new System.Drawing.Point(158, 324);
            this.btnGet.Name = "btnGet";
            this.btnGet.Size = new System.Drawing.Size(75, 23);
            this.btnGet.TabIndex = 4;
            this.btnGet.Text = "Get Record";
            this.btnGet.UseVisualStyleBackColor = true;
            this.btnGet.Click += new System.EventHandler(this.btnGet_Click);
            // 
            // btnParse
            // 
            this.btnParse.Location = new System.Drawing.Point(372, 324);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(75, 23);
            this.btnParse.TabIndex = 5;
            this.btnParse.Text = "Parse";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
            // 
            // btnWatch
            // 
            this.btnWatch.Location = new System.Drawing.Point(593, 324);
            this.btnWatch.Name = "btnWatch";
            this.btnWatch.Size = new System.Drawing.Size(78, 23);
            this.btnWatch.TabIndex = 6;
            this.btnWatch.Text = "Watch";
            this.btnWatch.UseVisualStyleBackColor = true;
            this.btnWatch.Click += new System.EventHandler(this.btnWatch_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 101);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Record type:";
            // 
            // lblRecordType
            // 
            this.lblRecordType.AutoSize = true;
            this.lblRecordType.Location = new System.Drawing.Point(155, 100);
            this.lblRecordType.Name = "lblRecordType";
            this.lblRecordType.Size = new System.Drawing.Size(76, 13);
            this.lblRecordType.TabIndex = 8;
            this.lblRecordType.Text = "lblRecordType";
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(12, 369);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(39, 13);
            this.lblError.TabIndex = 9;
            this.lblError.Text = "lblError";
            // 
            // lblDBName
            // 
            this.lblDBName.Location = new System.Drawing.Point(15, 12);
            this.lblDBName.Name = "lblDBName";
            this.lblDBName.Size = new System.Drawing.Size(450, 16);
            this.lblDBName.TabIndex = 12;
            this.lblDBName.Text = "Select a database...";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(471, 6);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(288, 24);
            this.btnBrowse.TabIndex = 11;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblPwd
            // 
            this.lblPwd.Location = new System.Drawing.Point(26, 41);
            this.lblPwd.Name = "lblPwd";
            this.lblPwd.Size = new System.Drawing.Size(64, 16);
            this.lblPwd.TabIndex = 10;
            this.lblPwd.Text = "Password:";
            // 
            // txtPwd
            // 
            this.txtPwd.Location = new System.Drawing.Point(106, 41);
            this.txtPwd.Name = "txtPwd";
            this.txtPwd.PasswordChar = '*';
            this.txtPwd.Size = new System.Drawing.Size(207, 20);
            this.txtPwd.TabIndex = 13;
            // 
            // btnOpenDB
            // 
            this.btnOpenDB.Location = new System.Drawing.Point(471, 41);
            this.btnOpenDB.Name = "btnOpenDB";
            this.btnOpenDB.Size = new System.Drawing.Size(288, 24);
            this.btnOpenDB.TabIndex = 14;
            this.btnOpenDB.Text = "Open Database";
            this.btnOpenDB.UseVisualStyleBackColor = true;
            this.btnOpenDB.Click += new System.EventHandler(this.btnOpenDB_Click);
            // 
            // frmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.lblDBName);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblPwd);
            this.Controls.Add(this.txtPwd);
            this.Controls.Add(this.btnOpenDB);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.lblRecordType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnWatch);
            this.Controls.Add(this.btnParse);
            this.Controls.Add(this.btnGet);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.lblNumRecords);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "frmTest";
            this.Text = "Parsing Test Form";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmTest_FormClosed);
            this.Load += new System.EventHandler(this.frmTest_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblNumRecords;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnGet;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.Button btnWatch;
        //private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblRecordType;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Label lblDBName;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblPwd;
        private System.Windows.Forms.TextBox txtPwd;
        private System.Windows.Forms.Button btnOpenDB;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}