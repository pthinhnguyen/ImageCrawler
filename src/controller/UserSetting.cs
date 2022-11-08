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

            try
            {
                this.initializeUserSettingXMLFile(this.userSettingFilePath);
            } catch { ; }

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

            if (!this.userSettingData.UseBuitInBrowser) this.userSettingData.SilentMode = false;
            if (string.IsNullOrEmpty(this.userSettingData.BrowserPath)) this.userSettingData.BrowserPath = this.defaultUserSettingData.BrowserPath;

            if (string.IsNullOrEmpty(this.userSettingData.DownloadLocation)) this.userSettingData.DownloadLocation = this.defaultUserSettingData.DownloadLocation;
            if (this.userSettingData.MinImageSizeInByte < 64) this.userSettingData.MinImageSizeInByte = this.defaultUserSettingData.MinImageSizeInByte;
            if (this.userSettingData.OrderStartingNumber < 1) this.userSettingData.OrderStartingNumber = this.defaultUserSettingData.OrderStartingNumber;
            if (this.userSettingData.DiffFolderEach) this.userSettingData.CreateNewFolder = true;

            return this.userSettingData;
        }
        
        public SettingData saveUserSettings()
        {
            
            if (string.IsNullOrEmpty(userSettingData.BrowserPath)) this.userSettingData.BrowserPath = this.defaultUserSettingData.BrowserPath;
            if (string.IsNullOrEmpty(this.userSettingData.BrowserPath)) this.userSettingData.BrowserPath = this.defaultUserSettingData.BrowserPath;

            if (string.IsNullOrEmpty(userSettingData.DownloadLocation)) this.userSettingData.DownloadLocation = this.defaultUserSettingData.DownloadLocation;
            if (userSettingData.MinImageSizeInByte < 64) this.userSettingData.MinImageSizeInByte = this.defaultUserSettingData.MinImageSizeInByte;
            if (this.userSettingData.OrderStartingNumber < 1) this.userSettingData.OrderStartingNumber = this.defaultUserSettingData.OrderStartingNumber;
            if (this.userSettingData.DiffFolderEach) this.userSettingData.CreateNewFolder = true;

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

                xmlWriter.WriteStartElement("BrowserPath");
                xmlWriter.WriteString(this.defaultUserSettingData.BrowserPath);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("SpeedMode");
                xmlWriter.WriteString(this.defaultUserSettingData.SpeedMode.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("UseBuitInBrowser");
                xmlWriter.WriteString(this.defaultUserSettingData.UseBuitInBrowser.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("SilentMode");
                xmlWriter.WriteString(this.defaultUserSettingData.SilentMode.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("DownloadLocation");
                xmlWriter.WriteString(this.defaultUserSettingData.DownloadLocation);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("MinImageSizeInByteNode");
                xmlWriter.WriteString(this.defaultUserSettingData.MinImageSizeInByte.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("OrderStartingNumberNode");
                xmlWriter.WriteString(this.defaultUserSettingData.OrderStartingNumber.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("CreateNewFolder");
                xmlWriter.WriteString(this.defaultUserSettingData.CreateNewFolder.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("DiffFolderEach");
                xmlWriter.WriteString(this.defaultUserSettingData.DiffFolderEach.ToString());
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

            XmlNode? BrowserPathNode;
            XmlNode? SpeedModeNode;
            XmlNode? UseBuitInBrowserNode;
            XmlNode? SilentModeNode;

            XmlNode? DownloadLocationNode;
            XmlNode? MinImageSizeInByteNode;
            XmlNode? OrderStartingNumberNode;
            XmlNode? CreateNewFolderNode;
            XmlNode? DiffFolderEachNode;

            if (xmlReader is not null && xmlReader.DocumentElement is not null)
            {
                BrowserPathNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/BrowserPath");
                SpeedModeNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/SpeedMode");
                UseBuitInBrowserNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/UseBuitInBrowser");
                SilentModeNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/SilentMode");

                DownloadLocationNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/DownloadLocation");
                MinImageSizeInByteNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/MinImageSizeInByteNode");
                OrderStartingNumberNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/OrderStartingNumberNode");
                CreateNewFolderNode= xmlReader.DocumentElement.SelectSingleNode("/Settings/User/CreateNewFolder");
                DiffFolderEachNode = xmlReader.DocumentElement.SelectSingleNode("/Settings/User/DiffFolderEach");

                readingUserSettingData.BrowserPath = (BrowserPathNode is not null) ? BrowserPathNode.InnerText : this.defaultUserSettingData.BrowserPath;
                readingUserSettingData.DownloadLocation = (DownloadLocationNode is not null) ? DownloadLocationNode.InnerText : this.defaultUserSettingData.DownloadLocation;

                try
                {
                    if (SpeedModeNode is not null)
                        readingUserSettingData.SpeedMode = bool.Parse(SpeedModeNode.InnerText);
                    else
                        readingUserSettingData.SpeedMode = this.defaultUserSettingData.SpeedMode;
                }
                catch
                {
                    readingUserSettingData.SpeedMode = this.defaultUserSettingData.SpeedMode;
                }

                try
                {
                    if (UseBuitInBrowserNode is not null)
                        readingUserSettingData.UseBuitInBrowser = bool.Parse(UseBuitInBrowserNode.InnerText);
                    else
                        readingUserSettingData.UseBuitInBrowser = this.defaultUserSettingData.UseBuitInBrowser;
                }
                catch
                {
                    readingUserSettingData.UseBuitInBrowser = this.defaultUserSettingData.UseBuitInBrowser;
                }

                try
                {
                    if (SilentModeNode is not null)
                        readingUserSettingData.SilentMode = bool.Parse(SilentModeNode.InnerText);
                    else
                        readingUserSettingData.SilentMode = this.defaultUserSettingData.SilentMode;
                }
                catch
                {
                    readingUserSettingData.SilentMode = this.defaultUserSettingData.SilentMode;
                }

                try
                {
                    if (MinImageSizeInByteNode is not null)
                        readingUserSettingData.MinImageSizeInByte = int.Parse(MinImageSizeInByteNode.InnerText);
                    else
                        readingUserSettingData.MinImageSizeInByte = this.defaultUserSettingData.MinImageSizeInByte;
                }
                catch
                {
                    readingUserSettingData.MinImageSizeInByte = this.defaultUserSettingData.MinImageSizeInByte;
                }

                try
                {
                    if (OrderStartingNumberNode is not null)
                        readingUserSettingData.OrderStartingNumber = int.Parse(OrderStartingNumberNode.InnerText);
                    else
                        readingUserSettingData.OrderStartingNumber = this.defaultUserSettingData.OrderStartingNumber;
                }
                catch
                {
                    readingUserSettingData.OrderStartingNumber = this.defaultUserSettingData.OrderStartingNumber;
                }

                try
                {
                    if (CreateNewFolderNode is not null)
                        readingUserSettingData.CreateNewFolder = bool.Parse(CreateNewFolderNode.InnerText);
                    else
                        readingUserSettingData.CreateNewFolder = this.defaultUserSettingData.CreateNewFolder;
                }
                catch
                {
                    readingUserSettingData.CreateNewFolder = this.defaultUserSettingData.CreateNewFolder;
                }

                try
                {
                    if (DiffFolderEachNode is not null)
                        readingUserSettingData.DiffFolderEach = bool.Parse(DiffFolderEachNode.InnerText);
                    else
                        readingUserSettingData.DiffFolderEach = this.defaultUserSettingData.DiffFolderEach;
                }
                catch
                {
                    readingUserSettingData.DiffFolderEach = this.defaultUserSettingData.DiffFolderEach;
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

            XmlNode? BrowserPathNode;
            XmlNode? SpeedModeNode;
            XmlNode? UseBuitInBrowserNode;
            XmlNode? SilentModeNode;

            XmlNode? DownloadLocationNode;
            XmlNode? MinImageSizeInByteNode;
            XmlNode? OrderStartingNumberNode;
            XmlNode? CreateNewFolderNode;
            XmlNode? DiffFolderEachNode;

            if (xmlWriter is not null && xmlWriter.DocumentElement is not null)
            {
                BrowserPathNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/BrowserPath");
                SpeedModeNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/SpeedMode");
                UseBuitInBrowserNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/UseBuitInBrowser");
                SilentModeNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/SilentMode");

                DownloadLocationNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/DownloadLocation");
                MinImageSizeInByteNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/MinImageSizeInByteNode");
                OrderStartingNumberNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/OrderStartingNumberNode");
                CreateNewFolderNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/CreateNewFolder");
                DiffFolderEachNode = xmlWriter.DocumentElement.SelectSingleNode("/Settings/User/DiffFolderEach");

                if (BrowserPathNode is not null) BrowserPathNode.InnerText = userSetttingData.BrowserPath;
                if (SpeedModeNode is not null) SpeedModeNode.InnerText = userSetttingData.SpeedMode.ToString();
                if (UseBuitInBrowserNode is not null) UseBuitInBrowserNode.InnerText = userSetttingData.UseBuitInBrowser.ToString();
                if (SilentModeNode is not null) SilentModeNode.InnerText = userSettingData.SilentMode.ToString();

                if (DownloadLocationNode is not null) DownloadLocationNode.InnerText = userSetttingData.DownloadLocation;
                if (MinImageSizeInByteNode is not null) MinImageSizeInByteNode.InnerText = userSetttingData.MinImageSizeInByte.ToString();
                if (OrderStartingNumberNode is not null) OrderStartingNumberNode.InnerText = userSetttingData.OrderStartingNumber.ToString();
                if (CreateNewFolderNode is not null) CreateNewFolderNode.InnerText = userSetttingData.CreateNewFolder.ToString();
                if (DiffFolderEachNode is not null) DiffFolderEachNode.InnerText = userSetttingData.DiffFolderEach.ToString();

                try
                {
                    xmlWriter.Save(xmlFilePath);
                } catch { ; }
            }
        }

        public SettingData UserSettingData { get => userSettingData; set => userSettingData = value; }
        public SettingData DefaultUserSettingData { get => defaultUserSettingData; set => defaultUserSettingData = value; }
    }
}
