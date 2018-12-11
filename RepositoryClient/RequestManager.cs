using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using XmlMessaging;
using MessagePackaging;


namespace RepositoryClient
{
    class RequestManager
    {
        private static readonly int BUFFER_SIZE = 1024 * 1024; // 1MB - this is also max packet size TODO: more? benchmark
        private byte[] _socketBuffer = new byte[BUFFER_SIZE];
        private Socket _clientSocket = null;

        private PacketManager _packetManager = null;
        private PackageBuilder _packetBuilder = null;
        private MessageDispatcher _messageDispatcher = null;
        private LocalRepositoryManager _localRepositoryManager = null;

        public RequestManager()
        {
            // create supporting objects
            _clientSocket = new Socket
                (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _packetManager = new PacketManager(BUFFER_SIZE);
            _packetBuilder = new PackageBuilder(/*verify checksum*/ true);
            _messageDispatcher = new MessageDispatcher();
            _localRepositoryManager = new LocalRepositoryManager();

            // add message receiver
            _packetManager.DataPacketReceived += _messageDispatcher.OnDataPacketReceived;

            // set message events
            _messageDispatcher.ListPackagesMessageReceived += OnListPackagesMessageReceived;
            _messageDispatcher.ListPackageVersionsReceived += OnListPackageVersionsReceived;

            _messageDispatcher.PackageDataHeaderReceived += _packetBuilder.OnFileHeaderReceive;
            _messageDispatcher.PackageFileDataBeginReceived += _packetBuilder.OnFileBeginReceive;
            _messageDispatcher.PackageFileDataReceived += _packetBuilder.OnFileDataReceive;
            _messageDispatcher.PackageFileDataEndReceived += _packetBuilder.OnFileEndReceive;
            _messageDispatcher.InvalidMessageReceived += _packetBuilder.OnInvalidMessageReceive;

            _packetBuilder.FileReciveBegin += OnFileReciveBegin;
            _packetBuilder.FileReciveUpdate += OnFileReciveUpdate;
            _packetBuilder.FileReciveEnd += OnFileReciveEnd;
        }

        public void Connect(int port)
        {
            int attempts = 0;

            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect(IPAddress.Loopback, port);
                }
                catch (SocketException)
                {
                    Console.Write("\r");
                    Console.Write("Connection attempts: " + attempts.ToString());
                }
            }

            Console.WriteLine("\n");
            Console.WriteLine("Connected");
        }

        public bool AddLocalRepository(
            string repositoryName,
            string repositoryPath)
        {
            return _localRepositoryManager.AddRepository(repositoryName, repositoryPath);
        }

        public bool RemoveLocalRepository(string repositoryName)
        {
            return _localRepositoryManager.RemoveRepository(repositoryName);
        }

        public void LocalRepositoryStatus()
        {
            Console.WriteLine("");
            Console.WriteLine("List of local repositories:");

            foreach (LocalRepository repository in _localRepositoryManager.LocalRepositories)
            {
                Console.WriteLine(" * name    : " + repository.Name);
                Console.WriteLine("   path    : " + repository.Path);
                Console.WriteLine("   packages: ");

                foreach (string package in repository.GetPackages())
                {
                    Console.WriteLine("            `-> " + package);

                    foreach (string version in repository.GetPackageVersions(package))
                    {
                        Console.WriteLine("            |      - " + version);
                    }
                }

                Console.WriteLine("");
            }
        }

        public void GetRepositoriesList()
        {
            // create
            PacketMessage packetMessage = new PacketMessage(PacketMessageType.C_LIST_PACKAGES, 0, null);
            byte[] packet = _packetManager.CreateMessage(packetMessage.GetMessageData());

            // send
            _clientSocket.Send(packet);

            // get response
            int rec = _clientSocket.Receive(_socketBuffer);
            byte[] data = new byte[rec];
            Array.Copy(_socketBuffer, data, rec);
            _packetManager.AddReceivedData(data);
        }

