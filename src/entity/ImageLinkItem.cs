using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_imageCrawler.src.entity
{
    public class ImageLinkItem
    {
        public int ID { get; set; }
        public int Number { get; set; }
        public string WebsiteURL { get; set; }
        public string ImageLink { get; set; }
        public bool IsDownloaded { get; set; }

        public ImageLinkItem()
        {
            this.ID = 0;
            this.Number = 0;
            this.WebsiteURL = "";
            this.ImageLink = "";
            this.IsDownloaded = false;
        }

        public ImageLinkItem(int number, string websiteURL, string imageLink, bool isDownloaded = false)
        {
            this.ID = 0;
            this.Number = number;
            this.WebsiteURL = websiteURL;
            this.ImageLink = imageLink;
            this.IsDownloaded = isDownloaded;
        }

        public ImageLinkItem(int id, int number, string websiteURL, string imageLink, bool isDownloaded = false)
        {
            this.ID = id;
            this.Number = number;
            this.WebsiteURL = websiteURL;
            this.ImageLink = imageLink;
            this.IsDownloaded = isDownloaded;
        }
    }
}
