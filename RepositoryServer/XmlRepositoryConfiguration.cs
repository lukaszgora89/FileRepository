

namespace RepositoryServer
{
    class XmlRepositoryConfiguration
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public XmlRepositoryConfiguration(
            string name,
            string path)
        {
            Name = name;
            Path = path;
        }
    }
}
