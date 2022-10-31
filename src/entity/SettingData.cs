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

        public SettingData()
        {
            this.downloadLocation = "";
            this.browserPath = "";
            this.minImageSizeInByte = 0;
        }

        public SettingData(string downloadLoction, string browserPath, int minImageSizeInByte)
        {
            this.downloadLocation = downloadLoction ?? throw new ArgumentNullException(nameof(downloadLoction));
            this.browserPath = browserPath ?? throw new ArgumentNullException(nameof(browserPath));
            this.minImageSizeInByte = minImageSizeInByte;
        }

        public static SettingData loadDefaultSettingData()
        {
            SettingData defaultSettingData = new SettingData();
            defaultSettingData.downloadLocation = KnownFolders.GetPath(KnownFolder.Downloads);
            defaultSettingData.browserPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
            defaultSettingData.minImageSizeInByte = 10240;

            return defaultSettingData;

        }

        public string BrowserPath { get => browserPath; set => browserPath = value; }
        public int MinImageSizeInByte { get => minImageSizeInByte; set => minImageSizeInByte = value; }
        public string DownloadLocation { get => downloadLocation; set => downloadLocation = value; }
        public int Id { get => id; set => id = value; }
    }
}