        public void GetPackageVersions(
            string repositoryName,
            string packageNames)
        {
            XmlMessageListPackages listPackagesMessage = XmlMessageCreator.CreateXmlMessageListPackages();

            List<string> packages = packageNames.Split(',').ToList<string>();
            listPackagesMessage.AddRepositoryPackages(repositoryName, packages);


            // create packet
            PacketMessage packetMessage =
                new PacketMessage(PacketMessageType.C_LIST_PACKAGE_VERSIONS, 0, listPackagesMessage.ToByteArray());
            byte[] packet = _packetManager.CreateMessage(packetMessage.GetMessageData());

            // send
            _clientSocket.Send(packet);

            // get response
            int rec = _clientSocket.Receive(_socketBuffer);
            byte[] data = new byte[rec];
            Array.Copy(_socketBuffer, data, rec);
            _packetManager.AddReceivedData(data);
        }

        public bool GetPackage(
            string repositoryName,
            string packageName,
            string version)
        {
            // verify repository
            LocalRepository requestedRepository = _localRepositoryManager.LocalRepositories.FirstOrDefault(
                localRepository => localRepository.Name == repositoryName);
            if (requestedRepository != null)
            {
                XmlMessageGetPackage getPackageMessage =
                    XmlMessageCreator.CreateXmlMessageGetPackage(repositoryName, packageName, version);


                // create packet
                PacketMessage packetMessage =
                    new PacketMessage(PacketMessageType.C_GET_PACKAGE, 0, getPackageMessage.ToByteArray());
                byte[] packet = _packetManager.CreateMessage(packetMessage.GetMessageData());

                // setup package builder and send request
                _packetBuilder.StartReceiving(requestedRepository.Path);
                _clientSocket.Send(packet);

                // get response
                Console.WriteLine("INFO: Waiting for incoming data...");

                do
                {
                    int rec = _clientSocket.Receive(_socketBuffer);
                    byte[] data = new byte[rec];
                    Array.Copy(_socketBuffer, data, rec);
                    _packetManager.AddReceivedData(data);

                    Thread.Sleep(10);  // TODO must be removed
                }
                while (_packetBuilder.IsReceiving());

                return true;
            }
            else
            {
                return false;
            }

        }

        private static void OnListPackagesMessageReceived(object sender, XmlMessageListPackages message)
        {
            foreach (KeyValuePair<string, List<string>> repository in message.Repositories)
            {
                Console.WriteLine("# Packages of '" + repository.Key + "' repository:");
                foreach (string package in repository.Value)
                {
                    Console.WriteLine("   * " + package);
                }
            }
        }

        private static void OnListPackageVersionsReceived(object sender, XmlMessageListPackageVersions message)
        {
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> repository in message.Repositories)
            {
                Console.WriteLine("# Packages versions of '" + repository.Key + "' repository:");
                foreach (KeyValuePair<string, List<string>> package in repository.Value)
                {
                    Console.WriteLine("   * " + package.Key);
                    foreach (string version in package.Value)
                    {
                        Console.WriteLine("      - " + version);
                    }
                }
            }
        }

        private static void OnFileReciveBegin(object sender, string fileName)
        {
            Console.WriteLine("");
            Console.WriteLine("# Reciving file: " + fileName);
        }

        private static void OnFileReciveUpdate(object sender, FileReciveUpdateEventArgs args)
        {
            Console.Write("\r");
            Console.Write(DrawProgressBar(args.CurrentSize, args.ExpectedSize));
        }

        private static void OnFileReciveEnd(object sender, string fileName)
        {
            Console.WriteLine("");
            Console.WriteLine("Verifying checksum...");
        }

        private static string DrawProgressBar(long currentProgress, long totalProgress)
        {
            int TOTAL_BAR_SIZE = 50;

            // percentage part
            int percent = 0;

            if (totalProgress != 0)
                percent = (int)((currentProgress * 100) / totalProgress);

            // graphic part
            int divFactor = 100 / TOTAL_BAR_SIZE;
            int hashes = percent / divFactor;
            int filling = TOTAL_BAR_SIZE - hashes;

            string progress = String.Format("{0}{1}", new string('#', hashes), new string(' ', filling));

            // return nice progress bar
            return String.Format("[{0}] {1}%", progress, percent.ToString());
        }
    }
}
