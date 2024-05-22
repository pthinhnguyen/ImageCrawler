using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wpf_imageCrawler.src.entity;
using HtmlAgilityPack;
using wpf_imageCrawler.Resources;
using wpf_imageCrawler.src.controller;
using System.Threading;

namespace wpf_imageCrawler.src.service
{
    public class Scraper
    {
        private static async Task<string> getFullHTML(string url, SettingData userSettingData, CancellationToken token)
        {
            string result = "";
            if (string.IsNullOrEmpty(url) || userSettingData is null) 
                return result;
            
            PuppeteerSharpBrowser puppeteerSharpBrowser = new PuppeteerSharpBrowser();

            try
            {
                if (userSettingData.SpeedMode)
                    result = await puppeteerSharpBrowser.loadPageContentFast(url, userSettingData, token);
                else
                    result = await puppeteerSharpBrowser.loadPageContentNormal(url, userSettingData, token);

                if (token.IsCancellationRequested) result = "";
            } 
            catch (Exception ex ) 
            { 
                result = ""; 
            }
                
            return result;
        }

        private static async Task<WebsiteData?> filterPageByXPath(UserRequestData userRequest, SettingData userSettingData, CancellationToken token)
        {
            if (string.IsNullOrEmpty(userRequest.MainURL) 
                || string.IsNullOrEmpty(userRequest.XpathSelector) 
                || userRequest.Attribute is null) return null;

            string fullHTML = "";
            WebsiteData resultWebsite = new WebsiteData();
            fullHTML = await getFullHTML(userRequest.MainURL, userSettingData, token);

            if (string.IsNullOrEmpty(fullHTML)) return null;

            try
            {
                string title = "";
                List<ImageLinkItem> imageLinkItemList = new List<ImageLinkItem>();
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(fullHTML);
                HtmlNodeCollection? imageLinkNodes = null;
                HtmlNode titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");

                // get website title
                title = (titleNode is not null) ? titleNode.InnerText : "No Title";

                // get urls
                imageLinkNodes = htmlDoc.DocumentNode.SelectNodes(userRequest.XpathSelector);
                
                if (imageLinkNodes is not null && imageLinkNodes.Count > 0)
                {
                    int orderStartingNumber = userSettingData.OrderStartingNumber;
                    foreach (HtmlNode imageLinkNode in imageLinkNodes)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        try
                        {
                            if (userRequest.Attribute is null) 
                                throw new ArgumentException("NULL Exception: User Request Attribute");
                            
                            if (imageLinkNode.Attributes[userRequest.Attribute] is not null)
                            {
                                ImageLinkItem imageLinkItem = new ImageLinkItem(
                                    orderStartingNumber, 
                                    userRequest.MainURL, 
                                    imageLinkNode.Attributes[userRequest.Attribute].Value,
                                    false);
                                imageLinkItemList.Add(imageLinkItem);
                            }
                            else
                            {
                                ImageLinkItem imageLinkItem = new ImageLinkItem(
                                    orderStartingNumber, 
                                    userRequest.MainURL, 
                                    imageLinkNode.OuterHtml,
                                    false);
                                imageLinkItemList.Add(imageLinkItem);
                            }
                                
                        }
                        catch
                        {
                            imageLinkItemList = new List<ImageLinkItem>();
                            break;
                        }
                        orderStartingNumber++;
                    }
                }
                else
                {
                    imageLinkItemList = new List<ImageLinkItem>();
                }

                // generate result
                resultWebsite.Url = userRequest.MainURL;
                resultWebsite.Title = title;
                resultWebsite.ImageLinkItemList = imageLinkItemList;
            }
            catch
            {
                return null;
            }

            return resultWebsite;
        }

        public static async Task<List<WebsiteData>?> analyzeSingleWebsite(UserRequestData userRequest, SettingData userSettingData, CancellationToken token)
        {
            WebsiteData? resultWebsiteData = await filterPageByXPath(userRequest, userSettingData, token);
            
            if (resultWebsiteData is null) 
                return null;

            List<WebsiteData> resultWebsiteList = new List<WebsiteData>();
            resultWebsiteList.Add(resultWebsiteData);

            return resultWebsiteList;
        }

