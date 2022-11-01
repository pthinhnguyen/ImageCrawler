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
        private int id;
        private string downloadLocation;
        private string browserPath;
        private int minImageSizeInByte;
        private int orderStartingNumber;

        public SettingData()
        {
            this.downloadLocation = "";
            this.browserPath = "";
            this.minImageSizeInByte = 0;
            this.OrderStartingNumber = 1;
        }

        public static SettingData loadDefaultSettingData()
        {
            SettingData defaultSettingData = new SettingData();
            defaultSettingData.downloadLocation = KnownFolders.GetPath(KnownFolder.Downloads);
            defaultSettingData.browserPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            defaultSettingData.minImageSizeInByte = 10240;
            defaultSettingData.orderStartingNumber = 1;

            return defaultSettingData;

        }

        public string BrowserPath { get => browserPath; set => browserPath = value; }
        public int MinImageSizeInByte { get => minImageSizeInByte; set => minImageSizeInByte = value; }
        public string DownloadLocation { get => downloadLocation; set => downloadLocation = value; }
        public int Id { get => id; set => id = value; }
        public int OrderStartingNumber { get => orderStartingNumber; set => orderStartingNumber = value; }
    }
}
