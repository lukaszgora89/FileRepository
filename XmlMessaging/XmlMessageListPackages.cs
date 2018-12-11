using System;
using System.Collections.Generic;
using System.Xml;


namespace XmlMessaging
{
    /*
     * <?xml version="1.0" encoding="UTF-8"?>
     * <REPOSIOTORIES>
     *   <REPOSITORY name="repo_name">
     *     <PACKAGE>package_name_1</PACKAGE>
     *     <PACKAGE>package_name_2</PACKAGE>
     *   </REPOSITORY>
     *   <REPOSITORY name="repo_name_2">
     *     <PACKAGE>package_2_name_1</PACKAGE>
     *     <PACKAGE>package_2_name_2</PACKAGE>
     *   </REPOSITORY>
     * </REPOSIOTORIES>
     */

    public class XmlMessageListPackages : XmlMessage
    {
        // TODO IEnumberable
        public Dictionary<string, List<string>> Repositories { get; private set; } = new Dictionary<string, List<string>>();

        public XmlMessageListPackages()
        {
        }

        public XmlMessageListPackages(XmlDocument xmlMessage)
        {
            LoadXmlDocument(xmlMessage);
        }

        public bool AddRepositoryPackages(
            string repositoryName,
            List<string> packages)
        {
            if (Repositories.ContainsKey(repositoryName))
            {
                Console.WriteLine("ERROR: Repository already exists - " + repositoryName);
                return false;
            }

            Repositories.Add(repositoryName, packages);
            return true;
        }

        protected override void CreateXmlDocumentBody(XmlDocument xmlMessage)
        {
            // add main REPOSITORIES node
            XmlElement repositoriesNode = xmlMessage.CreateElement(XmlMessageNodeName.REPOSITORIES);
            xmlMessage.AppendChild(repositoriesNode);

            foreach (KeyValuePair<string, List<string>> repositoryEntry in Repositories)
            {
                // repository
                XmlElement repositoryNode = xmlMessage.CreateElement(XmlMessageNodeName.REPOSITORY);
                repositoryNode.SetAttribute(XmlMessageAttributeName.NAME, repositoryEntry.Key);
                repositoriesNode.AppendChild(repositoryNode);

                // repository packages
                foreach (string package in repositoryEntry.Value)
                {
                    XmlElement packageNode = xmlMessage.CreateElement(XmlMessageNodeName.PACKAGE);
                    packageNode.InnerText = package;
                    repositoryNode.AppendChild(packageNode);
                }
            }
        }

        protected override void LoadXmlDocument(XmlDocument xmlMessage)
        {
            foreach (XmlNode rootNode in xmlMessage.ChildNodes)
            {
                if (rootNode.Name == XmlMessageNodeName.REPOSITORIES)
                {
                    LoadRepositories(rootNode);
                    break; // process only one repositories node
                }
            }
        }

        private void LoadRepositories(XmlNode repositoriesNode)
        {
            // skip all unknown nodes - allow for extensions
            foreach (XmlNode repositoryNode in repositoriesNode.ChildNodes)
            {
                if (repositoryNode.Name == XmlMessageNodeName.REPOSITORY)
                {
                    foreach (XmlAttribute attributeAttribute in repositoryNode.Attributes)
                    {
                        if (attributeAttribute.Name == XmlMessageAttributeName.NAME)
                        {
                            List<string> packages = new List<string>();
                            foreach (XmlNode packageNode in repositoryNode.ChildNodes)
                            {
                                if (packageNode.Name == XmlMessageNodeName.PACKAGE)
                                {
                                    packages.Add(packageNode.InnerText);
                                }
                            }

                            Repositories.Add(attributeAttribute.Value, packages);

                            break; // do not allow for multiple 'name' attribute
                        }
                    }
                }
            }
        }
    }
}
