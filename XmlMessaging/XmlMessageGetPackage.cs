using System;
using System.Xml;


namespace XmlMessaging
{
    /*
     * <?xml version="1.0" encoding="UTF-8"?>
     * <DATA>
     *   <REPOSITORY>repository_name</REPOSITORY>
     *   <PACKAGE>package_name</PACKAGE>
     *   <VERSION>version</VERSION>
     * </DATA>
     */

    public class XmlMessageGetPackage : XmlMessage
    {
        public string RepositoryName { get; private set; }
        public string PackageName { get; private set; }
        public string Version { get; private set; }

        public XmlMessageGetPackage(
            string repositoryName,
            string packageName,
            string version)
        {
            RepositoryName = repositoryName;
            PackageName = packageName;
            Version = version;
        }

        public XmlMessageGetPackage(XmlDocument xmlMessage)
        {
            LoadXmlDocument(xmlMessage);
        }

        protected override void CreateXmlDocumentBody(XmlDocument xmlMessage)
        {
            // data
            XmlElement dataRootNode = xmlMessage.CreateElement(XmlMessageNodeName.DATA);
            xmlMessage.AppendChild(dataRootNode);

            // repository
            XmlElement repositoryNode = xmlMessage.CreateElement(XmlMessageNodeName.REPOSITORY);
            repositoryNode.InnerText = RepositoryName;
            dataRootNode.AppendChild(repositoryNode);

            // package
            XmlElement packageNode = xmlMessage.CreateElement(XmlMessageNodeName.PACKAGE);
            packageNode.InnerText = PackageName;
            dataRootNode.AppendChild(packageNode);

            // version
            XmlElement versionNode = xmlMessage.CreateElement(XmlMessageNodeName.VERSION);
            versionNode.InnerText = Version;
            dataRootNode.AppendChild(versionNode);
        }

        protected override void LoadXmlDocument(XmlDocument xmlMessage)
        {
            // skip all unknown nodes - allow for extensions
            foreach (XmlNode rootNode in xmlMessage.ChildNodes)
            {
                if (rootNode.Name == XmlMessageNodeName.DATA)
                {
                    foreach (XmlNode dataElementNode in rootNode.ChildNodes)
                    {
                        if (dataElementNode.Name == XmlMessageNodeName.REPOSITORY)
                        {
                            if (!String.IsNullOrEmpty(RepositoryName))
                                throw new XmlException("Duplicated repository node - " + dataElementNode.InnerText);

                            RepositoryName = dataElementNode.InnerText;
                        }
                        else if (dataElementNode.Name == XmlMessageNodeName.PACKAGE)
                        {
                            if (!String.IsNullOrEmpty(PackageName))
                                throw new XmlException("Duplicated package node - " + dataElementNode.InnerText);

                            PackageName = dataElementNode.InnerText;
                        }
                        else if (dataElementNode.Name == XmlMessageNodeName.VERSION)
                        {
                            if (!String.IsNullOrEmpty(Version))
                                throw new XmlException("Duplicated version node - " + dataElementNode.InnerText);

                            Version = dataElementNode.InnerText;
                        }
                    }
                }
            }
        }
    }
}
