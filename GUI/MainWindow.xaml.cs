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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using wpf_imageCrawler.Resources;
using wpf_imageCrawler.src.controller;
using wpf_imageCrawler.src.entity;

using System.Windows.Forms;
using System.Text.RegularExpressions;
using wpf_imageCrawler.GUI;
using System.ComponentModel.DataAnnotations;
using wpf_imageCrawler.src.service;
using System.Security.Policy;
using System.Threading;
using System.ComponentModel;
using System.IO;

namespace wpf_imageCrawler
{
    public partial class MainWindow : Window
    {
        private static string downloadLocation = KnownFolders.GetPath(KnownFolder.Downloads);
        private static FolderBrowserDialog dialogFolder = new FolderBrowserDialog();

        private UserRequestData userRequest;
        private List<WebsiteData>? websiteList;
        private UserSetting userSetting;
        private int numberImageLinks;

        private CancellationTokenSource? cancelTokenAnalyze;
        private CancellationTokenSource? cancelTokenDownload;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;

            this.userRequest = new UserRequestData();
            this.websiteList = new List<WebsiteData>();
            this.userSetting = UserSetting.Instance;
            this.numberImageLinks = 0;
           
            this.Button_Download.IsEnabled = false;
            this.TextBox_DonwloadLocation.Text = this.userSetting.UserSettingData.DownloadLocation;
            this.CheckBox_EnablePageSeriesDownload.IsChecked = false;
            this.TextBox_Page2URL.IsEnabled = false;
            this.TextBox_Page3URL.IsEnabled = false;
            this.Textbox_FromPage.IsEnabled = false;
            this.Textbox_ToPage.IsEnabled = false;
            this.ComboBox_DiffFolderEach.IsEnabled = false;
            this.ProgressBar_Indicator.IsIndeterminate = false;
            this.Button_FixImageLinks.Visibility = Visibility.Hidden;
            this.Button_Analyze_Cancel.Visibility = Visibility.Hidden;
            this.Button_Download_Cancel.Visibility = Visibility.Hidden;
            this.ComboBox_CreateNewFolder.SelectedIndex = (this.userSetting.UserSettingData.CreateNewFolder) ? 1 : 0;
            this.ComboBox_DiffFolderEach.SelectedIndex = (this.userSetting.UserSettingData.DiffFolderEach) ? 1 : 0;
            this.resetDataGrid_ImageLinks();

            this.TextBox_XPathSelector.Text = "//img";
            this.Textbox_AttributeSelector.Text = "src";

            /** Test **/

        }

        public UserSetting UserSetting { get => userSetting; set => userSetting = value; }

