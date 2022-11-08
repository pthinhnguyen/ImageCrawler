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
using System.Windows.Shapes;

using wpf_imageCrawler.src.entity;

using System.Windows.Forms;
using wpf_imageCrawler.src.controller;

namespace wpf_imageCrawler.GUI
{
    public partial class SettingWindow : Window
    {
        private static OpenFileDialog dialogFile = new OpenFileDialog();
        private MainWindow mainWindow;

        public SettingWindow()
        {
            this.mainWindow = new MainWindow();
            InitializeComponent();
        }

        public SettingWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();

            // Browser
            this.ComboBox_BuiltInBrowser.SelectedIndex = (this.mainWindow.UserSetting.UserSettingData.UseBuitInBrowser) ? 1 : 0;
            this.TextBox_BrowserLocation.Text = this.mainWindow.UserSetting.UserSettingData.BrowserPath;
            this.ComboBox_SilentMode.SelectedIndex = (this.mainWindow.UserSetting.UserSettingData.SilentMode) ? 1 : 0;
            this.ComboBox_SpeedMode.SelectedIndex = (this.mainWindow.UserSetting.UserSettingData.SpeedMode) ? 1 : 0;

            // Files
            this.TextBox_MinimumImageSize.Text = this.mainWindow.UserSetting.UserSettingData.MinImageSizeInByte.ToString();
            this.TextBox_OrderStartingNumber.Text = this.mainWindow.UserSetting.UserSettingData.OrderStartingNumber.ToString();

            if (this.mainWindow.UserSetting.UserSettingData.UseBuitInBrowser)
            {
                this.ComboBox_SilentMode.IsEnabled = true;
                this.TextBox_BrowserLocation.IsEnabled = false;
                this.Button_BrowserFileBrowse.IsEnabled = false;
                this.TextBlock_Hint_BuiltInBrowser.Visibility = Visibility.Hidden;
            }
            else
            {
                this.ComboBox_SilentMode.IsEnabled = false;
                this.ComboBox_SilentMode.SelectedIndex = 0;
                this.TextBox_BrowserLocation.IsEnabled = true;
                this.Button_BrowserFileBrowse.IsEnabled = true;
                this.TextBlock_Hint_BuiltInBrowser.Visibility = Visibility.Visible;
            }
        }

        private void Button_BrowserFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            string resultSelectedPath = this.TextBox_BrowserLocation.Text;

            dialogFile.Filter = "Web Browser executable (.exe)|*.exe";
            dialogFile.FilterIndex = 1;
            dialogFile.Multiselect = false;

            if (dialogFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                resultSelectedPath = dialogFile.FileName;
            }

            TextBox_BrowserLocation.Text = resultSelectedPath;
        }

        private void Button_SettingCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_SettingSaveExit_Click(object sender, RoutedEventArgs e)
        {
            this.mainWindow.UserSetting.UserSettingData.UseBuitInBrowser = (this.ComboBox_BuiltInBrowser.SelectedIndex == 1) ? true : false;
            this.mainWindow.UserSetting.UserSettingData.BrowserPath = this.TextBox_BrowserLocation.Text;
            this.mainWindow.UserSetting.UserSettingData.SilentMode = (this.ComboBox_SilentMode.SelectedIndex == 1) ? true : false;
            this.mainWindow.UserSetting.UserSettingData.SpeedMode = (this.ComboBox_SpeedMode.SelectedIndex == 1) ? true : false;

            try
            {
                this.mainWindow.UserSetting.UserSettingData.MinImageSizeInByte = Int32.Parse(this.TextBox_MinimumImageSize.Text);
            }
            catch
            {
                this.mainWindow.UserSetting.UserSettingData.MinImageSizeInByte = this.mainWindow.UserSetting.DefaultUserSettingData.MinImageSizeInByte;
            }

            try
            {
                this.mainWindow.UserSetting.UserSettingData.OrderStartingNumber = Int32.Parse(this.TextBox_OrderStartingNumber.Text);
            }
            catch
            {
                this.mainWindow.UserSetting.UserSettingData.OrderStartingNumber = this.mainWindow.UserSetting.DefaultUserSettingData.OrderStartingNumber;
            }

            this.mainWindow.UserSetting.UserSettingData = this.mainWindow.UserSetting.saveUserSettings();
            Close();
        }

        private void Button_SettingReset_Click(object sender, RoutedEventArgs e)
        {
            this.ComboBox_BuiltInBrowser.SelectedIndex = (this.mainWindow.UserSetting.DefaultUserSettingData.UseBuitInBrowser) ? 1 : 0;
            this.TextBox_BrowserLocation.Text = this.mainWindow.UserSetting.DefaultUserSettingData.BrowserPath;
            this.ComboBox_SilentMode.SelectedIndex = (this.mainWindow.UserSetting.DefaultUserSettingData.SilentMode) ? 1 : 0;
            this.ComboBox_SpeedMode.SelectedIndex = (this.mainWindow.UserSetting.DefaultUserSettingData.SpeedMode) ? 1 : 0;

            this.TextBox_MinimumImageSize.Text = this.mainWindow.UserSetting.DefaultUserSettingData.MinImageSizeInByte.ToString();
            this.TextBox_OrderStartingNumber.Text = this.mainWindow.UserSetting.DefaultUserSettingData.OrderStartingNumber.ToString();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FieldValidator.IsNumberText(e.Text);
        }

        private void ComboBox_BuiltInBrowser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ComboBox_BuiltInBrowser.SelectedIndex == 0)
            {
                this.TextBox_BrowserLocation.IsEnabled = true;
                this.Button_BrowserFileBrowse.IsEnabled = true;
                this.TextBlock_Hint_BuiltInBrowser.Visibility = Visibility.Visible;
                this.ComboBox_SilentMode.IsEnabled = false;
                this.ComboBox_SilentMode.SelectedIndex = 0;
            }
            else
            {
                this.TextBox_BrowserLocation.IsEnabled = false;
                this.Button_BrowserFileBrowse.IsEnabled = false;
                this.TextBlock_Hint_BuiltInBrowser.Visibility = Visibility.Hidden;
                this.ComboBox_SilentMode.IsEnabled = true;
            }
        }

        private void ComboBox_SilentMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ;
        }

        private void ComboBox_FastMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ;
        }
    }
}
