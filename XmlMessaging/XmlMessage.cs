using System.Xml;
using System.IO;


namespace XmlMessaging
{
    public abstract class XmlMessage
    {
        public XmlDocument GetXmlDocument()
        {
            XmlDocument xmlMessage = new XmlDocument();
            XmlElement root = xmlMessage.DocumentElement;

            XmlDeclaration xmlDeclaration = xmlMessage.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlMessage.InsertBefore(xmlDeclaration, root);

            CreateXmlDocumentBody(xmlMessage);

            return xmlMessage;
        }

        public byte[] ToByteArray()
        {
            XmlDocument xmlMessage = GetXmlDocument();

            byte[] xmlData = null;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlMessage.Save(memoryStream);
                xmlData = memoryStream.ToArray();
            }

            return xmlData;
        }

        abstract protected void CreateXmlDocumentBody(XmlDocument xmlMessage);
        abstract protected void LoadXmlDocument(XmlDocument xmlMessage);
    }
}
