using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace RepositoryServer
{
    class ClientManager
    {
        const int LISTENING_SOCKETS = 2;

        static int _uniqueClientId;

        private List<ClientConnection> _clientConnections = new List<ClientConnection>();   // TODO Dictionary?
        private Socket _serverSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private RequestDispatcher _requestDispatcher;

        public ClientManager(RepositoryManager repositoryManager)
        {
            _requestDispatcher = new RequestDispatcher(repositoryManager);
        }

        public void AcceptClients(int listeningPort)
        {
            try
            {
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, listeningPort));
                _serverSocket.Listen(LISTENING_SOCKETS);
                _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("EXCEPTION ERROR: Cannot start accepting clients - " + ex.Message);
            }
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);

            Console.WriteLine("Connecting client...");
            ClientConnection clientConnection = new ClientConnection(GetUniqueClientId(), socket);

            // register events
            clientConnection.ClientRequestReceived += _requestDispatcher.OnClientRequestReceived;
            clientConnection.ClientConnectionClosed += OnClientConnectionClosed;

            _clientConnections.Add(clientConnection);
            Console.WriteLine("Client connected!");

            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private int GetUniqueClientId()
        {
            return _uniqueClientId++;
        }

        private void OnClientConnectionClosed(object source, EventArgs args)
        {
            // TODO lock here to prevent mulitple close at the same time

            Console.WriteLine("Disconnect client.");

            //_clientConnections.
        }
    }
}
