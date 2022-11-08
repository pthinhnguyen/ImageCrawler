using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpf_imageCrawler.Resources;

namespace wpf_imageCrawler.src.entity
{
    public class SettingData
    {
        // browser settings
        private string browserPath;
        private bool speedMode;
        private bool useBuitInBrowser;
        private bool silentMode;

        // file settings
        private string downloadLocation;
        private int minImageSizeInByte;
        private int orderStartingNumber;
        private bool createNewFolder;
        private bool diffFolderEach;

        public SettingData()
        {
            this.browserPath = "";
            this.speedMode = false;
            this.useBuitInBrowser = false;
            this.silentMode = false;

            this.downloadLocation = "";
            this.minImageSizeInByte = 0;
            this.OrderStartingNumber = 1;
            this.createNewFolder = true;
            this.diffFolderEach = false;
        }

        public static SettingData loadDefaultSettingData()
        {
            SettingData defaultSettingData = new SettingData();

            defaultSettingData.browserPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            defaultSettingData.speedMode = false;
            defaultSettingData.useBuitInBrowser = false;
            defaultSettingData.silentMode = false;

            defaultSettingData.downloadLocation = KnownFolders.GetPath(KnownFolder.Downloads);
            defaultSettingData.minImageSizeInByte = 10240;
            defaultSettingData.orderStartingNumber = 1;
            defaultSettingData.createNewFolder = true;
            defaultSettingData.diffFolderEach = false;

            return defaultSettingData;
        }
        
        public string BrowserPath { get => browserPath; set => browserPath = value; }
        public bool SpeedMode { get => speedMode; set => speedMode = value; }
        public bool UseBuitInBrowser { get => useBuitInBrowser; set => useBuitInBrowser = value; }
        public bool SilentMode { get => silentMode; set => silentMode = value; }

        public string DownloadLocation { get => downloadLocation; set => downloadLocation = value; }
        public int MinImageSizeInByte { get => minImageSizeInByte; set => minImageSizeInByte = value; }
        public int OrderStartingNumber { get => orderStartingNumber; set => orderStartingNumber = value; }
        public bool CreateNewFolder { get => createNewFolder; set => createNewFolder = value; }
        public bool DiffFolderEach { get => diffFolderEach; set => diffFolderEach = value; }
        
    }
}
