using System;
using System.Collections.Generic;
using System.Xml;


namespace RepositoryServer
{
    /*
     * <?xml version="1.0" encoding="UTF-8"?>
     * <REPOSITORIES>
     *   <REPOSITORY>
     *     <NAME>repository_name</NAME>
     *     <PATH>filesystem_path</PATH>
     *   </REPOSITORY>
     *   <REPOSITORY>
     *     <NAME>repository_name_2</NAME>
     *     <PATH>filesystem_path_2</PATH>
     *   </REPOSITORY>
     * </REPOSITORIES>
     */

    // TODO: Load paths relative to the configuration file

    class XmlConfigurationLoader
    {
        private List<XmlRepositoryConfiguration> _xmlRepositoryConfiguration = new List<XmlRepositoryConfiguration>();

        public XmlConfigurationLoader()
        {
        }

        public bool LoadConfiguration(string xmlConfigurationFilePath)
        {
            try
            {
                XmlDocument configurationFile = new XmlDocument();
                configurationFile.Load(xmlConfigurationFilePath);

                // skip all unknown nodes - allow for extensions
                foreach (XmlNode rootNode in configurationFile.ChildNodes)
                {
                    if (rootNode.Name.ToUpper() == "REPOSITORIES")
                    {
                        foreach (XmlNode repositoryNode in rootNode.ChildNodes)
                        {
                            if (repositoryNode.Name.ToUpper() == "REPOSITORY")
                            {
                                if (!ProcessRepositoryNode(repositoryNode))
                                {
                                    Console.WriteLine("ERROR: Cannot process configuration file repository node.");
                                    return false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("WARNING: Unknown repository node - " + repositoryNode.Name);
                            }
                        }
                    }
                    else if (rootNode.Name.ToLower() != "xml") // xml is basic first node
                    {
                        Console.WriteLine("WARNING: Unknown configuration file root node - " + rootNode.Name);
                    }
                }
            }
            catch (XmlException ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return false;
            }

            return true;
        }

        public IEnumerable<XmlRepositoryConfiguration> RepositoryConfigurations
        {
            get
            {
                return _xmlRepositoryConfiguration;
            }
        }

        private bool ProcessRepositoryNode(XmlNode repositoryNode)
        {
            string name = null;
            string path = null;

            // process repository node
            foreach (XmlNode repositoryElement in repositoryNode.ChildNodes)
            {
                string elementContent = repositoryElement.Name.ToUpper();
                if (elementContent == "NAME")
                {
                    name = repositoryElement.InnerText;
                }
                else if (elementContent == "PATH")
                {
                    path = repositoryElement.InnerText;
                }
                else
                {
                    Console.WriteLine("WARNING: Unknown configuration file repository element - " + elementContent);
                }
            }

            // validate repository data
            if (String.IsNullOrEmpty(name))
            {
                Console.WriteLine("ERROR: Configuration file repository name is missing.");
                return false;
            }

            if (String.IsNullOrEmpty(path))
            {
                Console.WriteLine("ERROR: Configuration file repository name is missing.");
                return false;
            }

            // add new repository to collection
            _xmlRepositoryConfiguration.Add(new XmlRepositoryConfiguration(name, path));

            return true;
        }
    }
}
