using System;
using System.IO;


namespace RepositoryServer
{
    class Program
    {
        private static RepositoryManager _repositoryManager = null;
        private static ClientManager _clientManager = null;

        static void Main(string[] args)
        {
            Console.Title = "File Repository Server";

            // parse input arguments
            if (args.Length != 1)
            {
                Console.WriteLine("ERROR: XML configuration file is expected.");
                Console.ReadLine();
                return;
            }

            string xmlConfigurationFilePath = args[0];
            if (!File.Exists(xmlConfigurationFilePath))
            {
                Console.WriteLine("ERROR: Specified XML configuration file does not exists - " + xmlConfigurationFilePath);
                Console.ReadLine();
                return;
            }

            // initialize files repository
            if (!InitializeRepository(xmlConfigurationFilePath))
            {
                Console.WriteLine("ERROR: Cannot initialize repository");
                Console.ReadLine();
                return;
            }

            // initialize server connection
            SetupServer();

            Console.ReadLine();
        }

        private static bool InitializeRepository(string xmlConfigurationFilePath)
        {
            Console.WriteLine("Initialize repository");

            // load configuration file
            XmlConfigurationLoader configutarionLoader = new XmlConfigurationLoader();

            if (!configutarionLoader.LoadConfiguration(xmlConfigurationFilePath))
            {
                Console.WriteLine("ERROR: Cannot load repository configuration - " + xmlConfigurationFilePath);
                return false;
            }

            // create repositories
            _repositoryManager = new RepositoryManager();
            foreach (XmlRepositoryConfiguration configuration in configutarionLoader.RepositoryConfigurations)
            {
                Console.WriteLine("Add repository: " + configuration.Name + "(" + configuration.Path + ")");
                _repositoryManager.AddRepository(configuration.Name, configuration.Path);
            }

            return true;
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");

            _clientManager = new ClientManager(_repositoryManager);

            Console.WriteLine("Start accept clients on port 32123");
            _clientManager.AcceptClients(32123);    // TODO
        }
    }
}
