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
                        imageCheck.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

                    }
                }
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
            ImageSource cmpLogo = GetCompanyLogo(); 


            if (empImage == null || string.IsNullOrWhiteSpace(employeeNumber) || string.IsNullOrWhiteSpace(employeeName)) {
                MessageBox.Show("Please fill all the boxes");
            }
            else
            {
                Employee generateBadge = new Employee(employeeName, employeeNumber, empImage);
                BitmapImage bitmapImage = GeneratePDF417Barcode(generateBadge);
                RenderTargetBitmap badgeImage = CreateBadge(generateBadge, bitmapImage, cmpLogo);
                photoImage.Source = badgeImage;


            }
            
        }



        private ImageSource GetCompanyLogo()
        {
            try
            {
                BitmapImage companyLogo = new BitmapImage();
                companyLogo.BeginInit();
                string basePath = System.AppDomain.CurrentDomain.BaseDirectory; 
                string relativePath = @"..\..\Images\MPLogo.png"; 
                string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePath, relativePath));
                companyLogo.UriSource = new Uri(fullPath, UriKind.Absolute);
                companyLogo.CacheOption = BitmapCacheOption.OnLoad;  
                companyLogo.EndInit();

                return companyLogo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load image: " + ex.Message);
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


            var result = writer.Write(employee.EmpNumber);

            return ResizeImage(result);
        }


        private RenderTargetBitmap CreateBadge(Employee employee, BitmapImage barcodeImage, ImageSource companyLogo)
        {
            int width = 354; 
            int height = 529;


            int yPosLogo = 60;
            int yPosEmpImg = 130;
            int yPosEmpName = 325;
            int yPosBarcode = height - 140;
            int yPosEmpNum = height - 30;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

               
                if (companyLogo != null)
                    context.DrawImage(companyLogo, new Rect((width - 170) / 2, yPosLogo, 170, 60));

               
                if (employee.EmpImage != null)
                    context.DrawImage(employee.EmpImage, new Rect((width - 190) / 2, yPosEmpImg, 190, 190));

                
                FormattedText nameText = new FormattedText(
                    employee.EmpName.ToUpper(), 
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"), 
                    23, 
                    Brushes.Black,
                    new NumberSubstitution(),
                    1);
                context.DrawText(nameText, new Point((width - nameText.Width) / 2, yPosEmpName));

                
                if (barcodeImage != null)
                    context.DrawImage(barcodeImage, new Rect((width - 300) / 2, yPosBarcode, 400, 250));

                
                FormattedText numberText = new FormattedText(
                    employee.EmpNumber,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),  
                    20, 
                    Brushes.Black,
                    new NumberSubstitution(),
                    1);

                FormattedText roleText = new FormattedText(
                    "MPK",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    20,
                    Brushes.Black,
                    new NumberSubstitution(),
                    1);
                context.DrawText(numberText, new Point((width + 210 - numberText.Width) / 2, yPosEmpNum));
                context.DrawText(roleText, new Point((width - 210 - numberText.Width) / 2, yPosEmpNum));


                
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


        private static BitmapImage ResizeImage(Bitmap bitmap)
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

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            photoImage.Source = null;
            imageCheck.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
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

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Position = 0; 

                var pdfDocument = new PdfSharp.Pdf.PdfDocument();
                var pdfPage = pdfDocument.AddPage();
                pdfPage.Width = photoImage.ActualWidth;
                pdfPage.Height = photoImage.ActualHeight;

                using (var xgr = XGraphics.FromPdfPage(pdfPage))
                {
                    var xImage = XImage.FromStream(stream);
                    xgr.DrawImage(xImage, 0, 0);
                }

                string pdfFileName = "output.pdf";
                pdfDocument.Save(pdfFileName);
                Process.Start("explorer.exe", pdfFileName);
            }
        }


    }
}
