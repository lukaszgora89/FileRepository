using System;
using System.Xml;


namespace XmlMessaging
{
    /*
     * <?xml version="1.0" encoding="UTF-8"?>
     * <FILE>
     *   <NAME>name_of_file</NAME>
     *   <SIZE>name_of_file</SIZE>
     *   <CHECKSUM>md5_file_checksum</CHECKSUM>
     * </FILE>
     */

    public class XmlMessagePackageFileBegin : XmlMessage
    {
        public string Name { get; private set; } = String.Empty;

        public long Size { get; private set; } = 0;                 // bytes
        public string Checksum{ get; private set; } = String.Empty;

        public XmlMessagePackageFileBegin(
            string name,
            long fileSize,
            string checksum)
        {
            Name = name;
            Size = fileSize;
            Checksum = checksum;
        }

        public XmlMessagePackageFileBegin(XmlDocument xmlMessage)
        {
            LoadXmlDocument(xmlMessage);
        }

        protected override void CreateXmlDocumentBody(XmlDocument xmlMessage)
        {
            // file
            XmlElement fileNode = xmlMessage.CreateElement(XmlMessageNodeName.FILE);
            xmlMessage.AppendChild(fileNode);

            // file name
            XmlElement nameNode = xmlMessage.CreateElement(XmlMessageNodeName.NAME);
            nameNode.InnerText = Name;
            fileNode.AppendChild(nameNode);

            // file name
            XmlElement sizeNode = xmlMessage.CreateElement(XmlMessageNodeName.SIZE);
            sizeNode.InnerText = Size.ToString();
            fileNode.AppendChild(sizeNode);

            // file checksum
            XmlElement checksumNode = xmlMessage.CreateElement(XmlMessageNodeName.CHECKSUM);
            checksumNode.InnerText = Checksum;
            fileNode.AppendChild(checksumNode);
        }

        protected override void LoadXmlDocument(XmlDocument xmlMessage)
        {
            // skip all unknown nodes - allow for extensions
            foreach (XmlNode rootNode in xmlMessage.ChildNodes)
            {
                if (rootNode.Name == XmlMessageNodeName.FILE)
                {
                    foreach (XmlNode filePropertyNode in rootNode.ChildNodes)
                    {
                        if (filePropertyNode.Name == XmlMessageNodeName.NAME)
                        {
                            Name = filePropertyNode.InnerText;
                        }
                        else if (filePropertyNode.Name == XmlMessageNodeName.SIZE)
                        {
                            try
                            {
                                Size = Convert.ToInt64(filePropertyNode.InnerText);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("WARNING: Cannot parse file size - " + filePropertyNode.InnerText);
                                Size = 0;
                            }
                            
                        }
                        else if (filePropertyNode.Name == XmlMessageNodeName.CHECKSUM)
                        {
                            Checksum = filePropertyNode.InnerText;
                        }
                    }

                    break; // allow for one file node
                }
            }
        }
    }
}
