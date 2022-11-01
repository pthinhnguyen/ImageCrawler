using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using wpf_imageCrawler.Resources;
using wpf_imageCrawler.src.entity;

namespace wpf_imageCrawler.src.controller
{
    public interface IImageDownloader
    {
        Task<bool> DownloadImageAsync(string directoryPath, string imageLink, int imageLinkOrder = 0, int minimumSizeByte = 1024);
    }

    public class ImageDownloader : IImageDownloader, IDisposable
    {
        private bool _disposed;
        private WebClient client;
        private bool isFreshClient;

        public ImageDownloader()
        {
            this._disposed = false;
            this.client = new WebClient();
            this.isFreshClient = false;
        }

        public async Task<bool> DownloadImageAsync(string directoryPath, string imageLink, int imageLinkOrder = 1, int minimumSizeByte = 1024)
        {
            if (this._disposed) { throw new ObjectDisposedException(GetType().FullName); }

            // generate image path
            ImageData imageData = new ImageData(imageLink);
            string imageFileNameWithLinkOrderPrefix = imageLinkOrder.ToString() + "_" + imageData.ImageName;

            // initialize client downloader
            if (!isFreshClient)
            {
                client.Credentials = new System.Net.NetworkCredential("userid", "pw");
                client.Headers.Add("User-Agent", "Other");
                this.isFreshClient = true;
            }

            // Download image
            bool isDownloaded = false;

            // save image without knowing the image format
            if (string.IsNullOrEmpty(imageData.ImageExtension)) 
            {
                string completePath = directoryPath + "\\" + imageFileNameWithLinkOrderPrefix + ".jpg";
                if (!FilesManagement.IsFileExistRelativePath(directoryPath, imageData.ImageName))
                {
                    try
                    {
                        Stream stream = await client.OpenReadTaskAsync(imageLink);
                        Bitmap bitmap = new Bitmap(stream);

                        if (bitmap != null)
                            bitmap.Save(completePath, ImageFormat.Jpeg);

                        stream.Flush();
                        stream.Close();

                        isDownloaded = !FilesManagement.DeleteFileIfSizeLessThan(completePath, minimumSizeByte);
                    }
                    catch { ; }
                }
            }
            
            // save image knowing the image format
            else
            {
                string completePath = directoryPath + "\\" + imageFileNameWithLinkOrderPrefix;
                if (!FilesManagement.IsFileExistRelativePath(directoryPath, imageData.ImageName))
                {
                    try
                    {
                        await this.client.DownloadFileTaskAsync(imageLink, completePath);
                        isDownloaded = !FilesManagement.DeleteFileIfSizeLessThan(completePath, minimumSizeByte);
                    }
                    catch { ; }
                }
            }

            // clean up
            this.client.Dispose();
            this.isFreshClient = false;

            return isDownloaded;
        }

        public void Dispose()
        {
            if (_disposed) { return; }

            this.client.Dispose();

            GC.SuppressFinalize(this);
            this._disposed = true;
        }
    }
}
