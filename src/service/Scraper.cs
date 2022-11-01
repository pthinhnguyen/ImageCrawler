using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using wpf_imageCrawler.src.entity;
using PuppeteerSharp;
using HtmlAgilityPack;
using wpf_imageCrawler.Resources;
using System.Windows.Controls;
using wpf_imageCrawler.src.controller;
using System.Security.Policy;
using System.Threading;
using System.Xml;
using Microsoft.VisualBasic.Logging;

namespace wpf_imageCrawler.src.service
{
    public class Scraper
    {
        public static async Task<string> getFullHTML(string url, SettingData userSetting, CancellationToken token)
        {
            string result = "";
            for (int attempt = 0; attempt < 3; attempt++)
            {
                result = "";

                // Initialize Browser
                using IBrowser BrowserIntance = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = false,
                    ExecutablePath = userSetting.BrowserPath
                });
                using IPage PageInstance = await BrowserIntance.NewPageAsync();

                // Get the page Content
                try
                {
                    if (BrowserIntance is null || PageInstance is null) return result;

                    // Access The Website
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromSeconds(10);

                        NavigationOptions opitions = new NavigationOptions();
                        opitions.WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded };
                        opitions.Timeout = (int)timeout.TotalMilliseconds;

                        await PageInstance.GoToAsync(url, opitions).WaitAsync(token);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                    
                    // Scroll to the bottom
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromSeconds(5);
                        await PageInstance.EvaluateExpressionAsync("window.scrollBy(0,window.document.body.scrollHeight)").WaitAsync(timeout, token);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }

                    // Wait to load the rest of the website
                    try
                    {
                        
                        int timeout = (int)TimeSpan.FromSeconds(7).TotalMilliseconds;
                        await PageInstance.WaitForTimeoutAsync(timeout).WaitAsync(token);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }

