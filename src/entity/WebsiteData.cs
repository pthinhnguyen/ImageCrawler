using System;
using System.Collections.Generic;

namespace wpf_imageCrawler.src.entity
{
    public class WebsiteData
    {
        private string title;
        private string url;
        private List<ImageLinkItem> imageLinks;

        public string Title { get => title; set => title = value; }
        public string Url { get => url; set => url = value; }
        public List<ImageLinkItem> ImageLinkItemList { get => imageLinks; set => imageLinks = value; }

        public WebsiteData()
        {
            this.title = "";
            this.url = "";
            this.imageLinks = new List<ImageLinkItem>();
        }

        public WebsiteData(string title, string url, List<ImageLinkItem> imageLinks)
        {
            this.title = title ?? throw new ArgumentNullException(nameof(title));
            this.url = url ?? throw new ArgumentNullException(nameof(url));
            this.imageLinks = imageLinks ?? throw new ArgumentNullException(nameof(imageLinks));
        }

        public WebsiteData(WebsiteData websiteData)
        {
            this.title = websiteData.Title;
            this.url = websiteData.Url;
            this.imageLinks = websiteData.ImageLinkItemList;
        }
    }
}
