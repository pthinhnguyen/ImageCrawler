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

            this.TextBox_BrowserLocation.Text = this.mainWindow.UserSetting.UserSettingData.BrowserPath;
            this.TextBox_MinimumImageSize.Text = this.mainWindow.UserSetting.UserSettingData.MinImageSizeInByte.ToString();
            this.TextBox_OrderStartingNumber.Text = this.mainWindow.UserSetting.UserSettingData.OrderStartingNumber.ToString();
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

        private void button_setting_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void button_setting_save_close_Click(object sender, RoutedEventArgs e)
        {
            this.mainWindow.UserSetting.UserSettingData.BrowserPath = this.TextBox_BrowserLocation.Text;
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

        private void button_setting_reset_Click(object sender, RoutedEventArgs e)
        {
            this.TextBox_BrowserLocation.Text = this.mainWindow.UserSetting.DefaultUserSettingData.BrowserPath;
            this.TextBox_MinimumImageSize.Text = this.mainWindow.UserSetting.DefaultUserSettingData.MinImageSizeInByte.ToString();
            this.TextBox_OrderStartingNumber.Text = this.mainWindow.UserSetting.DefaultUserSettingData.OrderStartingNumber.ToString();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FieldValidator.IsNumberText(e.Text);
        }
    }
}
