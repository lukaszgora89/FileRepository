using System;
using System.Xml;


namespace XmlMessaging
{
    /*
     * <?xml version="1.0" encoding="UTF-8"?>
     * <DATA repository="repo_name" package="package_name" version="package_version">
     *   <NUMBER_OF_FILES>number_of_files</NUMBER_OF_FILES>
     * </DATA>
     */

    public class XmlMessagePackageDataHeader : XmlMessage
    {
        public string RepositoryName { get; private set; } = String.Empty;
        public string PackageName { get; private set; } = String.Empty;
        public string Version { get; private set; } = String.Empty;
        public int NumberOfFiles { get; private set; } = 0;

        public XmlMessagePackageDataHeader(
            string repositoryName,
            string packageName,
            string version,
            int numberOfFiles )
        {
            RepositoryName = repositoryName;
            PackageName = packageName;
            Version = version;
            NumberOfFiles = numberOfFiles;
        }

        public XmlMessagePackageDataHeader(XmlDocument xmlMessage)
        {
            LoadXmlDocument(xmlMessage);
        }

        protected override void CreateXmlDocumentBody(XmlDocument xmlMessage)
        {
            // data
            XmlElement dataNode = xmlMessage.CreateElement(XmlMessageNodeName.DATA);
            dataNode.SetAttribute(XmlMessageAttributeName.REPOSITORY, RepositoryName);
            dataNode.SetAttribute(XmlMessageAttributeName.PACKAGE, PackageName);
            dataNode.SetAttribute(XmlMessageAttributeName.VERSION, Version);
            xmlMessage.AppendChild(dataNode);

            // number of files
            XmlElement filesNumberNode = xmlMessage.CreateElement(XmlMessageNodeName.NUMBER_OF_FILES);
            filesNumberNode.InnerText = NumberOfFiles.ToString();
            dataNode.AppendChild(filesNumberNode);
        }

        // throws XmlException
        protected override void LoadXmlDocument(XmlDocument xmlMessage)
        {
            // skip all unknown nodes - allow for extensions
            foreach (XmlNode rootNode in xmlMessage.ChildNodes)
            {
                if (rootNode.Name == XmlMessageNodeName.DATA)
                {
                    foreach (XmlAttribute attribute in rootNode.Attributes)
                    {
                        // process repository with specified name
                        if (attribute.Name == XmlMessageAttributeName.REPOSITORY)
                        {
                            RepositoryName = attribute.Value;
                        }
                        else if (attribute.Name == XmlMessageAttributeName.PACKAGE)
                        {
                            PackageName = attribute.Value;
                        }
                        else if (attribute.Name == XmlMessageAttributeName.VERSION)
                        {
                            Version = attribute.Value;
                        }
                    }

                    foreach (XmlNode dataInfoNode in rootNode.ChildNodes)
                    {
                        if (dataInfoNode.Name == XmlMessageNodeName.NUMBER_OF_FILES)
                        {
                            try
                            {
                                NumberOfFiles = Int32.Parse(dataInfoNode.InnerText);
                            }
                            catch (Exception ex)
                            {
                                throw new XmlException("Cannot parse number of files - " + ex.Message);
                            }

                            break; // allow for one data info node
                        }
                    }

                    break; // allow for one data node
                }
            }
        }
    }
}
