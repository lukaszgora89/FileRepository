using System;
using System.Collections.Generic;
using System.IO;


namespace RepositoryClient
{
    //  TODO merge with server Repository class!!!

    class LocalRepository
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        public LocalRepository(
            string name,
            string path)
        {
            Name = name;
            Path = path;
        }

        public IEnumerable<string> GetPackages()
        {
            DirectoryInfo repositoryDirectoryInfo = new DirectoryInfo(Path);
            List<string> packagesList = new List<string>();

            if (repositoryDirectoryInfo.Exists)
            {
                foreach (DirectoryInfo directory in repositoryDirectoryInfo.GetDirectories())
                    packagesList.Add(directory.Name);
            }
            else
            {
                Console.WriteLine("ERROR: Repository path does not exists - " + Path);
            }

            return packagesList;
        }

        public IEnumerable<string> GetPackageVersions(string packageName)
        {
            DirectoryInfo packageDirectory = new DirectoryInfo(System.IO.Path.Combine(Path, packageName));
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
    }
}
