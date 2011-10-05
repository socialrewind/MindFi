using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyBackup
{
    public partial class frmSelect : Form
    {
        public frmSelect()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            frmMain.currentProfile.backupMyInbox = chkInbox.Checked;
            frmMain.currentProfile.backupMyEvents = chkOwnEvents.Checked;
            frmMain.currentProfile.backupMyInformation = chkOwnInfo.Checked;
            frmMain.currentProfile.backupFriendsInfo = chkOwnFriends.Checked;
            frmMain.currentProfile.backupMyPhotos = chkOwnPhotos.Checked;
            frmMain.currentProfile.backupMyAlbums = chkOwnAlbums.Checked;
            frmMain.currentProfile.backupMyPosts = chkOwnPosts.Checked;
            frmMain.currentProfile.autoNextButton = chkAutoNext.Checked;          
            Close();
        }

        private void chkOwnAlbums_CheckedChanged(object sender, EventArgs e)
        {
            // cannot get photos without albums
            if (!chkOwnAlbums.Checked)
            {
                chkOwnPhotos.Checked = false;
            }
            chkOwnPhotos.Enabled = chkOwnAlbums.Checked;
        }

        private void chkAutoNext_CheckedChanged(object sender, EventArgs e)
        {
            txtSeconds.Enabled = chkAutoNext.Checked;
        }

    }
}
