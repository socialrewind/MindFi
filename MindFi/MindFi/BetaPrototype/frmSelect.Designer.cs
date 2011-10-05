namespace MyBackup
{
    partial class frmSelect
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
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.chkOwnInfo = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chkFriendsInfo = new System.Windows.Forms.CheckBox();
            this.chkOthersInfo = new System.Windows.Forms.CheckBox();
            this.chkOwnPhotos = new System.Windows.Forms.CheckBox();
            this.chkOtherPhotos = new System.Windows.Forms.CheckBox();
            this.chkFriendPhotos = new System.Windows.Forms.CheckBox();
            this.chkFriendPosts = new System.Windows.Forms.CheckBox();
            this.chkOwnPosts = new System.Windows.Forms.CheckBox();
            this.chkOtherPosts = new System.Windows.Forms.CheckBox();
            this.chkFriendsEvents = new System.Windows.Forms.CheckBox();
            this.chkOwnEvents = new System.Windows.Forms.CheckBox();
            this.chkOtherEvents = new System.Windows.Forms.CheckBox();
            this.chkInbox = new System.Windows.Forms.CheckBox();
            this.chkSelectFriends = new System.Windows.Forms.CheckBox();
            this.chkSelectOthers = new System.Windows.Forms.CheckBox();
            this.btnSelectFriends = new System.Windows.Forms.Button();
            this.btnSelectFriendsOfFriends = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.chkFriendTaggedPhotos = new System.Windows.Forms.CheckBox();
            this.chkOtherTaggedPhotos = new System.Windows.Forms.CheckBox();
            this.chkFriendPostsTagged = new System.Windows.Forms.CheckBox();
            this.chkOtherPostsTagged = new System.Windows.Forms.CheckBox();
            this.chkFriendEventsAttending = new System.Windows.Forms.CheckBox();
            this.chkOtherEventsAttending = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.dateTimeBackupStart = new System.Windows.Forms.DateTimePicker();
            this.dateTimeBackupEnd = new System.Windows.Forms.DateTimePicker();
            this.label11 = new System.Windows.Forms.Label();
            this.txtNumberToBackup = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.chkOwnAlbums = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.chkOwnFriends = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.chkAutoNext = new System.Windows.Forms.CheckBox();
            this.txtSeconds = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Profile Info";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 142);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Photos";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 199);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Posts";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 244);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Events";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 288);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Messages";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(108, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "My own";
            this.label6.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(196, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "My Friends";
            this.label7.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(292, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Friends of friends";
            this.label8.Visible = false;
            // 
            // chkOwnInfo
            // 
            this.chkOwnInfo.AutoSize = true;
            this.chkOwnInfo.Checked = true;
            this.chkOwnInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOwnInfo.Enabled = false;
            this.chkOwnInfo.Location = new System.Drawing.Point(125, 72);
            this.chkOwnInfo.Name = "chkOwnInfo";
            this.chkOwnInfo.Size = new System.Drawing.Size(15, 14);
            this.chkOwnInfo.TabIndex = 1;
            this.chkOwnInfo.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(29, 319);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(30, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Who";
            this.label9.Visible = false;
            // 
            // chkFriendsInfo
            // 
            this.chkFriendsInfo.AutoSize = true;
            this.chkFriendsInfo.Checked = true;
            this.chkFriendsInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFriendsInfo.Location = new System.Drawing.Point(212, 73);
            this.chkFriendsInfo.Name = "chkFriendsInfo";
            this.chkFriendsInfo.Size = new System.Drawing.Size(15, 14);
            this.chkFriendsInfo.TabIndex = 2;
            this.chkFriendsInfo.UseVisualStyleBackColor = true;
            this.chkFriendsInfo.Visible = false;
            // 
            // chkOthersInfo
            // 
            this.chkOthersInfo.AutoSize = true;
            this.chkOthersInfo.Location = new System.Drawing.Point(310, 73);
            this.chkOthersInfo.Name = "chkOthersInfo";
            this.chkOthersInfo.Size = new System.Drawing.Size(15, 14);
            this.chkOthersInfo.TabIndex = 3;
            this.chkOthersInfo.UseVisualStyleBackColor = true;
            this.chkOthersInfo.Visible = false;
            // 
            // chkOwnPhotos
            // 
            this.chkOwnPhotos.AutoSize = true;
            this.chkOwnPhotos.Checked = true;
            this.chkOwnPhotos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOwnPhotos.Location = new System.Drawing.Point(125, 140);
            this.chkOwnPhotos.Name = "chkOwnPhotos";
            this.chkOwnPhotos.Size = new System.Drawing.Size(15, 14);
            this.chkOwnPhotos.TabIndex = 4;
            this.chkOwnPhotos.UseVisualStyleBackColor = true;
            // 
            // chkOtherPhotos
            // 
            this.chkOtherPhotos.AutoSize = true;
            this.chkOtherPhotos.Location = new System.Drawing.Point(310, 141);
            this.chkOtherPhotos.Name = "chkOtherPhotos";
            this.chkOtherPhotos.Size = new System.Drawing.Size(37, 17);
            this.chkOtherPhotos.TabIndex = 7;
            this.chkOtherPhotos.Text = "All";
            this.chkOtherPhotos.UseVisualStyleBackColor = true;
            this.chkOtherPhotos.Visible = false;
            // 
            // chkFriendPhotos
            // 
            this.chkFriendPhotos.AutoSize = true;
            this.chkFriendPhotos.Location = new System.Drawing.Point(212, 140);
            this.chkFriendPhotos.Name = "chkFriendPhotos";
            this.chkFriendPhotos.Size = new System.Drawing.Size(37, 17);
            this.chkFriendPhotos.TabIndex = 5;
            this.chkFriendPhotos.Text = "All";
            this.chkFriendPhotos.UseVisualStyleBackColor = true;
            this.chkFriendPhotos.Visible = false;
            // 
            // chkFriendPosts
            // 
            this.chkFriendPosts.AutoSize = true;
            this.chkFriendPosts.Location = new System.Drawing.Point(212, 198);
            this.chkFriendPosts.Name = "chkFriendPosts";
            this.chkFriendPosts.Size = new System.Drawing.Size(37, 17);
            this.chkFriendPosts.TabIndex = 10;
            this.chkFriendPosts.Text = "All";
            this.chkFriendPosts.UseVisualStyleBackColor = true;
            this.chkFriendPosts.Visible = false;
            // 
            // chkOwnPosts
            // 
            this.chkOwnPosts.AutoSize = true;
            this.chkOwnPosts.Checked = true;
            this.chkOwnPosts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOwnPosts.Location = new System.Drawing.Point(125, 198);
            this.chkOwnPosts.Name = "chkOwnPosts";
            this.chkOwnPosts.Size = new System.Drawing.Size(15, 14);
            this.chkOwnPosts.TabIndex = 9;
            this.chkOwnPosts.UseVisualStyleBackColor = true;
            // 
            // chkOtherPosts
            // 
            this.chkOtherPosts.AutoSize = true;
            this.chkOtherPosts.Location = new System.Drawing.Point(310, 199);
            this.chkOtherPosts.Name = "chkOtherPosts";
            this.chkOtherPosts.Size = new System.Drawing.Size(37, 17);
            this.chkOtherPosts.TabIndex = 12;
            this.chkOtherPosts.Text = "All";
            this.chkOtherPosts.UseVisualStyleBackColor = true;
            this.chkOtherPosts.Visible = false;
            // 
            // chkFriendsEvents
            // 
            this.chkFriendsEvents.AutoSize = true;
            this.chkFriendsEvents.Location = new System.Drawing.Point(212, 244);
            this.chkFriendsEvents.Name = "chkFriendsEvents";
            this.chkFriendsEvents.Size = new System.Drawing.Size(37, 17);
            this.chkFriendsEvents.TabIndex = 15;
            this.chkFriendsEvents.Text = "All";
            this.chkFriendsEvents.UseVisualStyleBackColor = true;
            this.chkFriendsEvents.Visible = false;
            // 
            // chkOwnEvents
            // 
            this.chkOwnEvents.AutoSize = true;
            this.chkOwnEvents.Checked = true;
            this.chkOwnEvents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOwnEvents.Location = new System.Drawing.Point(125, 244);
            this.chkOwnEvents.Name = "chkOwnEvents";
            this.chkOwnEvents.Size = new System.Drawing.Size(15, 14);
            this.chkOwnEvents.TabIndex = 14;
            this.chkOwnEvents.UseVisualStyleBackColor = true;
            // 
            // chkOtherEvents
            // 
            this.chkOtherEvents.AutoSize = true;
            this.chkOtherEvents.Location = new System.Drawing.Point(310, 245);
            this.chkOtherEvents.Name = "chkOtherEvents";
            this.chkOtherEvents.Size = new System.Drawing.Size(37, 17);
            this.chkOtherEvents.TabIndex = 17;
            this.chkOtherEvents.Text = "All";
            this.chkOtherEvents.UseVisualStyleBackColor = true;
            this.chkOtherEvents.Visible = false;
            // 
            // chkInbox
            // 
            this.chkInbox.AutoSize = true;
            this.chkInbox.Checked = true;
            this.chkInbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInbox.Location = new System.Drawing.Point(125, 288);
            this.chkInbox.Name = "chkInbox";
            this.chkInbox.Size = new System.Drawing.Size(15, 14);
            this.chkInbox.TabIndex = 19;
            this.chkInbox.UseVisualStyleBackColor = true;
            // 
            // chkSelectFriends
            // 
            this.chkSelectFriends.AutoSize = true;
            this.chkSelectFriends.Checked = true;
            this.chkSelectFriends.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSelectFriends.Location = new System.Drawing.Point(212, 319);
            this.chkSelectFriends.Name = "chkSelectFriends";
            this.chkSelectFriends.Size = new System.Drawing.Size(37, 17);
            this.chkSelectFriends.TabIndex = 20;
            this.chkSelectFriends.Text = "All";
            this.chkSelectFriends.UseVisualStyleBackColor = true;
            this.chkSelectFriends.Visible = false;
            // 
            // chkSelectOthers
            // 
            this.chkSelectOthers.AutoSize = true;
            this.chkSelectOthers.Location = new System.Drawing.Point(310, 319);
            this.chkSelectOthers.Name = "chkSelectOthers";
            this.chkSelectOthers.Size = new System.Drawing.Size(37, 17);
            this.chkSelectOthers.TabIndex = 21;
            this.chkSelectOthers.Text = "All";
            this.chkSelectOthers.UseVisualStyleBackColor = true;
            this.chkSelectOthers.Visible = false;
            // 
            // btnSelectFriends
            // 
            this.btnSelectFriends.Location = new System.Drawing.Point(199, 342);
            this.btnSelectFriends.Name = "btnSelectFriends";
            this.btnSelectFriends.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFriends.TabIndex = 22;
            this.btnSelectFriends.Text = "Select";
            this.btnSelectFriends.UseVisualStyleBackColor = true;
            this.btnSelectFriends.Visible = false;
            // 
            // btnSelectFriendsOfFriends
            // 
            this.btnSelectFriendsOfFriends.Location = new System.Drawing.Point(304, 342);
            this.btnSelectFriendsOfFriends.Name = "btnSelectFriendsOfFriends";
            this.btnSelectFriendsOfFriends.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFriendsOfFriends.TabIndex = 23;
            this.btnSelectFriendsOfFriends.Text = "Select";
            this.btnSelectFriendsOfFriends.UseVisualStyleBackColor = true;
            this.btnSelectFriendsOfFriends.Visible = false;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(199, 505);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 27;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // chkFriendTaggedPhotos
            // 
            this.chkFriendTaggedPhotos.AutoSize = true;
            this.chkFriendTaggedPhotos.Checked = true;
            this.chkFriendTaggedPhotos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFriendTaggedPhotos.Location = new System.Drawing.Point(212, 163);
            this.chkFriendTaggedPhotos.Name = "chkFriendTaggedPhotos";
            this.chkFriendTaggedPhotos.Size = new System.Drawing.Size(81, 17);
            this.chkFriendTaggedPhotos.TabIndex = 6;
            this.chkFriendTaggedPhotos.Text = "Me Tagged";
            this.chkFriendTaggedPhotos.UseVisualStyleBackColor = true;
            this.chkFriendTaggedPhotos.Visible = false;
            // 
            // chkOtherTaggedPhotos
            // 
            this.chkOtherTaggedPhotos.AutoSize = true;
            this.chkOtherTaggedPhotos.Checked = true;
            this.chkOtherTaggedPhotos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOtherTaggedPhotos.Location = new System.Drawing.Point(310, 163);
            this.chkOtherTaggedPhotos.Name = "chkOtherTaggedPhotos";
            this.chkOtherTaggedPhotos.Size = new System.Drawing.Size(81, 17);
            this.chkOtherTaggedPhotos.TabIndex = 8;
            this.chkOtherTaggedPhotos.Text = "Me Tagged";
            this.chkOtherTaggedPhotos.UseVisualStyleBackColor = true;
            this.chkOtherTaggedPhotos.Visible = false;
            // 
            // chkFriendPostsTagged
            // 
            this.chkFriendPostsTagged.AutoSize = true;
            this.chkFriendPostsTagged.Checked = true;
            this.chkFriendPostsTagged.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFriendPostsTagged.Location = new System.Drawing.Point(212, 219);
            this.chkFriendPostsTagged.Name = "chkFriendPostsTagged";
            this.chkFriendPostsTagged.Size = new System.Drawing.Size(81, 17);
            this.chkFriendPostsTagged.TabIndex = 11;
            this.chkFriendPostsTagged.Text = "Me Tagged";
            this.chkFriendPostsTagged.UseVisualStyleBackColor = true;
            this.chkFriendPostsTagged.Visible = false;
            // 
            // chkOtherPostsTagged
            // 
            this.chkOtherPostsTagged.AutoSize = true;
            this.chkOtherPostsTagged.Checked = true;
            this.chkOtherPostsTagged.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOtherPostsTagged.Location = new System.Drawing.Point(310, 219);
            this.chkOtherPostsTagged.Name = "chkOtherPostsTagged";
            this.chkOtherPostsTagged.Size = new System.Drawing.Size(81, 17);
            this.chkOtherPostsTagged.TabIndex = 13;
            this.chkOtherPostsTagged.Text = "Me Tagged";
            this.chkOtherPostsTagged.UseVisualStyleBackColor = true;
            this.chkOtherPostsTagged.Visible = false;
            // 
            // chkFriendEventsAttending
            // 
            this.chkFriendEventsAttending.AutoSize = true;
            this.chkFriendEventsAttending.Checked = true;
            this.chkFriendEventsAttending.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFriendEventsAttending.Location = new System.Drawing.Point(212, 264);
            this.chkFriendEventsAttending.Name = "chkFriendEventsAttending";
            this.chkFriendEventsAttending.Size = new System.Drawing.Size(71, 17);
            this.chkFriendEventsAttending.TabIndex = 16;
            this.chkFriendEventsAttending.Text = "Attending";
            this.chkFriendEventsAttending.UseVisualStyleBackColor = true;
            this.chkFriendEventsAttending.Visible = false;
            // 
            // chkOtherEventsAttending
            // 
            this.chkOtherEventsAttending.AutoSize = true;
            this.chkOtherEventsAttending.Checked = true;
            this.chkOtherEventsAttending.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOtherEventsAttending.Location = new System.Drawing.Point(310, 264);
            this.chkOtherEventsAttending.Name = "chkOtherEventsAttending";
            this.chkOtherEventsAttending.Size = new System.Drawing.Size(71, 17);
            this.chkOtherEventsAttending.TabIndex = 18;
            this.chkOtherEventsAttending.Text = "Attending";
            this.chkOtherEventsAttending.UseVisualStyleBackColor = true;
            this.chkOtherEventsAttending.Visible = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(32, 374);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "Backup from";
            // 
            // dateTimeBackupStart
            // 
            this.dateTimeBackupStart.Location = new System.Drawing.Point(113, 374);
            this.dateTimeBackupStart.Name = "dateTimeBackupStart";
            this.dateTimeBackupStart.Size = new System.Drawing.Size(200, 20);
            this.dateTimeBackupStart.TabIndex = 24;
            this.dateTimeBackupStart.Value = new System.DateTime(2004, 2, 1, 0, 0, 0, 0);
            // 
            // dateTimeBackupEnd
            // 
            this.dateTimeBackupEnd.Location = new System.Drawing.Point(113, 406);
            this.dateTimeBackupEnd.Name = "dateTimeBackupEnd";
            this.dateTimeBackupEnd.Size = new System.Drawing.Size(200, 20);
            this.dateTimeBackupEnd.TabIndex = 25;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(29, 444);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(217, 13);
            this.label11.TabIndex = 16;
            this.label11.Text = "Number of elements of each type to backup:";
            // 
            // txtNumberToBackup
            // 
            this.txtNumberToBackup.Location = new System.Drawing.Point(260, 444);
            this.txtNumberToBackup.Name = "txtNumberToBackup";
            this.txtNumberToBackup.Size = new System.Drawing.Size(96, 20);
            this.txtNumberToBackup.TabIndex = 26;
            this.txtNumberToBackup.Text = "100000";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(29, 118);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(41, 13);
            this.label12.TabIndex = 1;
            this.label12.Text = "Albums";
            // 
            // chkOwnAlbums
            // 
            this.chkOwnAlbums.AutoSize = true;
            this.chkOwnAlbums.Checked = true;
            this.chkOwnAlbums.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOwnAlbums.Location = new System.Drawing.Point(125, 116);
            this.chkOwnAlbums.Name = "chkOwnAlbums";
            this.chkOwnAlbums.Size = new System.Drawing.Size(15, 14);
            this.chkOwnAlbums.TabIndex = 4;
            this.chkOwnAlbums.UseVisualStyleBackColor = true;
            this.chkOwnAlbums.CheckedChanged += new System.EventHandler(this.chkOwnAlbums_CheckedChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(29, 96);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(57, 13);
            this.label13.TabIndex = 0;
            this.label13.Text = "Friend Info";
            // 
            // chkOwnFriends
            // 
            this.chkOwnFriends.AutoSize = true;
            this.chkOwnFriends.Checked = true;
            this.chkOwnFriends.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOwnFriends.Enabled = false;
            this.chkOwnFriends.Location = new System.Drawing.Point(125, 95);
            this.chkOwnFriends.Name = "chkOwnFriends";
            this.chkOwnFriends.Size = new System.Drawing.Size(15, 14);
            this.chkOwnFriends.TabIndex = 1;
            this.chkOwnFriends.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(212, 96);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(15, 14);
            this.checkBox2.TabIndex = 2;
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.Visible = false;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(310, 96);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(15, 14);
            this.checkBox3.TabIndex = 3;
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.Visible = false;
            // 
            // chkAutoNext
            // 
            this.chkAutoNext.AutoSize = true;
            this.chkAutoNext.Checked = true;
            this.chkAutoNext.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoNext.Location = new System.Drawing.Point(35, 474);
            this.chkAutoNext.Name = "chkAutoNext";
            this.chkAutoNext.Size = new System.Drawing.Size(185, 17);
            this.chkAutoNext.TabIndex = 28;
            this.chkAutoNext.Text = "Advance steps automatically after";
            this.chkAutoNext.UseVisualStyleBackColor = true;
            this.chkAutoNext.CheckedChanged += new System.EventHandler(this.chkAutoNext_CheckedChanged);
            // 
            // txtSeconds
            // 
            this.txtSeconds.Location = new System.Drawing.Point(226, 471);
            this.txtSeconds.Name = "txtSeconds";
            this.txtSeconds.Size = new System.Drawing.Size(29, 20);
            this.txtSeconds.TabIndex = 29;
            this.txtSeconds.Text = "5";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(260, 475);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(47, 13);
            this.label14.TabIndex = 30;
            this.label14.Text = "seconds";
            // 
            // frmStep3
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 540);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtSeconds);
            this.Controls.Add(this.chkAutoNext);
            this.Controls.Add(this.txtNumberToBackup);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.dateTimeBackupEnd);
            this.Controls.Add(this.dateTimeBackupStart);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnSelectFriendsOfFriends);
            this.Controls.Add(this.btnSelectFriends);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.chkOtherEvents);
            this.Controls.Add(this.chkOtherPosts);
            this.Controls.Add(this.chkOtherPhotos);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.chkOthersInfo);
            this.Controls.Add(this.chkInbox);
            this.Controls.Add(this.chkSelectOthers);
            this.Controls.Add(this.chkSelectFriends);
            this.Controls.Add(this.chkOwnEvents);
            this.Controls.Add(this.chkFriendsEvents);
            this.Controls.Add(this.chkOwnPosts);
            this.Controls.Add(this.chkFriendPosts);
            this.Controls.Add(this.chkOwnAlbums);
            this.Controls.Add(this.chkOwnPhotos);
            this.Controls.Add(this.chkOtherEventsAttending);
            this.Controls.Add(this.chkOtherPostsTagged);
            this.Controls.Add(this.chkFriendEventsAttending);
            this.Controls.Add(this.chkFriendPostsTagged);
            this.Controls.Add(this.chkOtherTaggedPhotos);
            this.Controls.Add(this.chkFriendTaggedPhotos);
            this.Controls.Add(this.chkFriendPhotos);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.chkFriendsInfo);
            this.Controls.Add(this.chkOwnFriends);
            this.Controls.Add(this.chkOwnInfo);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label1);
            this.Name = "frmSelect";
            this.Text = "Step 2: What to backup";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkOwnInfo;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkFriendsInfo;
        private System.Windows.Forms.CheckBox chkOthersInfo;
        private System.Windows.Forms.CheckBox chkOwnPhotos;
        private System.Windows.Forms.CheckBox chkOtherPhotos;
        private System.Windows.Forms.CheckBox chkFriendPhotos;
        private System.Windows.Forms.CheckBox chkFriendPosts;
        private System.Windows.Forms.CheckBox chkOwnPosts;
        private System.Windows.Forms.CheckBox chkOtherPosts;
        private System.Windows.Forms.CheckBox chkFriendsEvents;
        private System.Windows.Forms.CheckBox chkOwnEvents;
        private System.Windows.Forms.CheckBox chkOtherEvents;
        private System.Windows.Forms.CheckBox chkInbox;
        private System.Windows.Forms.CheckBox chkSelectFriends;
        private System.Windows.Forms.CheckBox chkSelectOthers;
        private System.Windows.Forms.Button btnSelectFriends;
        private System.Windows.Forms.Button btnSelectFriendsOfFriends;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox chkFriendTaggedPhotos;
        private System.Windows.Forms.CheckBox chkOtherTaggedPhotos;
        private System.Windows.Forms.CheckBox chkFriendPostsTagged;
        private System.Windows.Forms.CheckBox chkOtherPostsTagged;
        private System.Windows.Forms.CheckBox chkFriendEventsAttending;
        private System.Windows.Forms.CheckBox chkOtherEventsAttending;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.DateTimePicker dateTimeBackupStart;
        private System.Windows.Forms.DateTimePicker dateTimeBackupEnd;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtNumberToBackup;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox chkOwnAlbums;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox chkOwnFriends;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox chkAutoNext;
        private System.Windows.Forms.TextBox txtSeconds;
        private System.Windows.Forms.Label label14;
    }
}