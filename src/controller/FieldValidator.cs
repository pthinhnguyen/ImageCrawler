using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace wpf_imageCrawler.src.controller
{
    internal class FieldValidator
    {
        public FieldValidator()
        {
        }

        public bool isValidAbsoluteURL(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public bool isValidURL(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
        }

        public bool isValidPathSelector(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return false;

                XPathExpression.Compile(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool isValidRequiredInput(string url, string pathSelector)
        {
            if (this.isValidAbsoluteURL(url) && this.isValidPathSelector(pathSelector)) 
                return true;
            return false;
        }

        public bool isValidOptionalInput(string page2URL, string page3URL, string fromPage, string toPage)
        {
            if (string.IsNullOrEmpty(fromPage) || string.IsNullOrEmpty(toPage))
                return false;

            try
            {
                int _fromPage = Int16.Parse(fromPage);
                int _toPage = Int16.Parse(toPage);

                if (_fromPage < 1 || _toPage < 3 || _fromPage > _toPage)
                    return false;
            }
            catch
            {
                return false;
            }

            if (this.isValidAbsoluteURL(page2URL) && this.isValidAbsoluteURL(page3URL))
                return true;
            
            return false;
        }

        public string fixURL(string url)
        {
            if (this.isValidURL(url))
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Relative))
                {
                    if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        if (url.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                            url = "https:" + url;
                        else if (url.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                            url = "https:/" + url;
                        else
                            url = "https://" + url;
                    }
                }
            }

            return url;
        }
    }
}
