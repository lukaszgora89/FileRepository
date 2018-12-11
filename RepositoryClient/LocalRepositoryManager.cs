using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace RepositoryClient
{
    class LocalRepositoryManager
    {
        readonly string DATA_FILE_PATH = "local_repository_data.dat";

        List<LocalRepository> _localRepositories = new List<LocalRepository>();

        public LocalRepositoryManager()
        {
            LoadRepositoriesData();
        }

        public IEnumerable<LocalRepository> LocalRepositories
        {
            get
            {
                return _localRepositories;
            }
        }

        public bool AddRepository(
            string repositoryName,
            string repositoryPath,
            bool noUpdate = false)
        {
            Console.WriteLine("INFO: Add repository '" + repositoryName + "' in '" + repositoryPath + "'.");

            // verify input data
            foreach (LocalRepository localRepo in _localRepositories)
            {
                if (localRepo.Name == repositoryName)
                {
                    Console.WriteLine("ERROR: Local repository '" + repositoryName + "' already exists!");
                    return false;
                }

                if (localRepo.Path == repositoryPath)
                {
                    Console.WriteLine("ERROR: Directory '" + repositoryName + "' is already used by ''" + localRepo.Name + "' repository!");
                    return false;
                }
            }

            // create repository directory
            string localRepositoryPath = Path.Combine(repositoryPath, repositoryName);
            Directory.CreateDirectory(localRepositoryPath);

            // add new repo entry
            _localRepositories.Add(new LocalRepository(repositoryName, repositoryPath));

            // update data file
            if (!noUpdate)
            {
                UpdateRepositoriesData();
            }

            return true;
        }

        public bool RemoveRepository(string repositoryName)
        {
            Console.WriteLine("INFO: Remove repository '" + repositoryName + "'.");

            LocalRepository repo = _localRepositories.SingleOrDefault(item => item.Name == repositoryName);
            if (repo != null)
            {
                // remove repository data from file system
                Directory.Delete(repo.Path, true);

                // remove repository from collection
                _localRepositories.Remove(repo);

                // update data file
                UpdateRepositoriesData();

                return true;
            }

            Console.WriteLine("ERROR: Repository '" + repositoryName + "' does not exists!");
            return false;
        }

        private void LoadRepositoriesData()
        {
            if (!File.Exists(DATA_FILE_PATH))
                return;

            Console.WriteLine("INFO: Load local repositories data.");

            string[] fileLines = File.ReadAllLines(DATA_FILE_PATH);

            foreach (string line in fileLines)
            {
                string[] repositoryData = line.Split('?');

                if (repositoryData.Length != 2)
                {
                    Console.WriteLine("FATAL ERROR: Invalid repository entry, skip - " + line);
                    continue;
                }

                if (!AddRepository(repositoryData.ElementAt(0), repositoryData.ElementAt(1), /*noUpdate*/ true))
                {
                    Console.WriteLine("ERROR: Cannot load local repository - " + repositoryData.ElementAt(0));
                    continue;
                }
            }
        }

        private void UpdateRepositoriesData()
        {
            Console.WriteLine("INFO: Save local repositories data.");

            if (File.Exists(DATA_FILE_PATH))
            {
                File.Delete(DATA_FILE_PATH);
            }

            using (StreamWriter fileStream = new StreamWriter(DATA_FILE_PATH))
            {
                foreach (LocalRepository localRepo in _localRepositories)
                {
                    fileStream.WriteLine(localRepo.Name + '?' + localRepo.Path);
                }
            }
        }
    }
}
