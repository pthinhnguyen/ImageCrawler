using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace wpf_imageCrawler.src.controller
{
    public static class FieldValidator
    {

        public static bool isValidAbsoluteURL(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public static bool isValidURL(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
        }

        public static bool isValidPathSelector(string path)
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

        public static bool isValidRequiredInput(string url, string pathSelector)
        {
            if (isValidAbsoluteURL(url) && isValidPathSelector(pathSelector)) 
                return true;
            return false;
        }

        public static bool isValidOptionalInput(string page2URL, string page3URL, string fromPage, string toPage)
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

            if (isValidAbsoluteURL(page2URL) && isValidAbsoluteURL(page3URL))
                return true;
            
            return false;
        }

        public static string fixURL(string url)
        {
            if (isValidURL(url))
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

        public static bool IsNumberText(string text)
        {
            Regex regex = new Regex("[^0-9]+");
            return regex.IsMatch(text);
        }
    }
}
