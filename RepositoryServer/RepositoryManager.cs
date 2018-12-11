using System;
using System.Collections.Generic;
using System.IO;


namespace RepositoryServer
{

    // TODO: Currently package name must be uniqie

    // TODO Split to repositories and packages

    /*
     * MANAGER
     * |
     * |---------- REPOSITORY1
     * |                |
     * |                |-------- Package1
     * |                |-------- Package2
     * |                '-------- Package3
     * |
     * '---------- REPOSITORY2
     *                  |
     *                  |-------- Package1
     *                  |-------- Package2
     *                  '-------- Package3
     */

    class RepositoryManager
    {
        public Dictionary<string, Repository> Repositories { get; private set; } = new Dictionary<string, Repository>();


        //public ConcurrentQueue<> MyProperty { get; set; }

        public RepositoryManager()
        {
        }

        public bool AddRepository(string name, string repositoryPath)
        {
            // TODO synchronize in case when you want to add and work on repositories

            if (!Directory.Exists(repositoryPath))
            {
                Console.WriteLine("ERROR: Cannot add repository - " + repositoryPath + " does not exists");
                return false;
            }

            if (Repositories.ContainsKey(name))
            {
                Console.WriteLine("ERROR: Repository " + name + "already exists");
                return false;
            }

            Repositories.Add(name, new Repository(name, repositoryPath));
            return true;
        }

        public Repository GetRepository(string name)
        {
            Repository repo = null;
            if (Repositories.TryGetValue(name, out repo))
                return repo;

            return null;
        }
    }
}
