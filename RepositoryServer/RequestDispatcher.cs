using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using XmlMessaging;
using MessagePackaging;
using System.Security.Cryptography;


namespace RepositoryServer
{
    class RequestDispatcher
    {
        private RepositoryManager _repositoryManager;

        // private ConcurrentQueue<ClientRequest> _clientRequestsQueue = new ConcurrentQueue<ClientRequest>();

        public RequestDispatcher(RepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public void OnClientRequestReceived(object sender, ClientRequestReceivedEventArgs args)
        {
            ClientConnection clientConnection = sender as ClientConnection;
            if (clientConnection != null)
            {
                Dispatch(clientConnection, args.ClientRequest);
            }
            else
            {
                throw new ArgumentException("The 'sender' argument is not ClientConnection type");
            }
        }

        private void Dispatch(ClientConnection clientConnection, ClientRequest clientRequest)
        {
            Console.WriteLine("Message received: " + clientRequest.Message.Type.ToString());

            switch (clientRequest.Message.Type)
            {
                case PacketMessageType.C_LIST_PACKAGES:
                    ListPackages(clientConnection, clientRequest);
                    break;

                case PacketMessageType.C_LIST_PACKAGE_VERSIONS:
                    ListPackageVersions(clientConnection, clientRequest);
                    break;

                case PacketMessageType.C_GET_PACKAGE:
                    GetPackage(clientConnection, clientRequest);
                    break;

                default:
                    Console.WriteLine("WARNING: Skip unsupported message - " + clientRequest.Message.Type.ToString());
                    break;
            }
        }

        private void ListPackages(
            ClientConnection clientConnection,
            ClientRequest clientRequest)
        {
            XmlMessageListPackages listPackagesMessage = XmlMessageCreator.CreateXmlMessageListPackages();

            foreach (Repository repository in _repositoryManager.Repositories.Values)
            {
                List<string> packages = new List<string>();
                foreach (string package in repository.GetPackages())
                {
                    packages.Add(package);
                }

                listPackagesMessage.AddRepositoryPackages(repository.Name, packages);
            }

            SendClientXmlResponse(clientConnection, clientRequest, PacketMessageType.S_PACKAGES, listPackagesMessage);
        }

        private void ListPackageVersions(
            ClientConnection clientConnection,
            ClientRequest clientRequest)
        {
            // request
            XmlDocument xmlDocument = new XmlDocument();
            using (MemoryStream memoryStream = new MemoryStream(clientRequest.Message.Data))
            {
                xmlDocument.Load(memoryStream);
            }

            XmlMessageListPackages listPackagesMessage = XmlMessageCreator.CreateXmlMessageListPackages(xmlDocument);

            // response
            XmlMessageListPackageVersions packageVersions = XmlMessageCreator.CreateXmlMessageListPackageVersions();

            foreach (KeyValuePair<string, List<string>> repositoryItem in listPackagesMessage.Repositories)
            {
                Repository repository = _repositoryManager.GetRepository(repositoryItem.Key);
                foreach (string packageName in repositoryItem.Value)
                {
                    List<string> versions = repository.GetPackageVersions(packageName);
                    if (!packageVersions.AddRepositoryPackageVersions(repository.Name, packageName, versions))
                    {
                        Console.WriteLine("ERROR: Cannot add package versions!");
                    }
                }
            }

            SendClientXmlResponse(clientConnection, clientRequest, PacketMessageType.S_PACKAGE_VERSIONS, packageVersions);
        }

        private void GetPackage(
            ClientConnection clientConnection,
            ClientRequest clientRequest)
        {
            // request
            XmlDocument xmlDocument = new XmlDocument();
            using (MemoryStream memoryStream = new MemoryStream(clientRequest.Message.Data))
            {
                xmlDocument.Load(memoryStream);
            }

            XmlMessageGetPackage getPackageMessage = XmlMessageCreator.CreateXmlMessageGetPackage(xmlDocument);

            Console.WriteLine("INFO: Requested to send package:");
            Console.WriteLine("      * repository: " + getPackageMessage.RepositoryName);
            Console.WriteLine("      * package   : " + getPackageMessage.PackageName);
            Console.WriteLine("      * version   : " + getPackageMessage.Version);

            // response
            Repository repository = _repositoryManager.GetRepository(getPackageMessage.RepositoryName);
            if (repository == null)
            {
                // invalid request
                string reason = "Invalid repository name - " + getPackageMessage.RepositoryName;
                Console.WriteLine("ERROR: Cannot get package. " + reason);
                SendClientStringResponse(clientConnection, clientRequest, PacketMessageType.S_ERROR_INVALID_PACKAGE_REQUEST, reason);
                return;
            }

            string packageDataDir = repository.GetPackagePath(getPackageMessage.PackageName, getPackageMessage.Version);
            if (String.IsNullOrEmpty(packageDataDir))
            {
                // invalid request
                string reason = "Invalid package - " + getPackageMessage.PackageName + "(" + getPackageMessage.Version + ")";
                Console.WriteLine("ERROR: Cannot get package. " + reason);
                SendClientStringResponse(clientConnection, clientRequest, PacketMessageType.S_ERROR_INVALID_PACKAGE_REQUEST, reason);
                return;
            }

            // send package data
            SendPackageData(clientConnection, clientRequest, getPackageMessage, packageDataDir);
        }

        private void SendPackageData(
            ClientConnection clientConnection,
            ClientRequest clientRequest,
            XmlMessageGetPackage getPackageMessage,
            string packageDataDir)
        {
            DirectoryInfo packageDataInfo = new DirectoryInfo(packageDataDir);

            // send data package header
            XmlMessagePackageDataHeader packagedataHeader =
                XmlMessageCreator.CreateXmlMessagePackageDataHeader(
                    getPackageMessage.RepositoryName,
                    getPackageMessage.PackageName,
                    getPackageMessage.Version,
                    packageDataInfo.GetFiles().Length);

            SendClientXmlResponse(clientConnection, clientRequest, PacketMessageType.S_PACKAGE_DATA_HEADER, packagedataHeader);

            // send files
            foreach (FileInfo packageFileInfo in packageDataInfo.GetFiles())
            {
                SendPackageFile(clientConnection, clientRequest, packageFileInfo);
            }
        }

        private void SendPackageFile(
            ClientConnection clientConnection,
            ClientRequest clientRequest,
            FileInfo packageFileInfo)
        {
            // send file data
            const int FILE_CHUNK_SIZE = 32 * 1024 * 1024; // read the file by chunks of 32MB
            using (FileStream file = File.OpenRead(packageFileInfo.FullName))
            {
                // send file begin
                XmlMessagePackageFileBegin fileBeginMessage = XmlMessageCreator.CreateXmlMessagePackageFileBegin(
                    packageFileInfo.Name,
                    file.Length,
                    ComputeFileChecksum(packageFileInfo));

                SendClientXmlResponse(clientConnection, clientRequest, PacketMessageType.S_PACKAGE_FILE_BEGIN, fileBeginMessage);

                int part = 0;
                int bytesRead = 0;
                byte[] buffer = new byte[FILE_CHUNK_SIZE];
                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] dataToSend = buffer;

                    // extract only "bytesRead" number of bytes
                    if (bytesRead < buffer.Length)
                    {
                        dataToSend = new byte[bytesRead];
                        Array.Copy(buffer, dataToSend, bytesRead);
                    }

                    SendClientRawDataResponse(clientConnection, clientRequest, PacketMessageType.S_PACKAGE_FILE_DATA, dataToSend, part++);
                }
            }

