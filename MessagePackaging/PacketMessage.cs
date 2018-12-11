using System;
using System.Collections.Generic;


namespace MessagePackaging
{
    // 0   - 500  client messages
    // 501 - 1000 server messages
    public enum PacketMessageType : int
    {
        // client messages
        C_LIST_PACKAGES = 0,
        C_LIST_PACKAGE_VERSIONS = 1,
        C_GET_PACKAGE = 2,

        // server messages
        S_PACKAGES = 500,
        S_PACKAGE_VERSIONS = 501,
        S_PACKAGE_DATA_HEADER = 502,
        S_PACKAGE_FILE_BEGIN = 503,
        S_PACKAGE_FILE_DATA = 504,
        S_PACKAGE_FILE_END = 505,

        // server error messages
        S_ERROR_INVALID_PACKAGE_REQUEST = 600,

        // invalid
        I_INVALID = -1
    }

    public class PacketMessage
    {
        // [ TYPE | PART | DATA ]
        static readonly int TYPE_SIZE = sizeof(int);
        static readonly int PART_SIZE = sizeof(int);

        public PacketMessageType Type { get; private set; } = PacketMessageType.I_INVALID;
        public int Part { get; private set; } = 0;                                          // 0 based
        public byte[] Data { get; private set; } = null;


        public PacketMessage(PacketMessageType type, int part, byte[] data)
        {
            Type = type;
            Part = part;
            Data = data;
        }

        public PacketMessage(byte[] data)
        {
            if (data.Length < TYPE_SIZE)
                throw new System.Net.ProtocolViolationException("Message size is less than " + TYPE_SIZE + "bytes.");

            Type = (PacketMessageType)BitConverter.ToInt32(data, 0);

            if (data.Length >= (TYPE_SIZE + PART_SIZE))
            {
                Part = BitConverter.ToInt32(data, TYPE_SIZE);

                if (data.Length > (TYPE_SIZE + PART_SIZE))
                {
                    Data = new byte[data.Length - (TYPE_SIZE + PART_SIZE)];
                    Array.Copy(data, TYPE_SIZE + PART_SIZE, Data, 0, Data.Length);
                }
            }
        }

        public int GetMessageSize()
        {
            int messageSize = TYPE_SIZE + PART_SIZE;
            if (Data != null)
                messageSize += Data.Length;

            return messageSize;
        }

        public byte[] GetMessageData()
        {
            byte[] data = new byte[GetMessageSize()];

            // serialize type
            int typeIndex = (int)Type;
            BitConverter.GetBytes(typeIndex).CopyTo(data, 0);

            // serialize part
            BitConverter.GetBytes(Part).CopyTo(data, TYPE_SIZE);

            // append data
            if (Data != null)
                Data.CopyTo(data, TYPE_SIZE + PART_SIZE);

            return data;
        }

        public IEnumerable<PacketMessage> SplitIntoParts(int partSize)
        {
            // every part contains type and part number

            // TODO check and add comments!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // TODO what if Data.Length == 0

            int headerSize = TYPE_SIZE + PART_SIZE;
            int dataPartSize = partSize - headerSize;

            List<PacketMessage> messageParts = new List<PacketMessage>();

            int processedDataBytes = 0;
            int partNumber = 0;
            while (processedDataBytes != Data.Length)
            {
                int currentPartSize = dataPartSize;
                int bytesLeft = Data.Length - processedDataBytes;

                if (bytesLeft < dataPartSize)
                {
                    // decrease number of bytes to send when it is less than expected part size
                    currentPartSize = bytesLeft;
                }

                byte[] partData = new byte[currentPartSize];

                Array.Copy(Data, processedDataBytes, partData, 0, currentPartSize);
                messageParts.Add(new PacketMessage(Type, partNumber, partData));

                processedDataBytes += currentPartSize;
                partNumber++;
            }

            return messageParts;
        }
    }
}
