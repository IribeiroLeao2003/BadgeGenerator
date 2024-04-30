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
                using (Bitmap originalImage = new Bitmap(filename))
                {
                    int requiredWidth = (int)(widthMax * dpiX);
                    int requiredHeight = (int)(heightMax * dpiX);


                    using (Bitmap resizedImage = new Bitmap(originalImage, requiredWidth, requiredHeight))
                    {
                        photoImage.Source = ResizeImage(resizedImage);
                        fileName.Text = Path.GetFileName(filename);
                        nameCheck.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

                    }
                }
            }
        }


        private BitmapImage ResizeImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
     

           
            if (System.Text.RegularExpressions.Regex.IsMatch(empNumber.Text, @"\D"))
            {

                empCheck.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            else
            {

                empCheck.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            }

        }

        private void empName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(empNumber.Text))
            {
                nameCheck.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            else
            {

                nameCheck.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            }
        }

        private void generateBadge_Click(object sender, RoutedEventArgs e)
        {
            string employeeName = empName.Text; 
            string employeeNumber = empNumber.Text;
            ImageSource empImage = photoImage.Source;

            if (empImage != null || string.IsNullOrWhiteSpace(employeeNumber) || string.IsNullOrWhiteSpace(employeeName)) {
                MessageBox.Show("Please fill all the boxes");
            }
            else
            {
                Employee generateBadge = new Employee(employeeName, employeeNumber, empImage);

            }
            
        }
    }
}
