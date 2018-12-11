using System;
using System.Collections.Generic;
using System.Xml;


namespace XmlMessaging
{
    /*
     * <?xml version="1.0" encoding="UTF-8"?>
     * <REPOSITORY name="repo_name">
     *   <PACKAGE name="package_name_1">
     *     <VERSION>version_1</VERSION>
     *     <VERSION>version_2</VERSION>
     *   </PACKAGE>
     * </REPOSITORY>
     */

    public class XmlMessageListPackageVersions : XmlMessage
    {
        public Dictionary<string, Dictionary<string, List<string>>> Repositories { get; private set; } =
            new Dictionary<string, Dictionary<string, List<string>>>();

        public XmlMessageListPackageVersions()
        {
        }

        public XmlMessageListPackageVersions(XmlDocument xmlMessage)
        {
            LoadXmlDocument(xmlMessage);
        }

        public bool AddRepositoryPackageVersions(
            string repository,
            string packageName,
            List<string> versions)
        {
            if (!Repositories.ContainsKey(repository))
                Repositories.Add(repository, new Dictionary<string, List<string>>());

            Dictionary<string, List<string>> packages = Repositories[repository];
            if (packages.ContainsKey(packageName))
            {
                Console.WriteLine("ERROR: Package already exists - " + packageName + "(repository: " + repository + ")");
                return false;
            }

            packages.Add(packageName, versions);
            return true;
        }

        protected override void CreateXmlDocumentBody(XmlDocument xmlMessage)
        {
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> repositoryEntry in Repositories)
            {
                // repository
                XmlElement repositoryNode = xmlMessage.CreateElement(XmlMessageNodeName.REPOSITORY);
                repositoryNode.SetAttribute(XmlMessageAttributeName.NAME, repositoryEntry.Key);
                xmlMessage.AppendChild(repositoryNode);

                // repository packages
                foreach (KeyValuePair<string, List<string>> packageEntry in repositoryEntry.Value)
                {
                    XmlElement packageNode = xmlMessage.CreateElement(XmlMessageNodeName.PACKAGE);
                    packageNode.SetAttribute(XmlMessageAttributeName.NAME, packageEntry.Key);
                    repositoryNode.AppendChild(packageNode);

                    // package versions
                    foreach (string version in packageEntry.Value)
                    {
                        XmlElement versionNode = xmlMessage.CreateElement(XmlMessageNodeName.VERSION);
                        versionNode.InnerText = version;
                        packageNode.AppendChild(versionNode);
                    }
                }
            }
        }

        // throws XmlException
        protected override void LoadXmlDocument(XmlDocument xmlMessage)
        {
            // skip all unknown nodes - allow for extensions
            foreach (XmlNode rootNode in xmlMessage.ChildNodes)
            {
                if (rootNode.Name == XmlMessageNodeName.REPOSITORY)
                {
                    foreach (XmlAttribute attribute in rootNode.Attributes)
                    {
                        // process repository with specified name
                        if (attribute.Name == XmlMessageAttributeName.NAME)
                        {
                            string repositoryName = attribute.Value;
                            if (Repositories.ContainsKey(repositoryName))
                                throw new XmlException("Duplicated repository node - " + repositoryName);

                            Dictionary<string, List<string>> packages = new Dictionary<string, List<string>>();
                            LoadPackagesFromXml(rootNode, packages);
                            Repositories.Add(repositoryName, packages);

                            break; // do not allow for multiple repository 'name' attribute
                        }
                    }
                }
            }
        }

        private void LoadPackagesFromXml(
            XmlNode repositoryNode,
            Dictionary<string, List<string>> packages)
        {
            foreach (XmlNode packageNode in repositoryNode.ChildNodes)
            {
                // process package with specified name
                if (packageNode.Name == XmlMessageNodeName.PACKAGE)
                {
                    foreach (XmlAttribute packageAttribute in packageNode.Attributes)
                    {
                        if (packageAttribute.Name == XmlMessageAttributeName.NAME)
                        {
                            string packageName = packageAttribute.Value;
                            if (packages.ContainsKey(packageName))
                                throw new XmlException("Duplicated package node - " + packageName);

                            List<string> versions = new List<string>();
                            foreach (XmlNode versionNode in packageNode.ChildNodes)
                            {
                                versions.Add(versionNode.InnerText);
                            }
                            packages.Add(packageName, versions);

                            break; // do not allow for multiple package 'name' attribute
                        }
                        else
                        {
                            Console.WriteLine("WARNING: Unknown package attribute - " + packageAttribute);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: Unknown package node - " + packageNode.Name);
                }
            }
        }
    }
}
