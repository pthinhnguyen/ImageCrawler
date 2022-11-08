using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace wpf_imageCrawler.Resources
{
    public static class FilesManagement
    {
        private static readonly int DirectoryNameMaxLength = 64;
        
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
            input = Regex.Replace(input, @"\t|\n|\r", "");
            input = Regex.Replace(input, @"\s+", " ");
            input = Regex.Replace(input, "[^0-9A-Za-z ]", "");
            input = input.Trim();
            input = input.Replace(" ", "_");

            if (input.Length > DirectoryNameMaxLength)
                input = input.Substring(0, DirectoryNameMaxLength);

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

        public static string CreateDirectoryIfNotExist(string relativePath, string directoryName = "")
        {
            if (string.IsNullOrEmpty(relativePath)) return "";

            string completeAbsolutePath = "";
            
            if (string.IsNullOrEmpty(directoryName)) completeAbsolutePath = relativePath;
            else
            {
                try
                {
                    directoryName = RemoveToneMark(directoryName);
                    directoryName = NormalizeDirectoryName(directoryName);

                    string curDirectoryName = new DirectoryInfo(relativePath).Name;
                    if (curDirectoryName == directoryName) completeAbsolutePath = relativePath;
                    else completeAbsolutePath = relativePath + "\\" + directoryName;
                }
                catch
                {
                    completeAbsolutePath = relativePath;
                }
            }

            try
            {
                System.IO.Directory.CreateDirectory(completeAbsolutePath);
            }
            catch
            {
                completeAbsolutePath = "";
            }
            
            return completeAbsolutePath;
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

        public static void deleteFile(string path)
        {
            try
            {
                FileInfo file = new FileInfo(path);
                if (file.Exists) file.Delete();
            } catch { ; }
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
