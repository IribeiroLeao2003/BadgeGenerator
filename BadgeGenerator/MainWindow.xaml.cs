﻿using Microsoft.Win32;
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
using ZXing.Common;
using ZXing.Rendering;
using ZXing;
using System.Globalization;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Pen = System.Windows.Media.Pen;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using PdfSharp.Drawing;



namespace BadgeGenerator
{

    public partial class MainWindow : Window
    {

        private double dpiX = 96;
        private double widthMax = 2.13;
        private double heightMax = 3.38;
        private string logoPath;


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
                        photoImage.Source = ResizeImage(resizedImage, 120, 130);
                     

                    }
                }
            }
        }


        private static BitmapImage ResizeBarcode(Bitmap bitmap)
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

        }

        private void empName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


      

        private void generateBadge_Click(object sender, RoutedEventArgs e)
        {
            string employeeName = empName.Text;
            string barcodeNumber = barcodeField.Text;
            string employeeNumber = empNumber.Text;
            ImageSource empImage = photoImage.Source;
            ImageSource cmpLogo = GetCompanyLogo();

            if (empImage == null || string.IsNullOrWhiteSpace(employeeNumber) || string.IsNullOrWhiteSpace(employeeName) || string.IsNullOrWhiteSpace(logoPath)) {
                MessageBox.Show("Please fill all the information");
            }
            else
            {
                photoImage.Source = null;
                Employee generateBadge = new Employee(employeeName, employeeNumber, barcodeNumber, empImage);
                try
                {
                    BitmapImage bitmapImage = GeneratePDF417Barcode(generateBadge);
                    RenderTargetBitmap badgeImage = CreateBadge(generateBadge, bitmapImage, cmpLogo);
                    photoImage.Source = badgeImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in generating barcode or badge: " + ex.Message);
                }

            }
            
        }



        private ImageSource GetCompanyLogo()
        {
            try
            {
                if (string.IsNullOrEmpty(logoPath) || !File.Exists(logoPath))
                {
                    MessageBox.Show("Logo file not found. Please ensure the logo path is correct.");
                    return null;
                }

                BitmapImage companyLogo = new BitmapImage();
                companyLogo.BeginInit();
                companyLogo.UriSource = new Uri(logoPath, UriKind.Absolute);
                companyLogo.CacheOption = BitmapCacheOption.OnLoad;
                companyLogo.EndInit();

                return companyLogo;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to locate logo: " + ex.Message);
                return null;
            }
        }




        public static BitmapImage GeneratePDF417Barcode(Employee employee)
        {
            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.PDF_417,
                Options = new EncodingOptions
                {
                    Width = 300,
                    Height = 150,
                    Margin = 10
                },
                Renderer = new BitmapRenderer()
            };

            var result = writer.Write(employee.EmpBarcode);

            return ResizeBarcode(result);
        }


        private RenderTargetBitmap CreateBadge(Employee employee, BitmapImage barcodeImage, ImageSource companyLogo)
        {
            int width = 204;
            int height = 324;


            int yPosLogo = 40;
            int yPosEmpImg = 98;
            int yPosEmpName = 235;
            int yPosBarcode = height - 70;  
            int yPosEmpNum = height - 15;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

               
                if (companyLogo != null)
                    context.DrawImage(companyLogo, new Rect((width - 140) / 2, yPosLogo, 140, 50));

               
                if (employee.EmpImage != null)
                    context.DrawImage(employee.EmpImage, new Rect((width - 120) / 2, yPosEmpImg, 120, 130));

                
                FormattedText nameText = new FormattedText(
                    employee.EmpName.ToUpper(), 
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"), 
                    15, 
                    Brushes.Black,
                    new NumberSubstitution(),
                    1);
                context.DrawText(nameText, new Point((width - nameText.Width) / 2, yPosEmpName));

                
                if (barcodeImage != null)
                    context.DrawImage(barcodeImage, new Rect((width - 190) / 2, yPosBarcode, 220, 120));

                
                FormattedText numberText = new FormattedText(
                    employee.EmpNumber,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),  
                    10, 
                    Brushes.Black,
                    new NumberSubstitution(),
                    1);

                FormattedText roleText = new FormattedText(
                    "MPK",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    10,
                    Brushes.Black,
                    new NumberSubstitution(),
                    1);
                context.DrawText(numberText, new Point((width + 152 - numberText.Width) / 2, yPosEmpNum));
                context.DrawText(roleText, new Point((width - 152 - numberText.Width) / 2, yPosEmpNum));


                
            }

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                width,
                height,
                96, 
                96, 
                PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);

            return renderBitmap;
        }


        private static BitmapImage ResizeImage(Bitmap bitmap, int targetWidth, int targetHeight)
        {
            int originalWidth = bitmap.Width;
            int originalHeight = bitmap.Height;

            float ratioX = (float)targetWidth / originalWidth;
            float ratioY = (float)targetHeight / originalHeight;
            float ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.DecodePixelWidth = newWidth;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }


        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            photoImage.Source = null;
            empNumber.Text = " ";
            empName.Text = " ";
            barcodeField.Text = " ";


        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(photoImage, "Printing Image");
            }

            var renderTargetBitmap = new RenderTargetBitmap((int)photoImage.ActualWidth, (int)photoImage.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(photoImage);

            var encoder = new PngBitmapEncoder(); 
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image files (*.png)|*.png|All files (*.*)|*.*"
            };

            bool? result = dlg.ShowDialog();

           
            if (result == true)
            {

                logoPath = dlg.FileName;
            }
        }
    }
}
