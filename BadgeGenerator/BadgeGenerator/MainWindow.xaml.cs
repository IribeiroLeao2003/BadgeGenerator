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
using System.Windows.Markup;
using Size = System.Windows.Size;
using Image = System.Windows.Controls.Image;
using System.Windows.Threading;



namespace BadgeGenerator
{

    public partial class MainWindow : Window
    {

        private double dpiX = 96;
        private double widthMax = 2.13;
        private double heightMax = 3.38;
        private string logoPath;

        private BitmapImage userPicture; 


        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            string filePath = "settings.txt";
            try
            {
                if (File.Exists(filePath))
                {
                    logoPath = File.ReadAllText(filePath);
                    chkDefaultPath.IsChecked = true;
                  
                }
                else
                {
                    chkDefaultPath.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load settings: " + ex.Message);
            }
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
                        userPicture = (BitmapImage)photoImage.Source; 
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
            string employeeName = empName.Text;

        }


      

        private void generateBadge_Click(object sender, RoutedEventArgs e)
        {
            
            string employeeName = empName.Text;
            string barcodeNumber = barcodeField.Text;
            string employeeNumber = empNumber.Text;
            ImageSource empImage = userPicture; 
            ImageSource cmpLogo = GetCompanyLogo();


          

            if (empImage == null || string.IsNullOrWhiteSpace(employeeNumber) || string.IsNullOrWhiteSpace(employeeName) || string.IsNullOrWhiteSpace(logoPath)) {
                MessageBox.Show("Please fill all the information");
            }
            else
            {

                if (photoImage.Source != null)
                {
                    ClearImageSource();
                }

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


        private void ClearImageSource()
        {
            Dispatcher.Invoke(() =>
            {
                photoImage.Source = null;
                photoImage.InvalidateVisual();
                photoImage.UpdateLayout();
            }, DispatcherPriority.Render); 
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

        private FixedDocument CreateFixedDocument(BitmapImage image)
        {

            FixedDocument fixedDoc = new FixedDocument();
            fixedDoc.DocumentPaginator.PageSize = new Size(96 * 8.5, 96 * 11); 

         
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();
            fixedPage.Width = fixedDoc.DocumentPaginator.PageSize.Width;
            fixedPage.Height = fixedDoc.DocumentPaginator.PageSize.Height;

            Image img = new Image();
            img.Source = image;
            img.Width = fixedPage.Width;
            img.Height = fixedPage.Height;
            img.Stretch = Stretch.Uniform;

    
            fixedPage.Children.Add(img);
            ((IAddChild)pageContent).AddChild(fixedPage);

           
            fixedDoc.Pages.Add(pageContent);

            return fixedDoc;
        }


        private void ShowPrintPreview(BitmapImage image)
        {
            
            FixedDocument fixedDocument = CreateFixedDocument(image);

            
            DocumentViewer viewer = new DocumentViewer();
            viewer.Document = fixedDocument;

           
            Window previewWindow = new Window();
            previewWindow.Content = viewer;
            previewWindow.Title = "Print Preview";
            previewWindow.Width = 600;
            previewWindow.Height = 800;
            previewWindow.Show();
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            string filePath = "settings.txt";
            try
            {
                File.WriteAllText(filePath, logoPath); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings: " + ex.Message);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            string filePath = "settings.txt";
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to clear settings: " + ex.Message);
            }
        }

        private void barcodeField_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
