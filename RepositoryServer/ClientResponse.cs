using MessagePackaging;


namespace RepositoryServer
{
    class ClientResponse : ClientDataBase
    {
        public ClientResponse(int id, PacketMessage message)
            : base(id, message)
        {
        }
    }
}
