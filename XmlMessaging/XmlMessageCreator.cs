using System.Xml;


namespace XmlMessaging
{
    public class XmlMessageCreator
    {
        static public XmlMessageListPackages CreateXmlMessageListPackages()
        {
            return new XmlMessageListPackages();
        }

        static public XmlMessageListPackages CreateXmlMessageListPackages(XmlDocument xmlMessage)
        {
            return new XmlMessageListPackages(xmlMessage);
        }

        static public XmlMessageListPackageVersions CreateXmlMessageListPackageVersions()
        {
            return new XmlMessageListPackageVersions();
        }

        static public XmlMessageListPackageVersions CreateXmlMessageListPackageVersions(XmlDocument xmlMessage)
        {
            return new XmlMessageListPackageVersions(xmlMessage);
        }

        static public XmlMessageGetPackage CreateXmlMessageGetPackage(
            string repositoryName,
            string packageName,
            string version)
        {
            return new XmlMessageGetPackage(repositoryName, packageName, version);
        }

        static public XmlMessageGetPackage CreateXmlMessageGetPackage(XmlDocument xmlMessage)
        {
            return new XmlMessageGetPackage(xmlMessage);
        }

        static public XmlMessagePackageDataHeader CreateXmlMessagePackageDataHeader(
            string repositoryName,
            string packageName,
            string version,
            int numberOfFiles)
        {
            return new XmlMessagePackageDataHeader(repositoryName, packageName, version, numberOfFiles);
        }

        static public XmlMessagePackageDataHeader CreateXmlMessagePackageDataHeader(XmlDocument xmlMessage)
        {
            return new XmlMessagePackageDataHeader(xmlMessage);
        }

        static public XmlMessagePackageFileBegin CreateXmlMessagePackageFileBegin(
            string name,
            long fileSize,
            string checksum)
        {
            return new XmlMessagePackageFileBegin(name, fileSize, checksum);
        }

        static public XmlMessagePackageFileBegin CreateXmlMessagePackageFileBegin(XmlDocument xmlMessage)
        {
            return new XmlMessagePackageFileBegin(xmlMessage);
        }
    }
}
