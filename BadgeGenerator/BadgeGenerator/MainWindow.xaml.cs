using Microsoft.Win32;
using System;
using System.IO; 
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace BadgeGenerator
{

    public partial class MainWindow : Window
    {

        private double dpiX = 96;
        private double widthMax = 2.13;
        private double heightMax = 3.38;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void getPhotoFileLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if ((bool)openFileDialog.ShowDialog())
            {
                string filename = openFileDialog.FileName;
                using (Bitmap image = new Bitmap(filename))
                {

   
                    int requiredWidth = (int)(widthMax * dpiX);
                    int requiredHeight = (int)(heightMax * dpiX);

                    
                    if (image.Width != requiredWidth || image.Height != requiredHeight)
                    {
                        MessageBox.Show("The image size must be exactly 2.13 x 3.38 inches.", "Invalid Image Size", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        fileName.Text = Path.GetFileName(filename);
                    }
                }
            }
        }
    }
}
