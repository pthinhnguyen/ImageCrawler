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

namespace wpf_imageCrawler
{
    public partial class MainWindow : Window
    {
        private static string downloadLocation = KnownFolders.GetPath(KnownFolder.Downloads);
        private static FolderBrowserDialog dialogFolder = new FolderBrowserDialog();
        private static FieldValidator fieldValidator = new FieldValidator();

        // private SettingData settingData;
        private WebsiteData websiteData;
        private UserSetting userSetting;

        private CancellationTokenSource? cancelTokenAnalyze;
        private CancellationTokenSource? cancelTokenDownload;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;

            this.websiteData = new WebsiteData();
            this.userSetting = UserSetting.Instance;
            int test = 0;
            test++;
           
            this.Button_Download.IsEnabled = false;
            this.TextBox_DonwloadLocation.Text = this.UserSetting.UserSettingData.DownloadLocation;
            this.CheckBox_EnablePageSeriesDownload.IsChecked = false;
            this.TextBox_Page2URL.IsEnabled = false;
            this.TextBox_Page3URL.IsEnabled = false;
            this.Textbox_FromPage.IsEnabled = false;
            this.Textbox_ToPage.IsEnabled = false;
            this.ProgressBar_Indicator.IsIndeterminate = false;
            this.Button_FixImageLinks.Visibility = Visibility.Hidden;
            this.Button_Analyze_Cancel.Visibility = Visibility.Hidden;
            this.Button_Download_Cancel.Visibility = Visibility.Hidden;

            /** Test **/
            
            this.TextBox_XPathSelector.Text = "//div[@class=\"bbImageWrapper  js-lbImage\"]/img";
            this.Textbox_AttributeSelector.Text = "src";
            
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
        }

        private void CheckBox_EnablePageSeriesDownload_Checked(object sender, RoutedEventArgs e)
        {
            this.TextBox_Page2URL.IsEnabled = true;
            this.TextBox_Page3URL.IsEnabled = true;
            this.Textbox_FromPage.IsEnabled = true;
            this.Textbox_ToPage.IsEnabled = true;
        }

        private void CheckBox_EnablePageSeriesDownload_Unchecked(object sender, RoutedEventArgs e)
        {
            this.TextBox_Page2URL.Text = "";
            this.TextBox_Page3URL.Text = "";
            this.Textbox_FromPage.Text = "";
            this.Textbox_ToPage.Text = "";
            
            this.TextBox_Page2URL.IsEnabled = false;
            this.TextBox_Page3URL.IsEnabled = false;
            this.Textbox_FromPage.IsEnabled = false;
            this.Textbox_ToPage.IsEnabled = false;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_Setting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingsDialog = new SettingWindow(this);
            settingsDialog.Top = this.Top + 100;
            settingsDialog.Left = this.Left + 40;

            settingsDialog.Show();
        }

        private async void Button_Download_Click(object sender, RoutedEventArgs e)
        {
            this.resetDetailInfomationResultView();

            if (!this.AreFieldsValid()) return;
            if (this.websiteData.ImageLinks is null || this.websiteData.ImageLinks.Count == 0)
            {
                this.TextBlock_ResultView_ProcessDescription.Text = "NO Image Found to Download";
                return;
            }
            if (this.cancelTokenDownload is not null) this.cancelTokenDownload.Dispose();
            this.cancelTokenDownload = new CancellationTokenSource();

            this.Button_Download.IsEnabled = false;
            this.Button_Analyze.IsEnabled = false;
            this.ProgressBar_Indicator.IsIndeterminate = false;
            this.Button_Download.Visibility = Visibility.Hidden;
            this.Button_Download_Cancel.Visibility = Visibility.Visible;
            this.TextBlock_ResultView_ProcessDescription.Text = "Downloading...";

            int numberDownloadedImage = await Scraper.downloadImages(this.websiteData, this.UserSetting.UserSettingData, this, this.cancelTokenDownload.Token);

            this.Button_Analyze.IsEnabled = true;
            this.Button_Download.IsEnabled = true;
            this.Button_Download.Visibility = Visibility.Visible;
            this.Button_Download_Cancel.Visibility = Visibility.Hidden;
            this.TextBlock_ResultView_ProcessDescription.Text = "Finished Downloading";
            this.TextBlock_ResultView_NumberOfImageLinks.Text = "Number of Image URLs: " + this.websiteData.ImageLinks.Count;
            this.TextBlock_ResultView_NumberOfImageDownloaded.Text = "Number of Downloaded Images: " + numberDownloadedImage;
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
            this.Button_Analyze.Visibility = Visibility.Hidden;
            this.Button_Analyze_Cancel.Visibility = Visibility.Visible;
            this.TextBox_ImageLinks.Text = "";
            this.TextBlock_ResultView_NumberOfImageLinks.Text = "";
            this.TextBlock_ResultView_ProcessDescription.Text = "Getting Image Links...";

            this.websiteData.MainURL = TextBox_MainURL.Text;
            this.websiteData.Page2URL = string.IsNullOrEmpty(TextBox_Page2URL.Text) ? "" : TextBox_Page2URL.Text;
            this.websiteData.Page3URL = string.IsNullOrEmpty(TextBox_Page3URL.Text) ? "" : TextBox_Page3URL.Text;
            this.websiteData.FromPage = string.IsNullOrEmpty(Textbox_FromPage.Text) ? 0 : Int16.Parse(Textbox_FromPage.Text);
            this.websiteData.ToPage = string.IsNullOrEmpty(Textbox_ToPage.Text) ? 0 : Int16.Parse(Textbox_ToPage.Text);
            this.websiteData.XpathSelector = string.IsNullOrEmpty(TextBox_XPathSelector.Text) ? "" : TextBox_XPathSelector.Text;
            this.websiteData.Attribute = string.IsNullOrEmpty(Textbox_AttributeSelector.Text) ? "" : Textbox_AttributeSelector.Text;

            if (this.CheckBox_EnablePageSeriesDownload.IsChecked == true)
                (this.websiteData.Title, this.websiteData.ImageLinks) = await Scraper.filterMutiplePageByXPath(this.websiteData, this.userSetting.UserSettingData, this.cancelTokenAnalyze.Token);
            else
                (this.websiteData.Title, this.websiteData.ImageLinks) = await Scraper.filterSinglePageByXPath(this.websiteData, this.userSetting.UserSettingData, this.cancelTokenAnalyze.Token);

            this.updateTextBox_ImageLinks();

            this.ProgressBar_Indicator.IsIndeterminate = false;
            this.Button_FixImageLinks.Visibility = Visibility.Visible;
            this.Button_Analyze.Visibility = Visibility.Visible;
            this.Button_Analyze_Cancel.Visibility = Visibility.Hidden;
            this.Button_Analyze.IsEnabled = true;
            this.Button_Download.IsEnabled = true;
            this.TextBlock_ResultView_ProcessDescription.Text = "Finished Analyzing";
            this.TextBlock_ResultView_NumberOfImageLinks.Text = "Number of Image URLs: " + ((this.websiteData.ImageLinks is not null) ? this.websiteData.ImageLinks.Count : 0);
            this.ProgressBar_Indicator.Minimum = 0;
            this.ProgressBar_Indicator.Maximum = ((this.websiteData.ImageLinks is not null) ? this.websiteData.ImageLinks.Count : 0);
        }

