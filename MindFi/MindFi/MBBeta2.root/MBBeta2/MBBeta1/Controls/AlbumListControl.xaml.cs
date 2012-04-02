using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MBBetaAPI;
using MBBetaAPI.AgentAPI;

namespace MBBeta2.Controls
{
    /// <summary>
    /// Interaction logic for AlbumListControl.xaml
    /// </summary>
    public partial class AlbumListControl : UserControl
    {
        public AlbumListControl()
        {
            InitializeComponent();
        }

        private void DownloadPhotosBt_Click(object sender, RoutedEventArgs e)
        {
            object clicked = (e.OriginalSource as FrameworkElement).DataContext;
            var lbi = AlbumListBox.ItemContainerGenerator.ContainerFromItem(clicked) as ListBoxItem;
            lbi.IsSelected = true;
            SNPhotoAlbum SelectedAlbum; // = new SNPhotoAlbum();
            SelectedAlbum = (SNPhotoAlbum)AlbumListBox.SelectedItem;
            // MessageBox.Show("Download Photos here");
            // TODO: Download photos for the album
            if (SelectedAlbum != null)
            {
                if (!System.IO.Directory.Exists(AsyncReqQueue.AlbumDestinationDir + "\\" + SelectedAlbum.SNID))
                {
                    System.IO.Directory.CreateDirectory(AsyncReqQueue.AlbumDestinationDir + "\\" + SelectedAlbum.SNID);
                }

                //System.Windows.Forms.MessageBox.Show("sending req for photos in album " + album.SNID + " ID: " + album.ID );
                AsyncReqQueue apiReq = FBAPI.PhotosInAlbum(SelectedAlbum.SNID.ToString(), AsyncReqQueue.SIZETOGETPERPAGE, AsyncReqQueue.ProcessPhotosInAlbum, (int?)SelectedAlbum.ID);
                apiReq.QueueAndSend(999);
                apiReq = FBAPI.Likes(SelectedAlbum.SNID.ToString(), AsyncReqQueue.SIZETOGETPERPAGE, AsyncReqQueue.ProcessLikes, (int?)SelectedAlbum.ID);
                apiReq.QueueAndSend(999);
            }
            else
            {
                // TODO: Localize
                MessageBox.Show("Download Photos should be here, album is not selected");
            }
        }

        
    }
}