                    // Get the content
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromSeconds(5);
                        result = await PageInstance.GetContentAsync().WaitAsync(timeout, token);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    attempt = 3;
                }
                catch (OperationCanceledException)
                {
                    result = "";
                    attempt = 3;
                }
                catch (Exception ex)
                {
                    result = "";
                }
                finally
                {
                    try
                    {
                        await PageInstance.CloseAsync().WaitAsync(TimeSpan.FromSeconds(5));
                        PageInstance.Dispose();
                    } catch { ; }
                    
                    try
                    {
                        await BrowserIntance.CloseAsync().WaitAsync(TimeSpan.FromSeconds(5));
                        BrowserIntance.Dispose();
                    } catch { ; }
                }
            }

            return result;
        }

        public static async Task<(string, List<string>)> filterSinglePageByXPath(WebsiteData websiteData, SettingData userSetting, CancellationToken token) // title, list of urls
        {            
            // get full site html page, 3 attempts
            string fullHTML = "";
            fullHTML = await getFullHTML(websiteData.MainURL, userSetting, token);

            if (string.IsNullOrEmpty(fullHTML)) return ("", new List<string>());

            List<string> imageLinks = new List<string>();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(fullHTML);
            string title = "";
            HtmlNodeCollection? nodes = null;

            // get website title
            try
            {
                title = htmlDoc.DocumentNode.SelectSingleNode("//title").InnerText;
                nodes = htmlDoc.DocumentNode.SelectNodes(websiteData.XpathSelector);
            }
            catch
            {
                return (title, imageLinks);
            }
                
            // get urls
            if  (nodes is not null && nodes.Count > 0)
            {
                foreach (HtmlNode node in nodes)
                {
                    try
                    {
                        if (node.Attributes[websiteData.Attribute] is not null)
                            imageLinks.Add(node.Attributes[websiteData.Attribute].Value);
                        else
                            imageLinks.Add(node.OuterHtml);
                    }
                    catch
                    {
                        imageLinks = new List<string>();
                        break;
                    }
                }
            }
            else
            {
                imageLinks = new List<string>();
            }

            return (title, imageLinks);
        }

        public static async Task<int> downloadImages(WebsiteData websiteData, SettingData settingData, MainWindow curViewInstance, CancellationToken token)
        {
            // Create image location directory
            string fullPath = FilesManagement.CreateDirectoryIfNotExist(settingData.DownloadLocation, websiteData.Title);
            if (string.IsNullOrEmpty(fullPath)) return 0;

            // Initialize Downloader
            ImageDownloader downloader = new ImageDownloader();
            int numberImageDownloaded = 0;
            int orderStartingNumber = settingData.OrderStartingNumber;

            for(int index = 0; index < websiteData.ImageLinks.Count; index++, orderStartingNumber++)
            {
                updateView_ProcessBar(index + 1, curViewInstance);
                
                if (token.IsCancellationRequested)
                {
                    return numberImageDownloaded;
                }

                if (FieldValidator.isValidURL(websiteData.ImageLinks[index]))
                {
                    bool isDownloaded = await downloader.DownloadImageAsync(fullPath, websiteData.ImageLinks[index], orderStartingNumber, settingData.MinImageSizeInByte);
                    if (isDownloaded) numberImageDownloaded++;
                }
            }

            // clean up
            downloader.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return numberImageDownloaded;
        }

        public static async Task<(string, List<string>)> filterMutiplePageByXPath(WebsiteData websiteData, SettingData userSetting, CancellationToken token)
        {
            string title = "";
            List<string> imageLinks = new List<string>();

            if (!string.IsNullOrEmpty(websiteData.Page2URL) 
                && !string.IsNullOrEmpty(websiteData.Page3URL)
                && websiteData.ToPage > 0
                && websiteData.FromPage < websiteData.ToPage
                && websiteData.ToPage >= 3)
            {
                bool doable = false;
                string front, back;
                int pageNumberIndicatorForPage2;

                (doable, front, back, pageNumberIndicatorForPage2) = getMutiplePageIndicator(websiteData.Page2URL, websiteData.Page3URL);
                if (doable)
                {
                    List<string> pageList = new List<string>();
                    
                    if (websiteData.FromPage == 1)
                    {
                        pageList.Add(websiteData.MainURL);
                        pageList.Add(websiteData.Page2URL);
                        
                        int pageNumberBuffer = 2 - pageNumberIndicatorForPage2;
                        for (int curPage = 3; curPage <= websiteData.ToPage; curPage++)
                        {
                            int accuratePageNumberOfCurPage = curPage + pageNumberBuffer;
                            string curPageURL = front + accuratePageNumberOfCurPage.ToString() + back;
                            pageList.Add(curPageURL);
                        }
                    }
                    else if (websiteData.FromPage == 2)
                    {
                        pageList.Add(websiteData.Page2URL);

                        int pageNumberBuffer = 2 - pageNumberIndicatorForPage2;
                        for (int curPage = 3; curPage <= websiteData.ToPage; curPage++)
                        {
                            int accuratePageNumberOfCurPage = curPage + pageNumberBuffer;
                            string curPageURL = front + accuratePageNumberOfCurPage.ToString() + back;
                            pageList.Add(curPageURL);
                        }
                    }
                    else // from page 3 or greater
                    {
                        int pageNumberBuffer = 2 - pageNumberIndicatorForPage2;
                        for (int curPage = websiteData.FromPage; curPage <= websiteData.ToPage; curPage++)
                        {
                            int accuratePageNumberOfCurPage = curPage + pageNumberBuffer;
                            string curPageURL = front + accuratePageNumberOfCurPage.ToString() + back;
                            pageList.Add(curPageURL);
                        }
                    }

                    foreach (string page in pageList)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return (title, imageLinks);
                        }

                        WebsiteData currentWebsiteData = new WebsiteData(websiteData);
                        currentWebsiteData.MainURL = page;

                        string curTitle = "";
                        List<string> curImageLinks = new List<string>();

                        (curTitle, curImageLinks) = await filterSinglePageByXPath(currentWebsiteData, userSetting, token);

                        if (!string.IsNullOrEmpty(curTitle))
                        {
                            if (string.IsNullOrEmpty(title)) title = curTitle;
                            imageLinks.AddRange(curImageLinks);
                        }
                    }
                }
            }

            return (title, imageLinks);
        }

        private static (bool, string, string, int) getMutiplePageIndicator(string page2URL, string page3URL)
        {
            string front = "", back = "";
            int pageNumberIndicatorForPage2 = 0;

            if (string.IsNullOrEmpty(page2URL) || string.IsNullOrEmpty(page3URL) || page2URL.Length != page3URL.Length)
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
                back = page2URL.Substring(pageIndicatorIndices[0] + 1, (page2URL.Length - 1) - (pageIndicatorIndices[0] + 1));

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

        private static void updateView_ProcessBar(int curWorkingPosition, MainWindow curViewInstance)
        {
            curViewInstance.updateProgressBar_Indicator(curWorkingPosition);
        }
    }
}
