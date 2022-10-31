using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace wpf_imageCrawler.Resources
{
    public static class FilesManagement
    {
        private static string NormalizeDirectoryName(string input)
        {
            input = input.Replace("/", string.Empty);
            input = input.Replace("\\", string.Empty);
            input = input.Replace("<", string.Empty);
            input = input.Replace(">", string.Empty);
            input = input.Replace(":", string.Empty);
            input = input.Replace("|", string.Empty);
            input = input.Replace("?", string.Empty);
            input = input.Replace("*", string.Empty);
            input = input.Replace("\"", string.Empty);

            return input;
        }

        private static string RemoveToneMark(string input)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(input);
            string asciiStr = System.Text.Encoding.UTF8.GetString(tempBytes);

            return asciiStr;
        }

        public static string CreateDirectoryIfNotExist(string path, string directoryName)
        {
            directoryName = RemoveToneMark(directoryName);
            directoryName = NormalizeDirectoryName(directoryName);
            string fullPath = "";

            try
            {
                string curDirectoryName = new DirectoryInfo(path).Name;
                if (curDirectoryName == directoryName) 
                {
                    fullPath = path;
                }
                else
                {
                    fullPath = path + "\\" + directoryName;
                    System.IO.Directory.CreateDirectory(fullPath);
                }
                return fullPath;
            }
            catch
            {
                return "";
            }
        }

        public static bool DeleteFileIfSizeLessThan(string path, int minimumSizeByte)
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                if (minimumSizeByte > file.Length)
                {
                    file.Delete();
                    return true;
                }
            }
            return false;
        }

        public static bool IsFileExistAbsolutePath(string path)
        {
            FileInfo file = new FileInfo(path);
            return file.Exists;
        }

        public static bool IsFileExistRelativePath(string directoryPath, string fileName)
        {
            return Directory.EnumerateFiles(directoryPath).Any(f => f.Contains(fileName));
        }
    }
}
