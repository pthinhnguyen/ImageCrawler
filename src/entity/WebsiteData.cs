using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_imageCrawler.src.entity
{
    public class WebsiteData
    {
        private string title;
        private string mainURL;
        private string page2URL;
        private string page3URL;
        private int fromPage;
        private int toPage;
        private string xpathSelector;
        private string attribute;
        private List<string> imageLinks;

        public WebsiteData()
        {
            this.title = "";
            this.mainURL = "";
            this.page2URL = "";
            this.page3URL = "";
            this.fromPage = 0;
            this.toPage = 0;
            this.xpathSelector = "";
            this.attribute = "";
            this.imageLinks = new List<string>();
        }

        public WebsiteData(string title, string mainURL, string page2URL, string page3URL, int fromPage, int toPage, string xpathSelector, string attribute, List<string> imageLinks)
        {
            this.title = title ?? throw new ArgumentNullException(nameof(title));
            this.mainURL = mainURL ?? throw new ArgumentNullException(nameof(mainURL));
            this.page2URL = page2URL ?? throw new ArgumentNullException(nameof(page2URL));
            this.page3URL = page3URL ?? throw new ArgumentNullException(nameof(page3URL));
            this.fromPage = fromPage;
            this.toPage = toPage;
            this.xpathSelector = xpathSelector ?? throw new ArgumentNullException(nameof(xpathSelector));
            this.attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            this.imageLinks = imageLinks ?? throw new ArgumentNullException(nameof(imageLinks));
        }

        public WebsiteData(WebsiteData websiteData)
        {
            this.title = websiteData.Title;
            this.mainURL = websiteData.MainURL;
            this.page2URL = websiteData.Page2URL;
            this.page3URL = websiteData.Page3URL;
            this.fromPage = websiteData.FromPage;
            this.toPage = websiteData.ToPage;
            this.xpathSelector = websiteData.XpathSelector;
            this.attribute = websiteData.Attribute;
            this.imageLinks = websiteData.imageLinks;
        }

        public string Title { get => title; set => title = value; }
        public string MainURL { get => mainURL; set => mainURL = value; }
        public string Page2URL { get => page2URL; set => page2URL = value; }
        public string Page3URL { get => page3URL; set => page3URL = value; }
        public int FromPage { get => fromPage; set => fromPage = value; }
        public int ToPage { get => toPage; set => toPage = value; }
        public string XpathSelector { get => xpathSelector; set => xpathSelector = value; }
        public string Attribute { get => attribute; set => attribute = value; }
        public List<string> ImageLinks { get => imageLinks; set => imageLinks = value; }
    }
}