        private void Button_FixImageLinks_Click(object sender, RoutedEventArgs e)
        {
            List<string> tmp = websiteData.ImageLinks;

            if (websiteData.ImageLinks is not null && websiteData.ImageLinks.Count > 0)
            {
                for (int i = 0; i < websiteData.ImageLinks.Count; i++)
                {
                    websiteData.ImageLinks[i] = fieldValidator.fixURL(websiteData.ImageLinks[i]);
                }
            }
            this.updateTextBox_ImageLinks();
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

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var t = Task.Run(async () =>
            {
                try
                {
                    // this.userSetting.saveUserSettings();
                    await Scraper.disposeBrowser().WaitAsync(TimeSpan.FromSeconds(5));
                }
                finally
                {
                    e.Cancel = false;
                }
            });
        }

        private bool AreFieldsValid()
        {
            if (this.CheckBox_EnablePageSeriesDownload.IsChecked == false)
            {
                if (fieldValidator.isValidRequiredInput(TextBox_MainURL.Text,
                        TextBox_XPathSelector.Text) == false
                    )
                {
                    System.Windows.MessageBox.Show(
                        "Invalid Required Fields",
                        "Download Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }

            }
            else
            {
                if (fieldValidator.isValidRequiredInput(TextBox_MainURL.Text,
                        TextBox_XPathSelector.Text) == false

                    )
                {
                    System.Windows.MessageBox.Show(
                        "Invalid Required Fields",
                        "Download Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }


                if (fieldValidator.isValidOptionalInput(
                        TextBox_Page2URL.Text,
                        TextBox_Page3URL.Text,
                        Textbox_FromPage.Text,
                        Textbox_ToPage.Text) == false
                    )
                {
                    System.Windows.MessageBox.Show(
                        "Invalid Optional Fields",
                        "Download Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }
            }
            return true;
        }

        private void updateTextBox_ImageLinks()
        {
            if (this.websiteData.ImageLinks is not null && this.websiteData.ImageLinks.Count > 0)
            {
                string formattedOuputString = "";
                foreach (string imageLink in this.websiteData.ImageLinks)
                    formattedOuputString = formattedOuputString + imageLink + "\n";
                this.TextBox_ImageLinks.Text = formattedOuputString;
            }
            else
            {
                this.TextBox_ImageLinks.Text = "No Content\n";
            }
        }

        private void resetDetailInfomationResultView()
        {
            this.TextBlock_ResultView_NumberOfImageDownloaded.Text = "";
            this.TextBlock_ResultView_ProcessDescription.Text = "";
            this.TextBlock_ResultView_NumberOfImageLinks.Text = "";
            this.ProgressBar_Indicator.Value = 0;
        }

        public void updateProgressBar_Indicator(int curWorkingPosition)
        {
            if (this.websiteData.ImageLinks is not null && this.websiteData.ImageLinks.Count > 0)
            {
                this.ProgressBar_Indicator.Value = curWorkingPosition;
            }
        }
    }
}
