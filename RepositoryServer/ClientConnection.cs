using System;
using System.Net.Sockets;
using MessagePackaging;


namespace RepositoryServer
{
    // TODO add keepalive check
    // TODO przerobic na NetworkStream!!!

    class ClientRequestReceivedEventArgs : EventArgs
    {
        public ClientRequest ClientRequest { get; set; }
    }

    class ClientConnection
    {
        private const int BUFFER_SIZE = 1024 * 1024; // 1MB - this is also max packet size TODO: more? benchmark

        private Socket _clientSocket;
        private byte[] _socketBuffer = new byte[BUFFER_SIZE];
        private PacketManager _packetManager;

        public int Id { get; private set; }

        public event EventHandler<ClientRequestReceivedEventArgs> ClientRequestReceived;
        public event EventHandler ClientConnectionClosed;

        public ClientConnection(int id, Socket socket)
        {
            Id = id;
            _clientSocket = socket;

            _packetManager = new PacketManager(BUFFER_SIZE);
            _packetManager.DataPacketReceived += OnDataPacketReceived;

            _clientSocket.BeginReceive(_socketBuffer, 0, _socketBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }

        public bool SendResponse(ClientResponse response)
        {
            if (response.Message.GetMessageSize() > _packetManager.GetMaxDataFieldSize())
            {
                // split response into packets
                foreach (PacketMessage messagePart in response.Message.SplitIntoParts(_packetManager.GetMaxDataFieldSize()))
                {
                    byte[] packet = _packetManager.CreateMessage(messagePart.GetMessageData());
                    _clientSocket.BeginSend(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(SendCallback), _clientSocket);
                }
            }
            else
            {
                // response can be send as single packet
                byte[] packet = _packetManager.CreateMessage(response.Message.GetMessageData());
                _clientSocket.BeginSend(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(SendCallback), _clientSocket);
            }

            return true;
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;

            try
            {
                int received = socket.EndReceive(AR);
                byte[] dataBuf = new byte[received];
                Array.Copy(_socketBuffer, dataBuf, received);

                // TODO mozna przyspieszyc zeby czytac od razu z bufora glownego

                _packetManager.AddReceivedData(dataBuf);

                socket.BeginReceive(_socketBuffer, 0, _socketBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _clientSocket);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Socket exception occured: " + ex.Message);
                OnClientConnectionClosed();
            }
        }

        private void OnDataPacketReceived(object source, DataPacketReceivedEventArgs args)
        {
            ClientRequest clientRequest = new ClientRequest(Id, args.Data);
            OnClientRequestReceived(clientRequest);
        }

        private void OnClientRequestReceived(ClientRequest clientRequest)
        {
            if (ClientRequestReceived != null)
                ClientRequestReceived(this, new ClientRequestReceivedEventArgs() { ClientRequest = clientRequest });
        }

        private void OnClientConnectionClosed()
        {
            if (ClientConnectionClosed != null)
                ClientConnectionClosed(this, null);
        }

        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
    }
}
