using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_imageCrawler.src.entity
{
    public class UserRequestData
    {
        private string mainURL;
        private string page2URL;
        private string page3URL;
        private int fromPage;
        private int toPage;
        private string xpathSelector;
        private string attribute;

        public UserRequestData()
        {
            this.mainURL = "";
            this.page2URL = "";
            this.page3URL = "";
            this.fromPage = 0;
            this.toPage = 0;
            this.xpathSelector = "";
            this.attribute = "";
        }

        public UserRequestData(UserRequestData userRequestData)
        {
            this.mainURL = userRequestData.mainURL;
            this.page2URL = userRequestData.page2URL;
            this.page3URL = userRequestData.page3URL;
            this.fromPage = userRequestData.FromPage;
            this.toPage = userRequestData.toPage;
            this.xpathSelector = userRequestData.xpathSelector;
            this.attribute = userRequestData.attribute;
        }

        public string MainURL { get => mainURL; set => mainURL = value; }
        public string Page2URL { get => page2URL; set => page2URL = value; }
        public string Page3URL { get => page3URL; set => page3URL = value; }
        public int FromPage { get => fromPage; set => fromPage = value; }
        public int ToPage { get => toPage; set => toPage = value; }
        public string XpathSelector { get => xpathSelector; set => xpathSelector = value; }
        public string Attribute { get => attribute; set => attribute = value; }
    }
}
