using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.XPath;
using static System.Net.Mime.MediaTypeNames;

namespace wpf_imageCrawler.src.controller
{
    public static class FieldValidator
    {

        private static bool containsDomainExtension(string url)
        {
            url = url.ToLower();
            if (url.Contains(".com")) return true;
            else if (url.Contains(".net")) return true;
            else if (url.Contains(".org")) return true;
            else if (url.Contains(".co")) return true;
            else if (url.Contains(".edu")) return true;
            else if (url.Contains(".us")) return true;
            else if (url.Contains(".me")) return true;
            else if (url.Contains(".cn")) return true;
            else if (url.Contains(".uk")) return true;
            else if (url.Contains(".de")) return true;
            else if (url.Contains(".ly")) return true;
            else if (url.Contains(".vn")) return true;
            else if (url.Contains(".ru")) return true;
            else if (url.Contains(".gov")) return true;
            else return false;
        }


        public static bool isValidAbsoluteURL(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public static bool isValidRelativeURL(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            
            if (!containsDomainExtension(url)) 
                 return false;
            
            return Uri.IsWellFormedUriString(url, UriKind.Relative);
        }

        public static bool isValidAbsoluteOrRelative(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
        }

        public static bool isValidHTTP_HTTPS_URL(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            Uri? uriResult;
            bool result = false;
            
            try
            {
                result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            } catch { result = false; }

            return result;
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
            if (isValidHTTP_HTTPS_URL(url) && isValidPathSelector(pathSelector)) 
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

            if (isValidHTTP_HTTPS_URL(page2URL) && isValidHTTP_HTTPS_URL(page3URL))
                return true;
            else
                return false;
        }

        public static string fixURL(string url)
        {
            if (isValidHTTP_HTTPS_URL(url) || isValidAbsoluteURL(url))
            {
                ;
            }
            else if (isValidRelativeURL(url))
            {
                string fixedURL = addMissingProtocolIndicator(url);
                if (isValidHTTP_HTTPS_URL(fixedURL)) url = fixedURL;
                else url = "";
            }
            else
            {
                string fixedURL = attemptToGetTheImageLink(url);
                if (isValidHTTP_HTTPS_URL(fixedURL)) url = fixedURL;
                else url = "";
            }

            return url;
        }

        public static bool IsNumberText(string text)
        {
            Regex regex = new Regex("[^0-9]+");
            return regex.IsMatch(text);
        }

        private static string addMissingProtocolIndicator(string url)
        {
            string result = "";
            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    if (url.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                        result = "https:" + url;
                    else if (url.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                        result = "https:/" + url;
                    else
                        result = "https://" + url;
                }
            }
            return result;
        }

        private static string attemptToGetTheImageLink(string url)
        {
            string result = "";
            string[] inputs = url.Split(null); 
            foreach (string input in inputs)
            {
                Regex regex = new Regex("\"(.*?)\"");

                var matches = regex.Matches(input);
                if (matches.Count > 0)
                {
                    string formattedInput = matches[0].Groups[1].Value;
                    /*formattedInput = input.Replace("\"", "");
                    formattedInput = formattedInput.Trim('\'');*/
                    formattedInput = formattedInput.Trim();

                    if (isValidAbsoluteOrRelative(formattedInput))
                    {
                        if (isValidRelativeURL(formattedInput))
                            result = addMissingProtocolIndicator(formattedInput);
                        else 
                            result = formattedInput;

                        if (isValidHTTP_HTTPS_URL(result)) 
                            break;
                        else 
                            result = "";
                    }
                }
                else
                {
                    ;
                }
            }

            return result;
        }
    }
}
