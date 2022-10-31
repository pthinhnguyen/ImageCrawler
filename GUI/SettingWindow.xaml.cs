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

            this.textBox_browserLocation.Text = this.mainWindow.UserSetting.UserSettingData.BrowserPath;
            this.textBox_minimumImageSize.Text = this.mainWindow.UserSetting.UserSettingData.MinImageSizeInByte.ToString();
        }

        private void button_browserFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            string resultSelectedPath = this.textBox_browserLocation.Text;

            dialogFile.Filter = "Web Browser executable (.exe)|*.exe";
            dialogFile.FilterIndex = 1;
            dialogFile.Multiselect = false;

            if (dialogFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                resultSelectedPath = dialogFile.FileName;
            }

            textBox_browserLocation.Text = resultSelectedPath;
        }

        private void button_setting_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void button_setting_save_close_Click(object sender, RoutedEventArgs e)
        {
            this.mainWindow.UserSetting.UserSettingData.BrowserPath = this.textBox_browserLocation.Text;
            try
            {
                this.mainWindow.UserSetting.UserSettingData.MinImageSizeInByte = Int32.Parse(this.textBox_minimumImageSize.Text);
            }
            catch
            {
                this.mainWindow.UserSetting.UserSettingData.MinImageSizeInByte = this.mainWindow.UserSetting.DefaultUserSettingData.MinImageSizeInByte;
            }
            finally
            {
                this.mainWindow.UserSetting.UserSettingData = this.mainWindow.UserSetting.saveUserSettings();
            }
            Close();
        }

        private void button_setting_reset_Click(object sender, RoutedEventArgs e)
        {
            this.textBox_browserLocation.Text = this.mainWindow.UserSetting.DefaultUserSettingData.BrowserPath;
            this.textBox_minimumImageSize.Text = this.mainWindow.UserSetting.DefaultUserSettingData.MinImageSizeInByte.ToString();
        }
    }
}