        public static async Task<List<WebsiteData>?> analyzeMutipleWebsites(UserRequestData userRequest, SettingData userSettingData, CancellationToken token)
        {
            if (string.IsNullOrEmpty(userRequest.Page2URL) || string.IsNullOrEmpty(userRequest.Page3URL)) 
                return null;
            
            if (userRequest.FromPage <= 0 || userRequest.ToPage <= 0 || userRequest.FromPage > userRequest.ToPage) 
                return null;

            List<string>? workingURLList = generateWorkingURLs(
                userRequest.MainURL, 
                userRequest.Page2URL, 
                userRequest.Page3URL, 
                userRequest.FromPage, 
                userRequest.ToPage);
            
            if (workingURLList is null || workingURLList.Count == 0) return null;

            List<WebsiteData> resultWebsiteList = new List<WebsiteData>();
            
            foreach (string workingURL in workingURLList)
            {
                if (token.IsCancellationRequested)
                    break;

                UserRequestData userRequestForEachWorkingURL = new UserRequestData();
                userRequestForEachWorkingURL.MainURL = workingURL;
                userRequestForEachWorkingURL.XpathSelector = userRequest.XpathSelector;
                userRequestForEachWorkingURL.Attribute = userRequest.Attribute;

                WebsiteData? resultWebsiteData = await filterPageByXPath(
                    userRequestForEachWorkingURL, 
                    userSettingData, 
                    token);
                
                if (resultWebsiteData is not null) resultWebsiteList.Add(resultWebsiteData);
            }

            return resultWebsiteList;
        }

        public static async Task<List<WebsiteData>?> analyzeImportedWebsites(UserRequestData userRequest, SettingData userSettingData, CancellationToken token)
        {
            var result = new List<WebsiteData>();
            if (userRequest.ImportedURLs is null || userRequest.ImportedURLs.Count == 0) return null;
            foreach(var url in userRequest.ImportedURLs)
            {
                if (token.IsCancellationRequested) break;
                UserRequestData userRequestForEachWorkingURL = new UserRequestData();
                userRequestForEachWorkingURL.MainURL = url;
                userRequestForEachWorkingURL.XpathSelector = userRequest.XpathSelector;
                userRequestForEachWorkingURL.Attribute = userRequest.Attribute;

                WebsiteData? resultWebsiteData = await filterPageByXPath(
                    userRequestForEachWorkingURL,
                    userSettingData,
                    token);

                if (resultWebsiteData is not null) result.Add(resultWebsiteData);
            }
            return result;
        }

        public static async Task<int> downloadImages(List<WebsiteData>? websiteList, SettingData userSettingData, MainWindow curViewInstance, CancellationToken token)
        {
            if (websiteList is null || websiteList.Count == 0)
            {
                try
                {
                    updateView_ProcessBar((int)curViewInstance.ProgressBar_Indicator.Maximum, curViewInstance);
                } catch { ; }
                return 0;
            }

            // Create destination directory
            string completeAbsolutePath = "";

            if (userSettingData.CreateNewFolder) 
                completeAbsolutePath = FilesManagement.CreateDirectoryIfNotExist(userSettingData.DownloadLocation, websiteList[0].Title);
            else 
                completeAbsolutePath = FilesManagement.CreateDirectoryIfNotExist(userSettingData.DownloadLocation);
            
            if (string.IsNullOrEmpty(completeAbsolutePath)) return 0;

            // Downloading Images
            ImageDownloader downloader = new ImageDownloader();
            int numberImageProceeded = 0;
            int numberImageDownloaded = 0;
            int orderStartingNumber = userSettingData.OrderStartingNumber;
            
            // foreach (WebsiteData website in websiteList)
            for (int websiteIndex = 0; websiteIndex < websiteList.Count; websiteIndex++)
            {
                if (token.IsCancellationRequested) return numberImageDownloaded;

                if (userSettingData.DiffFolderEach)
                {
                    completeAbsolutePath = FilesManagement.CreateDirectoryIfNotExist(userSettingData.DownloadLocation, websiteList[websiteIndex].Title);
                    orderStartingNumber = userSettingData.OrderStartingNumber;
                }

                // foreach (ImageLinkItem imageLinkItem in websiteList[websiteIndex].ImageLinkItemList)
                for (int imageLinkItemIndex = 0; imageLinkItemIndex < websiteList[websiteIndex].ImageLinkItemList.Count; imageLinkItemIndex++)
                {
                    if (token.IsCancellationRequested) return numberImageDownloaded;

                    if (string.IsNullOrEmpty(completeAbsolutePath)) 
                        break;

                    ++numberImageProceeded;
                    if (FieldValidator.isValidHTTP_HTTPS_URL(websiteList[websiteIndex].ImageLinkItemList[imageLinkItemIndex].ImageLink))
                    {
                        bool isDownloaded = await downloader.DownloadImageAsync(completeAbsolutePath, websiteList[websiteIndex].ImageLinkItemList[imageLinkItemIndex].ImageLink, orderStartingNumber, userSettingData.MinImageSizeInByte);
                        if (isDownloaded)
                        {
                            websiteList[websiteIndex].ImageLinkItemList[imageLinkItemIndex].IsDownloaded = true;
                            numberImageDownloaded++;
                            orderStartingNumber++;
                        }
                    }
                    updateView_ProcessBar(numberImageProceeded, curViewInstance);
                }
            }

            // clean up
            downloader.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return numberImageDownloaded;
        }

