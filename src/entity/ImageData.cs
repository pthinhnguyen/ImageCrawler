using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using wpf_imageCrawler.Resources;

namespace wpf_imageCrawler.src.entity
{
    internal class ImageData
    {
        private string imageName;
        private string imageUrl;
        private int imageSize;
        private string imageExtension;

        public ImageData()
        {
            this.imageName = "";
            this.imageUrl = "";
            this.imageSize = 0;
            this.imageExtension = "";
        }

        public ImageData(string imageName, string imageUrl, int imageSize, string imageExtension)
        {
            this.imageName = imageName ?? throw new ArgumentNullException(nameof(imageName));
            this.imageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
            this.imageSize = imageSize;
            this.imageExtension = imageExtension ?? throw new ArgumentNullException(nameof(imageExtension));
        }

        public ImageData(string imageUrl, int imageSize = 0)
        {
            this.imageUrl = imageUrl;
            this.imageSize = imageSize;
            this.imageExtension = IdentifyImageExtension(imageUrl);
            if (string.IsNullOrEmpty(this.imageExtension))
                this.imageName = GenerateImageNameWithoutExtensionFromURL(this.imageUrl);
            else
                this.imageName = GenerateImageNameWithExtensionFromURL(this.imageUrl, this.imageExtension);
        }

        public string ImageName { get => imageName; set => imageName = value; }
        public string ImageUrl { get => imageUrl; set => imageUrl = value; }
        public int ImageSize { get => imageSize; set => imageSize = value; }
        public string ImageExtension { get => imageExtension; set => imageExtension = value; }

        public static string IdentifyImageExtension(string inputImageURL)
        {
            inputImageURL = inputImageURL.ToLower();
            if (inputImageURL.Contains("jpg") || inputImageURL.Contains("jpeg"))
                return "jpg";
            else if (inputImageURL.Contains("png"))
                return "png";
            else if (inputImageURL.Contains("gif"))
                return "gif";
            else if (inputImageURL.Contains("bmp"))
                return "bmp";
            else if (inputImageURL.Contains("tiff"))
                return "tiff";
            else if (inputImageURL.Contains("webp"))
                return "webp";
            else if (inputImageURL.Contains("heif"))
                return "heif";
            else if (inputImageURL.Contains("raw"))
                return "raw";
            else
                return "";
        }

        public static string GenerateImageNameWithExtensionFromURL(string inputImageURL, string inputImageExtension = "")
        {
            if (string.IsNullOrEmpty(inputImageExtension)) throw new ArgumentNullException(nameof(inputImageExtension));

            return Hashing.GetHashString(inputImageURL) + "." + inputImageExtension;
        }

        public static string GenerateImageNameWithoutExtensionFromURL(string inputImageURL)
        {
            return Hashing.GetHashString(inputImageURL);
        }
    }
}
