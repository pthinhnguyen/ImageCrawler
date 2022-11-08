using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using PuppeteerSharp;
using wpf_imageCrawler.src.controller;
using wpf_imageCrawler.src.entity;

namespace wpf_imageCrawler.src.service
{
    public class PuppeteerSharpBrowser
    {
        private async Task DisposeBrowserPage(IBrowser? browserInstance, IPage? pageInstance)
        {
            try
            {
                if (pageInstance is not null)
                {
                    await pageInstance.CloseAsync().WaitAsync(TimeSpan.FromSeconds(5));
                    pageInstance.Dispose();
                }
            }
            catch {; }

            try
            {
                if (browserInstance is not null)
                {
                    await browserInstance.CloseAsync().WaitAsync(TimeSpan.FromSeconds(5));
                    browserInstance.Dispose();
                }
            }
            catch { ; }
        }

        private async Task<IBrowser?> InitializeBrowser(SettingData userSettingData, CancellationToken token)
        {
            if (userSettingData is null) return null;

            if (userSettingData.UseBuitInBrowser)
            {
                await new BrowserFetcher(Product.Firefox).DownloadAsync().WaitAsync(token);
                var BrowserInstance = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = userSettingData.SilentMode,
                    Product = Product.Firefox
                }).WaitAsync(token);

                if (token.IsCancellationRequested)
                {
                    await this.DisposeBrowserPage(BrowserInstance, null);
                    return null;
                }
                
                return BrowserInstance;
            }
            else
            {
                var BrowserInstance = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = userSettingData.SilentMode,
                    ExecutablePath = userSettingData.BrowserPath
                }).WaitAsync(token);

                if (token.IsCancellationRequested)
                {
                    await this.DisposeBrowserPage(BrowserInstance, null);
                    return null;
                }
                
                return BrowserInstance;
            }
        }
        
        public async Task<string> loadPageContentNormal(string url, SettingData userSettingData, CancellationToken token)
        {
            if (userSettingData is null) 
                return "";
            
            string pageContent = "";

            for (int attempt = 0; attempt < 3; attempt++)
            {
                using IBrowser? browserInstance = await InitializeBrowser(userSettingData, token);
                using IPage? pageInstance = (browserInstance is not null) ? await browserInstance.NewPageAsync().WaitAsync(token) : null;

                if (token.IsCancellationRequested || browserInstance is null || pageInstance is null)
                {
                    await DisposeBrowserPage(browserInstance, pageInstance);
                    return "";
                }

                // Get the page Content
                pageContent = "";
                try
                {
                    // Access The Website
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromSeconds(10);
                        NavigationOptions opitions = new NavigationOptions();
                        opitions.WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded };
                        opitions.Timeout = (int)timeout.TotalMilliseconds;

                        await pageInstance.GoToAsync(url, opitions).WaitAsync(token);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }

                    // Wait for loading
                    try
                    {
                        int timeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
                        await pageInstance.WaitForTimeoutAsync(timeout).WaitAsync(token);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }

                    // Scroll to the bottom
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromSeconds(5);
                        await pageInstance.EvaluateExpressionAsync("window.scrollBy(0,window.document.body.scrollHeight)").WaitAsync(timeout, token);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }

                    // Wait to load the rest of the website
                    try
                    {
                        int timeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
                        await pageInstance.WaitForTimeoutAsync(timeout).WaitAsync(token);
                    }
                    catch (Exception ex)
                    {
                        ;
                    }

                    // Get the content
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromSeconds(5);
                        pageContent = await pageInstance.GetContentAsync().WaitAsync(timeout, token);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    if (token.IsCancellationRequested)
                        pageContent = "";

                    attempt = 3;
                }
                catch (OperationCanceledException)
                {
                    pageContent = "";
                    attempt = 3;
                }
                catch (Exception ex)
                {
                    pageContent = "";
                }
                finally
                {
                    await DisposeBrowserPage(browserInstance, pageInstance);
                }
            }

            return pageContent;
        }

        public async Task<string> loadPageContentFast(string url, SettingData userSettingData, CancellationToken token)
        {
            if (userSettingData is null) 
                return "";

            string pageContent = "";

            for (int attempt = 0; attempt < 3; attempt++)
            {
                using IBrowser? browserInstance = await InitializeBrowser(userSettingData, token);
                using IPage? pageInstance = (browserInstance is not null) ? await browserInstance.NewPageAsync().WaitAsync(token) : null;

                if (token.IsCancellationRequested || browserInstance is null || pageInstance is null)
                {
                    await DisposeBrowserPage(browserInstance, pageInstance);
                    return "";
                }

                // Get the page Content
                pageContent = "";
                try
                {
                    // Access The Website
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromSeconds(10);
                        NavigationOptions opitions = new NavigationOptions();
                        opitions.Timeout = (int)timeout.TotalMilliseconds;

                        await pageInstance.GoToAsync(url).WaitAsync(token);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    // Get the content
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromSeconds(5);
                        pageContent = await pageInstance.GetContentAsync().WaitAsync(timeout, token);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    if (token.IsCancellationRequested)
                        pageContent = "";

                    attempt = 3;
                }
                catch (OperationCanceledException)
                {
                    pageContent = "";
                    attempt = 3;
                }
                catch (Exception ex)
                {
                    pageContent = "";
                }
                finally
                {
                    await DisposeBrowserPage(browserInstance, pageInstance);
                }
            }

            return pageContent;
        }

        ~PuppeteerSharpBrowser()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
