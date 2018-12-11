using MessagePackaging;


namespace RepositoryServer
{
    class ClientDataBase
    {
        public int Id { get; protected set; }
        public PacketMessage Message { get; protected set; }

        public ClientDataBase(int id, byte[] data)
        {
            Id = id;
            Message = new PacketMessage(data);
        }

        public ClientDataBase(int id, PacketMessage message)
        {
            Id = id;
            Message = message;
        }
    }
}
