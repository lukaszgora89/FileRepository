using System;
using System.IO;
using System.Xml;
using XmlMessaging;
using MessagePackaging;


namespace RepositoryClient
{
    class MessageDispatcher
    {
        public event EventHandler<XmlMessageListPackages> ListPackagesMessageReceived;
        public event EventHandler<XmlMessageListPackageVersions> ListPackageVersionsReceived;
        public event EventHandler<XmlMessagePackageDataHeader> PackageDataHeaderReceived;
        public event EventHandler<XmlMessagePackageFileBegin> PackageFileDataBeginReceived;
        public event EventHandler<PacketMessage> PackageFileDataReceived;
        public event EventHandler PackageFileDataEndReceived;
        public event EventHandler<PacketMessage> InvalidMessageReceived;


        public MessageDispatcher()
        {
        }

        public void OnDataPacketReceived(object source, DataPacketReceivedEventArgs args)
        {
            try
            {
                PacketMessage packetMessage = new PacketMessage(args.Data);

                switch(packetMessage.Type)
                {
                    case PacketMessageType.S_PACKAGES:
                        OnListPackagesMessageReceived(packetMessage);
                        break;

                    case PacketMessageType.S_PACKAGE_VERSIONS:
                        OnListPackageVersionsReceived(packetMessage);
                        break;

                    case PacketMessageType.S_PACKAGE_DATA_HEADER:
                        OnPackageDataHeaderReceived(packetMessage);
                        break;

                    case PacketMessageType.S_PACKAGE_FILE_BEGIN:
                        OnPackageFileDataBeginReceived(packetMessage);
                        break;

                    case PacketMessageType.S_PACKAGE_FILE_DATA:
                        OnPackageFileDataReceived(packetMessage);
                        break;

                    case PacketMessageType.S_PACKAGE_FILE_END:
                        OnPackageFileDataEndReceived(packetMessage);
                        break;

                    default:
                        OnInvalidMessageReceived(packetMessage);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION ERROR: Cannot dispatch message - " + ex.Message);
            }
        }

        private void OnListPackagesMessageReceived(PacketMessage packetMessage)
        {
            XmlDocument xmlDocument = CreateXmlDocumentFromMessage(packetMessage);
            //Console.WriteLine("DEBUG: Received S_PACKAGES: " + xmlDocument.OuterXml);
            XmlMessageListPackages message = XmlMessageCreator.CreateXmlMessageListPackages(xmlDocument);

            if (ListPackagesMessageReceived != null)
                ListPackagesMessageReceived(this, message);
        }

        private void OnListPackageVersionsReceived(PacketMessage packetMessage)
        {
            XmlDocument xmlDocument = CreateXmlDocumentFromMessage(packetMessage);
            //Console.WriteLine("DEBUG: Received S_PACKAGE_VERSIONS: " + xmlDocument.OuterXml);
            XmlMessageListPackageVersions message = XmlMessageCreator.CreateXmlMessageListPackageVersions(xmlDocument);

            if (ListPackageVersionsReceived != null)
                ListPackageVersionsReceived(this, message);
        }

        private void OnPackageDataHeaderReceived(PacketMessage packetMessage)
        {
            XmlDocument xmlDocument = CreateXmlDocumentFromMessage(packetMessage);
            //Console.WriteLine("DEBUG: Received S_PACKAGE_DATA_HEADER: " + xmlDocument.OuterXml);
            XmlMessagePackageDataHeader message = XmlMessageCreator.CreateXmlMessagePackageDataHeader(xmlDocument);

            if (PackageDataHeaderReceived != null)
                PackageDataHeaderReceived(this, message);
        }

        private void OnPackageFileDataBeginReceived(PacketMessage packetMessage)
        {
            XmlDocument xmlDocument = CreateXmlDocumentFromMessage(packetMessage);
            //Console.WriteLine("DEBUG: Received S_PACKAGE_FILE_BEGIN: " + xmlDocument.OuterXml);
            XmlMessagePackageFileBegin message = XmlMessageCreator.CreateXmlMessagePackageFileBegin(xmlDocument);

            if (PackageFileDataBeginReceived != null)
                PackageFileDataBeginReceived(this, message);
        }

        private void OnPackageFileDataReceived(PacketMessage packetMessage)
        {
            //Console.WriteLine("DEBUG: Received S_PACKAGE_FILE_DATA part " + packetMessage.Part);

            if (PackageFileDataReceived != null)
                PackageFileDataReceived(this, packetMessage);
        }

        private void OnPackageFileDataEndReceived(PacketMessage packetMessage)
        {
            //Console.WriteLine("DEBUG: Received S_PACKAGE_FILE_END");

            if (PackageFileDataEndReceived != null)
                PackageFileDataEndReceived(this, null);
        }

        private void OnInvalidMessageReceived(PacketMessage packetMessage)
        {
            Console.WriteLine("ERROR: Invalid message type received - " + packetMessage.Type.ToString());
            //Console.WriteLine("DEBUG: Received invalid message");

            if (InvalidMessageReceived != null)
                InvalidMessageReceived(this, packetMessage);
        }

        private XmlDocument CreateXmlDocumentFromMessage(PacketMessage packetMessage)
        {
            XmlDocument xmlDocument = new XmlDocument();

            using (MemoryStream memoryStream = new MemoryStream(packetMessage.Data))
            {
                xmlDocument.Load(memoryStream);
            }

            return xmlDocument;
        }
    }
}
