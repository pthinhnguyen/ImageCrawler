using iTextSharp.text.pdf;
using iTextSharp.text;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpf_imageCrawler.src.entity;
using iTextSharp.text.pdf.parser;

namespace wpf_imageCrawler.GUI
{
    /// <summary>
    /// Interaction logic for PDFGenerator.xaml
    /// </summary>
    public partial class PDFGenerator : Window
    {
        public static PDFGenerator Instance = new PDFGenerator();
        private List<Folder> folders;
        private string outputLocation;
        
        private PDFGenerator()
        {
            InitializeComponent();

            this.folders = new List<Folder>();
            this.outputLocation = String.Empty;
            this.Closing += Window_Closing;

            var dpdDataGrid = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            if (dpdDataGrid != null)
            {
                dpdDataGrid.AddValueChanged(this.dgInputInfo, dgInputInfoOnChange);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void dgInputInfoOnChange(object sender, EventArgs e)
        {
            if (dgInputInfo is null || dgInputInfo.Items.Count == 0 || String.IsNullOrEmpty(txtBoxOutputLocation.Text))
            {
                this.btnConvertToPDFs.IsEnabled = false;
            }
            else
            {
                this.btnConvertToPDFs.IsEnabled = true;
            }
        }

        private void txtBoxOutputLocationOnChange(object sender, EventArgs e)
        {
            if (dgInputInfo is null || dgInputInfo.Items.Count == 0 || String.IsNullOrEmpty(txtBoxOutputLocation.Text))
            {
                this.btnConvertToPDFs.IsEnabled = false;
            }
            else
            {
                this.btnConvertToPDFs.IsEnabled = true;
            }
        }

        private void btnSelectSingle_Click(object sender, RoutedEventArgs e)
        {   
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var selectedPath = dialog.SelectedPath;
            var selectedFolder = new DirectoryInfo(selectedPath).Name;
            var files = getAllImages(selectedPath);
            if (files.Count > 0)
            {
                this.folders = new List<Folder>();
                var folder = new Folder()
                {
                    Path = selectedPath,
                    Files = files,
                    Name = selectedFolder
                };
                folders.Add(folder);
                this.dgInputInfo.ItemsSource = folders;
            }
            else
            {
                this.folders = new List<Folder>();
                this.dgInputInfo.ItemsSource = folders;
            }
        }

        private void btnSelectParent_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var selecedtPath = dialog.SelectedPath;
            var dirs = Directory.GetDirectories(path: selecedtPath, searchPattern: "*", searchOption: SearchOption.TopDirectoryOnly);

            this.folders = new List<Folder>();
            if (dirs is null || dirs.Count() == 0)
            {
                this.dgInputInfo.ItemsSource = folders;
                return;
            }
            
            foreach (var dir in dirs)
            {
                var selectedFolder = new DirectoryInfo(dir).Name;
                var files = getAllImages(dir);
                if (files.Count > 0)
                {
                    var folder = new Folder()
                    {
                        Path = dir,
                        Files = files,
                        Name = selectedFolder
                    };
                    folders.Add(folder);
                }
            }

            this.dgInputInfo.ItemsSource = folders;
        }

        private void btnBrowseOutputLocation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            this.txtBoxOutputLocation.Text = dialog.SelectedPath;
            this.outputLocation = dialog.SelectedPath;
        }

        private void btnConvertToPDFs_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            foreach (var folder in folders)
            {
                Document document = new Document(PageSize.A4);

                try
                {
                    // Create a PdfWriter instance to write the document to a file
                    string filePath = System.IO.Path.Combine(this.outputLocation, folder.Name + ".pdf");
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        PdfWriter writer = PdfWriter.GetInstance(document, fs);
                        document.Open();
                        writer.Open();
                        document.SetMargins(25, 25, 25, 25);

                        foreach (var imagePath in folder.Files)
                        {
                            // Load the image
                            var itextSharpImage = iTextSharp.text.Image.GetInstance(imagePath);

                            // Get the dimension of the image
                            var imageWidth = itextSharpImage.Width;
                            var imageHeight = itextSharpImage.Height;

                            //var itextSharpImage = iTextSharp.text.Image.GetInstance(imagePath);

                            // Add the image to a new page, added 50 to both height and width for padding
                            document.SetPageSize(new iTextSharp.text.Rectangle(imageWidth + 50, imageHeight + 50));
                            document.NewPage();
                            document.Add(itextSharpImage);
                        }
                        document.Close();
                    }
                }
                catch (Exception ex)
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                    System.Windows.MessageBox.Show(ex.Message, "Convert to PDF failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                }
            }

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            System.Windows.MessageBox.Show("Convert to PDF(s) finished!", "Task Done!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static List<string> getAllImages(string path)
        {
            var files = Directory.EnumerateFiles(path: path, searchPattern: "*.*", searchOption: SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".png") || s.EndsWith(".gif") || s.EndsWith(".bmp"))
                .OrderBy(s =>
                {
                    var fileName = System.IO.Path.GetFileName(s);

                    // Split the string by "_"
                    var parts = fileName.Split('_');

                    // Check if there's a numeric part (at least one element after split)
                    if (parts.Length > 1)
                    {
                        // Parse the first numeric part (assuming it's the relevant one)
                        int numericPart;
                        if (int.TryParse(parts[0], out numericPart))
                        {
                            // Return a custom object for sorting
                            return numericPart;
                        }
                    }

                    return 0;
                })
                .ToList();

            return files;
        }

        private sealed class Folder
        {
            private string _path;
            private string _name;
            private List<string> _files;

            public string Path { get => _path; set => _path = value; }
            public string Name { get => _name; set => _name = value; }
            public List<string> Files { get => _files; set => _files = value; }

            public int NumFiles 
            { 
                get
                {
                    if (_files == null) return 0;
                    return _files.Count;
                }
            }

            public Folder()
            {
                _path = string.Empty;
                _name = string.Empty;
                _files = new List<string>();
            }

        }
    }

}
