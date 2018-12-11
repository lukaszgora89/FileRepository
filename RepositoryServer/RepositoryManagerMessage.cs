

namespace RepositoryServer
{
    // TODO should be removed?

    class RepositoryManagerMessage
    {
        private static readonly object _uniqueIdLock = new object();
        private static int _uniqueIdCounter = 0;

        public int ID { get; private set; }


        public RepositoryManagerMessage(int id)
        {
            ID = id;
        }

        public static int GetUniqueId()
        {
            lock(_uniqueIdLock)
            {
                return _uniqueIdCounter++;
            }
        }
    }
}