            // send file end
            SendClientNoDataResponse(clientConnection, clientRequest, PacketMessageType.S_PACKAGE_FILE_END);
        }

        private void SendClientRawDataResponse(
            ClientConnection clientConnection,
            ClientRequest clientRequest,
            PacketMessageType messageType,
            byte[] data,
            int part)
        {
            PacketMessage message = new PacketMessage(messageType, part, data);
            ClientResponse clientResponse = new ClientResponse(clientRequest.Id, message);
            clientConnection.SendResponse(clientResponse);
        }

        private void SendClientNoDataResponse(
            ClientConnection clientConnection,
            ClientRequest clientRequest,
            PacketMessageType messageType)
        {
            SendClientRawDataResponse(clientConnection, clientRequest, messageType, null, 0);
        }


        private void SendClientXmlResponse(
            ClientConnection clientConnection,
            ClientRequest clientRequest,
            PacketMessageType messageType,
            XmlMessage xmlMessage)
        {
            SendClientRawDataResponse(clientConnection, clientRequest, messageType, xmlMessage.ToByteArray(), 0);
        }

        private void SendClientStringResponse(
            ClientConnection clientConnection,
            ClientRequest clientRequest,
            PacketMessageType messageType,
            string messageText)
        {
            byte[] data = Encoding.ASCII.GetBytes(messageText);
            SendClientRawDataResponse(clientConnection, clientRequest, messageType, data, 0);
        }

        private string ComputeFileChecksum(FileInfo packageFileInfo)
        {
            using (FileStream stream = File.OpenRead(packageFileInfo.FullName))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}
