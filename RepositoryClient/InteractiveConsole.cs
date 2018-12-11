using System;


namespace RepositoryClient
{
    class InteractiveConsole
    {
        RequestManager _requestManager = null;

        public InteractiveConsole(RequestManager requestManager)
        {
            _requestManager = requestManager;
        }

        public void run()
        {
            Console.WriteLine("Type \"help\" for more support.");

            while (true)
            {
                Console.WriteLine("");
                Console.WriteLine("Enter a request: ");
                string req = Console.ReadLine();

                if (req.ToUpper() == "EXIT")
                    return; // exit - send close message

                if (req.ToUpper() == "HELP")
                {
                    Console.WriteLine("ADD    - add local repository");
                    Console.WriteLine("DEL    - remove local repository");
                    Console.WriteLine("STATUS - local repository status");
                    Console.WriteLine("GET    - get repositories");
                    Console.WriteLine("GETV   - get package versions");
                    Console.WriteLine("GETP   - get package");
                    Console.WriteLine("EXIT   - exit client");
                }
                else if (req.ToUpper() == "ADD")
                {
                    Console.WriteLine("Enter repository name:");
                    string repositoryName = Console.ReadLine();

                    Console.WriteLine("Enter repository path:");
                    string repositoryPath = Console.ReadLine();

                    if (!_requestManager.AddLocalRepository(repositoryName, repositoryPath))
                    {
                        Console.WriteLine("ERROR: Cannot add local repository!");
                    }
                }
                else if (req.ToUpper() == "DEL")
                {
                    Console.WriteLine("Enter repository name:");
                    string repositoryName = Console.ReadLine();

                    if (!_requestManager.RemoveLocalRepository(repositoryName))
                    {
                        Console.WriteLine("ERROR: Cannot remove local repository!");
                    }
                }
                else if (req.ToUpper() == "STATUS")
                {
                    _requestManager.LocalRepositoryStatus();
                }
                else if (req.ToUpper() == "GET")
                {
                    _requestManager.GetRepositoriesList();
                }
                else if (req.ToUpper() == "GETV")
                {
                    Console.WriteLine("Enter repository name:");
                    string repositoryName = Console.ReadLine();

                    Console.WriteLine("Enter package name(separator - ,):");
                    string packageNames = Console.ReadLine();

                    _requestManager.GetPackageVersions(repositoryName, packageNames);
                }
                else if (req.ToUpper() == "GETP")
                {
                    Console.WriteLine("Enter repository name:");
                    string repositoryName = Console.ReadLine();

                    Console.WriteLine("Enter package name:");
                    string packageName = Console.ReadLine();

                    Console.WriteLine("Enter version:");
                    string version = Console.ReadLine();

                    // TODO - check if it is not in repository

                    if (!_requestManager.GetPackage(repositoryName, packageName, version))
                    {
                        Console.WriteLine("ERROR: Specified respository is not local - " + repositoryName);
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: Invalid request - " + req);
                }
            }
        }
    }
}