        private void TextBox_MainURL_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_MainURL.Text.ToLower() == "website link")
                TextBox_MainURL.Text = "";
        }

        private void TextBox_MainURL_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBox_MainURL.Text.ToLower() == "")
                TextBox_MainURL.Text = "Website Link";
        }

        private void Button_DownloadLocationBrowse_Click(object sender, RoutedEventArgs e)
        {
            string resultSelectedPath = TextBox_DonwloadLocation.Text;

            // dialogFolder.Description = "Select a folder";
            dialogFolder.SelectedPath = resultSelectedPath;
            dialogFolder.ShowNewFolderButton = true;

            if (dialogFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                resultSelectedPath = dialogFolder.SelectedPath;
            }

            TextBox_DonwloadLocation.Text = resultSelectedPath;
            this.userSetting.UserSettingData.DownloadLocation = TextBox_DonwloadLocation.Text;
        }

        private void CheckBox_EnablePageSeriesDownload_Checked(object sender, RoutedEventArgs e)
        {
            this.TextBox_Page2URL.IsEnabled = true;
            this.TextBox_Page3URL.IsEnabled = true;
            this.Textbox_FromPage.IsEnabled = true;
            this.Textbox_ToPage.IsEnabled = true;
            this.ComboBox_DiffFolderEach.IsEnabled = true;
        }

        private void ComboBox_CreateNewFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ComboBox_CreateNewFolder.SelectedIndex == 1)
            {
                this.userSetting.UserSettingData.CreateNewFolder = true;
            }
            else
            {
                this.ComboBox_DiffFolderEach.SelectedIndex = 0;
                this.userSetting.UserSettingData.CreateNewFolder = false;
                this.userSetting.UserSettingData.DiffFolderEach = false;
            }

            this.userSetting.UserSettingData.CreateNewFolder = (this.ComboBox_CreateNewFolder.SelectedIndex == 1) ? true : false;
        }

        private void CheckBox_EnablePageSeriesDownload_Unchecked(object sender, RoutedEventArgs e)
        {
            /*this.TextBox_Page2URL.Text = "";
            this.TextBox_Page3URL.Text = "";
            this.Textbox_FromPage.Text = "";
            this.Textbox_ToPage.Text = "";*/
            
            this.TextBox_Page2URL.IsEnabled = false;
            this.TextBox_Page3URL.IsEnabled = false;
            this.Textbox_FromPage.IsEnabled = false;
            this.Textbox_ToPage.IsEnabled = false;
            this.ComboBox_DiffFolderEach.IsEnabled = false;
        }

        private void ComboBox_DiffFolderEach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ComboBox_DiffFolderEach.SelectedIndex == 1)
            {
                this.ComboBox_CreateNewFolder.SelectedIndex = 1;
                this.userSetting.UserSettingData.DiffFolderEach = true;
                this.userSetting.UserSettingData.CreateNewFolder = true;
            }
            else
            {
                this.userSetting.UserSettingData.DiffFolderEach = false;
            }
        }

        private async void Button_Analyze_Click(object sender, RoutedEventArgs e)
        {
            this.resetDetailInfomationResultView();

            if (!this.AreFieldsValid()) return;

            if (this.cancelTokenAnalyze is not null) this.cancelTokenAnalyze.Dispose();
            this.cancelTokenAnalyze = new CancellationTokenSource();

            this.Button_Analyze.IsEnabled = false;
            this.Button_Download.IsEnabled = false;
            this.ProgressBar_Indicator.IsIndeterminate = true;
            this.Button_FixImageLinks.Visibility = Visibility.Hidden;
            this.Button_ClearDownloadedLinks.Visibility = Visibility.Hidden;
            this.Button_Analyze.Visibility = Visibility.Hidden;
            this.Button_Analyze_Cancel.Visibility = Visibility.Visible;
            this.resetDataGrid_ImageLinks();
            this.TextBlock_ResultView_NumberOfImageLinks.Text = "";
            this.TextBlock_ResultView_ProcessDescription.Text = "Getting Image Links...";

            this.userRequest.MainURL = TextBox_MainURL.Text;
            this.userRequest.Page2URL = string.IsNullOrEmpty(TextBox_Page2URL.Text) ? "" : TextBox_Page2URL.Text;
            this.userRequest.Page3URL = string.IsNullOrEmpty(TextBox_Page3URL.Text) ? "" : TextBox_Page3URL.Text;
            this.userRequest.FromPage = string.IsNullOrEmpty(Textbox_FromPage.Text) ? 0 : int.Parse(Textbox_FromPage.Text);
            this.userRequest.ToPage = string.IsNullOrEmpty(Textbox_ToPage.Text) ? 0 : int.Parse(Textbox_ToPage.Text);
            this.userRequest.XpathSelector = string.IsNullOrEmpty(TextBox_XPathSelector.Text) ? "" : TextBox_XPathSelector.Text;
            this.userRequest.Attribute = string.IsNullOrEmpty(Textbox_AttributeSelector.Text) ? "" : Textbox_AttributeSelector.Text;

            if (this.userRequest.ImportedURLs != null && this.userRequest.ImportedURLs.Count > 0)
            {
                this.websiteList = await Scraper.analyzeImportedWebsites(this.userRequest, this.userSetting.UserSettingData, this.cancelTokenAnalyze.Token);
            }
            else if ((this.userRequest.ImportedURLs is null || this.userRequest.ImportedURLs.Count == 0) &&  this.CheckBox_EnablePageSeriesDownload.IsChecked == true)
            {
                this.websiteList = await Scraper.analyzeMutipleWebsites(this.userRequest, this.userSetting.UserSettingData, this.cancelTokenAnalyze.Token);
            }
            else
            {
                this.websiteList = await Scraper.analyzeSingleWebsite(this.userRequest, this.userSetting.UserSettingData, this.cancelTokenAnalyze.Token);
            }

            this.updateDataGrid_ImageLinks();

            this.ProgressBar_Indicator.IsIndeterminate = false;
            this.Button_Analyze.Visibility = Visibility.Visible;
            this.Button_Analyze_Cancel.Visibility = Visibility.Hidden;
            this.Button_Analyze.IsEnabled = true;
            this.Button_Download.IsEnabled = true;

            if (this.cancelTokenAnalyze is null || this.cancelTokenAnalyze.Token.IsCancellationRequested)
                this.TextBlock_ResultView_ProcessDescription.Text = "Analyzing Canceled"; 
            else
                this.TextBlock_ResultView_ProcessDescription.Text = "Finished Analyzing";
            
            if (this.numberImageLinks > 0)
                this.Button_FixImageLinks.Visibility = Visibility.Visible;

            this.TextBlock_ResultView_NumberOfImageLinks.Text = "Number of Image URLs: " + this.numberImageLinks;
            this.ProgressBar_Indicator.Minimum = 0;
            this.ProgressBar_Indicator.Maximum = this.numberImageLinks;
        }

        private void Button_Analyze_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Analyze.Visibility = Visibility.Visible;
            this.Button_Analyze_Cancel.Visibility = Visibility.Hidden;
            this.TextBlock_ResultView_ProcessDescription.Text = "Canceling...";

            if (this.cancelTokenAnalyze is not null)
            {
                this.cancelTokenAnalyze.Cancel();
                this.cancelTokenAnalyze.Dispose();
            }

            this.cancelTokenAnalyze = null;
        }

        private async void Button_Download_Click(object sender, RoutedEventArgs e)
        {
            this.resetDetailInfomationResultView();

            if (!this.AreFieldsValid()) return;
            if (this.numberImageLinks == 0 || this.websiteList is null || this.websiteList.Count == 0)
            {
                this.TextBlock_ResultView_ProcessDescription.Text = "NO Image Found to Download";
                return;
            }
            if (this.cancelTokenDownload is not null) this.cancelTokenDownload.Dispose();
            this.cancelTokenDownload = new CancellationTokenSource();

            this.Button_Download.IsEnabled = false;
            this.Button_Analyze.IsEnabled = false;
            this.Button_Import.IsEnabled = false;
            this.ProgressBar_Indicator.IsIndeterminate = false;
            this.Button_Download.Visibility = Visibility.Hidden;
            this.Button_Download_Cancel.Visibility = Visibility.Visible;
            this.TextBlock_ResultView_ProcessDescription.Text = "Downloading...";

            int numberDownloadedImage = await Scraper.downloadImages(this.websiteList, this.UserSetting.UserSettingData, this, this.cancelTokenDownload.Token);

            this.Button_Analyze.IsEnabled = true;
            this.Button_Download.IsEnabled = true;
            this.Button_Import.IsEnabled = true;
            this.Button_Download.Visibility = Visibility.Visible;
            this.Button_Download_Cancel.Visibility = Visibility.Hidden;

            if (this.cancelTokenDownload is null || this.cancelTokenDownload.Token.IsCancellationRequested)
                this.TextBlock_ResultView_ProcessDescription.Text = "Downloading Canceled";
            else
                this.TextBlock_ResultView_ProcessDescription.Text = "Finished Downloading";

            if (this.numberImageLinks > 0)
                this.Button_ClearDownloadedLinks.Visibility = Visibility.Visible;

            this.updateDataGrid_ImageLinks();
            this.TextBlock_ResultView_NumberOfImageLinks.Text = "Number of Image URLs: " + this.numberImageLinks;
            this.TextBlock_ResultView_NumberOfImageDownloaded.Text = "Number of Downloaded Images: " + numberDownloadedImage;
        }

        private void Button_Download_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Button_Download.Visibility = Visibility.Visible;
            this.Button_Download_Cancel.Visibility = Visibility.Hidden;
            this.TextBlock_ResultView_ProcessDescription.Text = "Canceling...";
            if (this.cancelTokenDownload is not null)
            {
                this.cancelTokenDownload.Cancel();
                this.cancelTokenDownload.Dispose();
            }
            this.cancelTokenDownload = null;
        }

        private void Button_FixImageLinks_Click(object sender, RoutedEventArgs e)
        {
            if (this.websiteList is null) return;
            
            for (int websiteIndex = this.websiteList.Count - 1; websiteIndex >= 0 ; websiteIndex--)
            {
                for(int imageLinkIndex = this.websiteList[websiteIndex].ImageLinkItemList.Count - 1; imageLinkIndex >= 0 ; imageLinkIndex--)
                {
                    string fixedURL = FieldValidator.fixURL(this.websiteList[websiteIndex].ImageLinkItemList[imageLinkIndex].ImageLink);

                    if (string.IsNullOrEmpty(fixedURL)) 
                        this.websiteList[websiteIndex].ImageLinkItemList.RemoveAt(imageLinkIndex);
                    else 
                        this.websiteList[websiteIndex].ImageLinkItemList[imageLinkIndex].ImageLink = fixedURL;
                }

                if (this.websiteList[websiteIndex].ImageLinkItemList.Count == 0) this.websiteList.RemoveAt(websiteIndex);
            }

            this.updateDataGrid_ImageLinks();
        }

        private void Button_ClearDownloadedLinks_Click(object sender, RoutedEventArgs e)
        {
            if (this.websiteList is null || this.websiteList.Count == 0)
            {
                resetDataGrid_ImageLinks();
                return;
            }

            for (int websiteIndex = this.websiteList.Count - 1; websiteIndex >= 0; websiteIndex--)
            {
                for (int imageLinkIndex = this.websiteList[websiteIndex].ImageLinkItemList.Count - 1; imageLinkIndex >= 0 ; imageLinkIndex--)
                {
                    if (this.websiteList[websiteIndex].ImageLinkItemList[imageLinkIndex].IsDownloaded)
                    {
                        this.websiteList[websiteIndex].ImageLinkItemList.RemoveAt(imageLinkIndex);
                        this.numberImageLinks--;
                    }
                }

                if (this.websiteList[websiteIndex].ImageLinkItemList.Count == 0)
                    this.websiteList.RemoveAt(websiteIndex);
            }

            updateDataGrid_ImageLinks();
        }

        private void Button_Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result is null || result.Value == false) return;

            string fullPath = dialog.FileName;
            var urls = new List<string>();
            var displayedUrls = new List<ImageLinkItem>();
            var id = 0;

            foreach (var line in File.ReadLines(fullPath))
            {                
                string cleaned = Regex.Replace(line, @"\t|\n|\r", "");

                if (FieldValidator.isValidHTTP_HTTPS_URL(cleaned))
                {
                    id += 1;
                    urls.Add(cleaned);
                    displayedUrls.Add(new ImageLinkItem
                    {
                        ID = id,
                        Number = null,
                        WebsiteURL = cleaned,
                    });
                }
            }

            if (urls.Count > 0)
            {
                userRequest.ImportedURLs = urls;
                this.Button_Download.IsEnabled = false;
                this.ComboBox_DiffFolderEach.IsEnabled = true;
                this.ComboBox_DiffFolderEach.SelectedIndex = 1;
                this.DataGrid_ImageLink.ItemsSource = displayedUrls;
            }
        }

        private void Button_PDF_Generator_Click(object sender, RoutedEventArgs e)
        {
            var pdfGenerator = new PDFGenerator();
            pdfGenerator.Top = this.Top + 100;
            pdfGenerator.Left = this.Left + 40;
            pdfGenerator.Show();
        }

        private void Button_Setting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingsDialog = new SettingWindow(this);
            settingsDialog.Top = this.Top + 100;
            settingsDialog.Left = this.Left + 40;

            settingsDialog.ShowDialog();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.userSetting.saveUserSettings();
            e.Cancel = false;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FieldValidator.IsNumberText(e.Text);
        }

        private bool AreFieldsValid()
        {
            if (this.userRequest.ImportedURLs != null && this.userRequest.ImportedURLs.Count > 0) return true;
            
            if (this.CheckBox_EnablePageSeriesDownload.IsChecked == false)
            {
                if (FieldValidator.isValidRequiredInput(TextBox_MainURL.Text,
                        TextBox_XPathSelector.Text) == false
                    )
                {
                    System.Windows.MessageBox.Show(
                        "Invalid Required Fields",
                        "Analyze Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }

            }
            else
            {
                if (FieldValidator.isValidRequiredInput(TextBox_MainURL.Text,
                        TextBox_XPathSelector.Text) == false

                    )
                {
                    System.Windows.MessageBox.Show(
                        "Invalid Required Fields",
                        "Analyze Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }


                if (FieldValidator.isValidOptionalInput(
                        TextBox_Page2URL.Text,
                        TextBox_Page3URL.Text,
                        Textbox_FromPage.Text,
                        Textbox_ToPage.Text) == false
                    )
                {
                    System.Windows.MessageBox.Show(
                        "Invalid Optional Fields",
                        "Analyze Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }
            }
            return true;
        }

        private void updateDataGrid_ImageLinks()
        {
            if (this.websiteList is null || this.websiteList.Count == 0)
            {
                resetDataGrid_ImageLinks();
                return;
            }

            this.numberImageLinks = 0;
            int count = 0;
            List<ImageLinkItem> imageLinkList = new List<ImageLinkItem>();

            for (int websiteIndex = 0; websiteIndex < this.websiteList.Count; websiteIndex++)
            {
                ImageLinkItem firstImageLinkOfTheWebsite = new ImageLinkItem(
                    ++count,
                    websiteList[websiteIndex].ImageLinkItemList.Count,
                    websiteList[websiteIndex].Url, 
                    "");
                imageLinkList.Add(firstImageLinkOfTheWebsite);
                
                int imageLinkListIndexForCurWebsite = imageLinkList.Count - 1;
                int numberDownloadedOfCurWebsite = 0;

                for (int imageLinkIndex = 0; imageLinkIndex < websiteList[websiteIndex].ImageLinkItemList.Count; imageLinkIndex++)
                {
                    this.numberImageLinks++;

                    ImageLinkItem imageLinkItemOfTheWebsite = new ImageLinkItem(
                        ++count,
                        imageLinkIndex + 1,
                        websiteList[websiteIndex].Url, 
                        websiteList[websiteIndex].ImageLinkItemList[imageLinkIndex].ImageLink, 
                        websiteList[websiteIndex].ImageLinkItemList[imageLinkIndex].IsDownloaded);
                    imageLinkList.Add(imageLinkItemOfTheWebsite);

                    if (imageLinkItemOfTheWebsite.IsDownloaded) 
                        numberDownloadedOfCurWebsite++;
                }

                if (numberDownloadedOfCurWebsite == websiteList[websiteIndex].ImageLinkItemList.Count)
                    imageLinkList[imageLinkListIndexForCurWebsite].IsDownloaded = true;
            }

            this.DataGrid_ImageLink.ItemsSource = imageLinkList;
            this.ProgressBar_Indicator.Maximum = this.numberImageLinks;
            this.TextBlock_ResultView_NumberOfImageLinks.Text = "Number of Image URLs: " + this.numberImageLinks;
        }

        private void resetDataGrid_ImageLinks()
        {
            List<ImageLinkItem> emtptyDataGrid = new List<ImageLinkItem>();
            emtptyDataGrid.Add(new ImageLinkItem(0, 0, "", "", false));
            this.DataGrid_ImageLink.ItemsSource = emtptyDataGrid;
        }

        private void resetDetailInfomationResultView()
        {
            this.TextBlock_ResultView_NumberOfImageDownloaded.Text = "";
            this.TextBlock_ResultView_ProcessDescription.Text = "";
            this.TextBlock_ResultView_NumberOfImageLinks.Text = "";
            this.Button_FixImageLinks.Visibility = Visibility.Hidden;
            this.Button_ClearDownloadedLinks.Visibility = Visibility.Hidden;
            this.ProgressBar_Indicator.Value = 0;
        }

        public void updateProgressBar_Indicator(int curWorkingPosition)
        {
            if (this.numberImageLinks == 0 || this.websiteList is null || this.websiteList.Count == 0)
            {
                this.ProgressBar_Indicator.Value = this.ProgressBar_Indicator.Maximum;
            }
            else
            {
                this.ProgressBar_Indicator.Value = curWorkingPosition;
            }
        }
    }
}
