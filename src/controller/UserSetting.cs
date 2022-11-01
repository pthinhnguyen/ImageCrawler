using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using wpf_imageCrawler.Resources;
using wpf_imageCrawler.src.entity;

namespace wpf_imageCrawler.src.controller
{
    public class UserSetting
    {
        private static UserSetting? instance = null;
        private static readonly object padlock = new object();

        private readonly string userSettingLocation = System.AppDomain.CurrentDomain.BaseDirectory;
        private readonly string userSettingFileName = "config.xml";
        private readonly string userSettingFilePath;
        private SettingData userSettingData;
        private SettingData defaultUserSettingData;

        public UserSetting()
        {
            this.userSettingData = new SettingData();
            this.defaultUserSettingData = SettingData.loadDefaultSettingData();
            this.userSettingFilePath = this.userSettingLocation + "\\" + this.userSettingFileName;

            this.initializeUserSettingXMLFile(this.userSettingFilePath);
            this.loadUserSetting();
        }

        public static UserSetting Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new UserSetting();
                    }
                    return instance;
                }
            }
        }

        public SettingData loadUserSetting()
        {
            this.userSettingData = this.readUserSettingFromXMLFile(this.userSettingFilePath);
            if (string.IsNullOrEmpty(this.userSettingData.DownloadLocation)) this.userSettingData.DownloadLocation = this.defaultUserSettingData.DownloadLocation;
            if (string.IsNullOrEmpty(this.userSettingData.BrowserPath)) this.userSettingData.BrowserPath = this.defaultUserSettingData.BrowserPath;
            if (this.userSettingData.MinImageSizeInByte < 64) this.userSettingData.MinImageSizeInByte = this.defaultUserSettingData.MinImageSizeInByte;
            if (this.userSettingData.OrderStartingNumber < 1) this.userSettingData.OrderStartingNumber = this.defaultUserSettingData.OrderStartingNumber;

            return this.userSettingData;
        }
        
        public SettingData saveUserSettings()
        {
            if (string.IsNullOrEmpty(userSettingData.DownloadLocation)) this.userSettingData.DownloadLocation = this.defaultUserSettingData.DownloadLocation;
            if (string.IsNullOrEmpty(userSettingData.BrowserPath)) this.userSettingData.BrowserPath = this.defaultUserSettingData.BrowserPath;
            if (userSettingData.MinImageSizeInByte < 64) this.userSettingData.MinImageSizeInByte = this.defaultUserSettingData.MinImageSizeInByte;
            if (this.userSettingData.OrderStartingNumber < 1) this.userSettingData.OrderStartingNumber = this.defaultUserSettingData.OrderStartingNumber;

            this.writeUserSettingToXMLFile(this.userSettingData, this.userSettingFilePath);

            return this.userSettingData;
        }


        private void initializeUserSettingXMLFile(string xmlFilePath)
        {
            if (!File.Exists(xmlFilePath))
            {
                var sts = new XmlWriterSettings()
                {
                    Indent = true,
                };
                using var xmlWriter = XmlWriter.Create(xmlFilePath, sts);

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Settings");
                xmlWriter.WriteStartElement("User");
                
                xmlWriter.WriteStartElement("DownloadLocation");
                xmlWriter.WriteString(this.defaultUserSettingData.DownloadLocation);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("BrowserPath");
                xmlWriter.WriteString(this.defaultUserSettingData.BrowserPath);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("MinImageSizeInByte");
                xmlWriter.WriteString(this.defaultUserSettingData.MinImageSizeInByte.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("OrderStartingNumber");
                xmlWriter.WriteString(this.defaultUserSettingData.OrderStartingNumber.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
        }

        public SettingData resetUserSetting()
        {
            this.userSettingData = SettingData.loadDefaultSettingData();
            return this.userSettingData;
        }

        private SettingData readUserSettingFromXMLFile(string xmlFilePath)
        {
            SettingData readingUserSettingData = new SettingData();
            XmlDocument xmlReader = new XmlDocument();

            try
            {
                xmlReader.Load(xmlFilePath);
            }
            catch
            {
                return this.defaultUserSettingData;
            }

            XmlNode? DownloadLocationNode;
            XmlNode? BrowserPathNode;
            XmlNode? MinImageSizeInByte;
            XmlNode? OrderStartingNumber;

            if (xmlReader is not null && xmlReader.DocumentElement is not null)
            {
                DownloadLocationNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/DownloadLocation");
                BrowserPathNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/BrowserPath");
                MinImageSizeInByte = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/MinImageSizeInByte");
                OrderStartingNumber = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/OrderStartingNumber");

                readingUserSettingData.DownloadLocation = (DownloadLocationNode is not null) ? DownloadLocationNode.InnerText : this.defaultUserSettingData.DownloadLocation;
                readingUserSettingData.BrowserPath = (BrowserPathNode is not null) ? BrowserPathNode.InnerText : this.defaultUserSettingData.BrowserPath;
                try
                {
                    if (MinImageSizeInByte is not null)
                        readingUserSettingData.MinImageSizeInByte = Int32.Parse(MinImageSizeInByte.InnerText);
                    else
                        readingUserSettingData.MinImageSizeInByte = 0;
                }
                catch
                {
                    readingUserSettingData.MinImageSizeInByte = 0;
                }
                try
                {
                    if (OrderStartingNumber is not null)
                        readingUserSettingData.OrderStartingNumber = Int32.Parse(OrderStartingNumber.InnerText);
                    else
                        readingUserSettingData.OrderStartingNumber = 0;
                }
                catch
                {
                    readingUserSettingData.OrderStartingNumber = 0;
                }
            }
            else
            {
                readingUserSettingData = this.defaultUserSettingData;
            }

            return readingUserSettingData;
        }

        private void writeUserSettingToXMLFile(SettingData userSetttingData, string xmlFilePath)
        {
            XmlDocument xmlWriter = new XmlDocument();
            try
            {
                xmlWriter.Load(xmlFilePath);
            }
            catch
            {
                return;
            }

            XmlNode? DownloadLocationNode;
            XmlNode? BrowserPathNode;
            XmlNode? MinImageSizeInByte;
            XmlNode? OrderStartingNumber;

            if (xmlWriter is not null && xmlWriter.DocumentElement is not null)
            {
                DownloadLocationNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/DownloadLocation");
                BrowserPathNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/BrowserPath");
                MinImageSizeInByte = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/MinImageSizeInByte");
                OrderStartingNumber = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/OrderStartingNumber");

                if (DownloadLocationNode is not null) DownloadLocationNode.InnerText = userSetttingData.DownloadLocation;
                if (BrowserPathNode is not null) BrowserPathNode.InnerText = userSetttingData.BrowserPath;
                if (MinImageSizeInByte is not null) MinImageSizeInByte.InnerText = userSetttingData.MinImageSizeInByte.ToString();
                if (OrderStartingNumber is not null) OrderStartingNumber.InnerText = userSetttingData.OrderStartingNumber.ToString();

                xmlWriter.Save(xmlFilePath);
            }
        }

        public SettingData UserSettingData { get => userSettingData; set => userSettingData = value; }
        public SettingData DefaultUserSettingData { get => defaultUserSettingData; set => defaultUserSettingData = value; }
    }
}
