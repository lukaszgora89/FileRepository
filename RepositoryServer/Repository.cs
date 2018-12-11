using System;
using System.Collections.Generic;
using System.IO;


namespace RepositoryServer
{
    class Repository
    {
        private DirectoryInfo _path = null;

        public string Name { get; private set; } = "Unknown";

        public Repository(string name, string path)
        {
            Name = name;
            _path = new DirectoryInfo(path);
        }

        public List<string> GetPackages()
        {
            List<string> packagesList = new List<string>();

            if (_path.Exists)
            {
                foreach (DirectoryInfo directory in _path.GetDirectories())
                    packagesList.Add(directory.Name);
            }
            else
            {
                Console.WriteLine("ERROR: Repository path does not exists - " + _path);
            }

            return packagesList;
        }

        public List<string> GetPackageVersions(string packageName)
        {
            DirectoryInfo packageDirectory = new DirectoryInfo(Path.Combine(_path.FullName, packageName));

            List<string> packagesVersions = new List<string>();

            if (packageDirectory.Exists)
            {
                foreach (DirectoryInfo directory in packageDirectory.GetDirectories())
                    packagesVersions.Add(directory.Name);
            }
            else
            {
                Console.WriteLine("ERROR: Package path does not exists - " + packageDirectory);
            }

            return packagesVersions;
        }

        public string GetPackagePath(string packageName,string packageVersion)
        {
            string packageDirectory = Path.Combine(_path.FullName, packageName);
            if (!Directory.Exists(packageDirectory))
                return null;

            string packageVersionDirectory = Path.Combine(packageDirectory, packageVersion);
            if (!Directory.Exists(packageVersionDirectory))
                return null;

            return packageVersionDirectory;
        }
    }
}