        private static (bool, string, string, int) getMutiplePageIndicator(string page2URL, string page3URL)
        {
            string front = "", back = "";
            int pageNumberIndicatorForPage2 = 0;

            if (string.IsNullOrEmpty(page2URL) || string.IsNullOrEmpty(page3URL) || page2URL.Length != page3URL.Length || page2URL.Equals(page3URL))
                return (false, front, back, pageNumberIndicatorForPage2);

            List<int> pageIndicatorIndices = new List<int>();
                         
            for (int curIdx = 0; curIdx < page2URL.Length; curIdx++)
            {
                if ((page2URL[curIdx] != page3URL[curIdx]))
                    pageIndicatorIndices.Add(curIdx);
            }

            if (pageIndicatorIndices.Count != 1) return (false, front, back, pageNumberIndicatorForPage2);

            front = page2URL.Substring(0, pageIndicatorIndices[0]);
            if (pageIndicatorIndices[0] == page2URL.Length - 1)
                back = "";
            else
                back = page2URL.Substring(pageIndicatorIndices[0] + 1, (page2URL.Length - 1) - (pageIndicatorIndices[0]));

            try
            {
                pageNumberIndicatorForPage2 = Int16.Parse(page2URL.Substring(pageIndicatorIndices[0], 1));
                return (true, front, back, pageNumberIndicatorForPage2);
            }
            catch
            {
                return (false, front, back, pageNumberIndicatorForPage2);
            }
        }

        private static List<String>? generateWorkingURLs(string mainPageURL, string page2URL, string page3URL, int fromPage, int toPage) 
        {
            if (fromPage <= 0 || toPage <= 0) return null;

            bool doable = false;
            string urlFrontPart = "";
            string urlBackPart = "";
            int pageNumberIndicatorForPage2 = 2;
            int pageNumberBuffer = 0;
            List<String> resultURLList = new List<string>();
            
            (doable, urlFrontPart, urlBackPart, pageNumberIndicatorForPage2) = getMutiplePageIndicator(page2URL, page3URL);
            
            if (doable == false) return null;

            pageNumberBuffer = 2 - pageNumberIndicatorForPage2;

            if (fromPage == 1)
            {
                resultURLList.Add(mainPageURL);
                ++fromPage;
            }

            for(int curPageNumber = fromPage; curPageNumber <= toPage; curPageNumber++)
            {
                int pageNumber = curPageNumber + pageNumberBuffer;
                string curPageURL = urlFrontPart + curPageNumber.ToString() + urlBackPart;
                resultURLList.Add(curPageURL);
            }

            return resultURLList;
        }

        private static void updateView_ProcessBar(int curWorkingPosition, MainWindow curViewInstance)
        {
            curViewInstance.updateProgressBar_Indicator(curWorkingPosition);
        }
    }
}
