using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BadgeGenerator
{
    /// <summary>
    /// Interaction logic for pPriview.xaml
    /// </summary>
    public partial class pPriview : Window
    {
        public pPriview()
        {
            InitializeComponent();
        }
        public void LoadImage(ImageSource image)
        {
            FixedDocument fixedDoc = new FixedDocument();
            fixedDoc.DocumentPaginator.PageSize = new Size(96 * 8.5, 96 * 11); // Letter size

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

            documentViewer.Document = fixedDoc;
        }
    }
}
