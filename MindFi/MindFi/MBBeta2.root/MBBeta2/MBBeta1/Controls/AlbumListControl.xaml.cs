﻿using System;
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
            MessageBox.Show("Download Photos here");
        }

        
    }
}
