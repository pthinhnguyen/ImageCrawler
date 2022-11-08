using ICSharpCode.SharpZipLib.Zip;
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
        Task<bool> DownloadImageAsync(string directoryPath, string imageLink, int imageLinkOrder = 1, int minimumSizeByte = 1024);
    }

    public class ImageDownloader : IImageDownloader, IDisposable
    {
        private bool _disposed;
        private WebClient client;

        public ImageDownloader()
        {
            this._disposed = false;
            this.client = new WebClient();
        }

        private void initializeClientDownloader()
        {
            this.client.Credentials = new System.Net.NetworkCredential("userid", "pw");
            this.client.Headers.Add("User-Agent", "Other");
        }

        public async Task<bool> DownloadImageAsync(string directoryPath, string imageLink, int imageLinkOrder = 1, int minimumSizeByte = 1024)
        {
            if (this._disposed) return false;

            // generate image path
            ImageData imageData = new ImageData(imageLink);
            string orderPrefix_FileName_optionalFileExtension = imageLinkOrder.ToString() + "_" + imageData.ImageName;

            // initialize client downloader
            initializeClientDownloader();
            bool isDownloaded = false;

            // save image without knowing the image format
            if (string.IsNullOrEmpty(imageData.ImageExtension)) 
            {
                string completePath = directoryPath + "\\" + orderPrefix_FileName_optionalFileExtension + ".jpg";
                for (int attempts = 0; attempts < 3; attempts++)
                {
                    try
                    {
                        if (FilesManagement.IsFileExistRelativePath(directoryPath, imageData.ImageName)) break;

                        Stream stream = await client.OpenReadTaskAsync(imageLink);
                        Bitmap bitmap = new Bitmap(stream);

                        if (bitmap != null) bitmap.Save(completePath, ImageFormat.Jpeg);

                        stream.Flush();
                        stream.Close();

                        isDownloaded = !FilesManagement.DeleteFileIfSizeLessThan(completePath, minimumSizeByte);
                        break;
                    }
                    catch (WebException)
                    {
                        if (FilesManagement.IsFileExistRelativePath(directoryPath, imageData.ImageName))
                            FilesManagement.deleteFile(completePath);
                        this.initializeClientDownloader();
                    }
                    catch (Exception ex)
                    {
                        isDownloaded = false;
                        break;
                    }
                }
            }
            
            // save image knowing the image format
            else
            {
                string completePath = directoryPath + "\\" + orderPrefix_FileName_optionalFileExtension;
                for (int attempts = 0; attempts < 3; attempts++)
                {
                    try
                    {
                        if (FilesManagement.IsFileExistRelativePath(directoryPath, imageData.ImageName)) break;
                            
                        await this.client.DownloadFileTaskAsync(imageLink, completePath);
                        isDownloaded = !FilesManagement.DeleteFileIfSizeLessThan(completePath, minimumSizeByte);
                        break;
                    }
                    catch (WebException)
                    {
                        if (FilesManagement.IsFileExistRelativePath(directoryPath, imageData.ImageName))
                            FilesManagement.deleteFile(completePath);
                        this.initializeClientDownloader();
                    }
                    catch (Exception ex)
                    {
                        isDownloaded = false;
                        break;
                    }
                }
            }

            // clean up
            this.client.Dispose();

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
