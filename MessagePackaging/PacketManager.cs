using System;


namespace MessagePackaging
{
    public class DataPacketReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }



    // size = 4bytes  -  stores number of bytes to read
    // data = nbytes

    public class PacketManager
    {
        // Event creation:
        // 1 - define delegate
        // 2 - define an event based on that delegate
        // 3 - raise the event

        // 1
        public delegate void KeepalivePacketReveivedEventHandler(object source, EventArgs args);
        public delegate void DataPacketReceivedEventHandler(object source, DataPacketReceivedEventArgs args);

        // EventHandler
        // EventHandler<TEventHandler>
        // public event EventHandler<DataPacketReceivedEventArgs> DataPacketReceived;


        // 2
        public event KeepalivePacketReveivedEventHandler KeepalivePacketReceived;
        public event DataPacketReceivedEventHandler DataPacketReceived;


        private byte[] sizeFieldBuffer;
        private byte[] dataFieldBuffer;
        private int receivedPacketBytesCounter;

        public int SizeFieldLenght { get; } = sizeof(int);
        public int MinDataFieldLenght { get; } = sizeof(int);  // TODO not 0?
        public int MaxPacketSize { get; private set; } = 1024; // 1Kb - replaced in constructor

        // returns 4 byte keepalive message
        // [size=0][data=N/A]
        public static byte[] GetKeepAliveMessage()
        {
            return BitConverter.GetBytes(0);
        }

        public PacketManager(int maxPacketSize)
        {
            if (maxPacketSize < GetMinimumPacketSize())
            {
                throw new System.Net.ProtocolViolationException(
                    "Minimum packet size cannot be less than " + GetMinimumPacketSize().ToString());
            }

            MaxPacketSize = maxPacketSize;
            sizeFieldBuffer = new byte[SizeFieldLenght];
            dataFieldBuffer = null;
            receivedPacketBytesCounter = 0;
        }

        // TODO desc
        public int GetMaxDataFieldSize()
        {
            return MaxPacketSize - SizeFieldLenght;
        }

        // crate packed message - [size][data]
        // throws an exception when data is too big
        public byte[] CreateMessage(byte[] data)
        {
            if (data.Length > GetMaxDataFieldSize())
            {
                throw new System.Net.ProtocolViolationException(
                    "Data size " + data.Length.ToString() + " exceeds maximum size "
                    + GetMaxDataFieldSize().ToString() + " for packet creation");
            }

            // get 4bytes of message size
            byte[] sizeField = BitConverter.GetBytes(data.Length);

            // create full packet: size + data
            byte[] packet = new byte[sizeField.Length + data.Length];
            sizeField.CopyTo(packet, 0);
            data.CopyTo(packet, sizeField.Length);

            return packet;
        }

        // add received data for depacking
        // 
        public void AddReceivedData(byte[] data)
        {
            int processedBytesCounter = 0;

            while (processedBytesCounter != data.Length)
            {
                int availableBytesCounter = data.Length - processedBytesCounter;

                if (dataFieldBuffer == null)
                {
                    // size fild is not decoded yet because buffer was not created

                    // check if something was already received for the size field
                    int bytesToCopy = sizeFieldBuffer.Length - receivedPacketBytesCounter;
                    bytesToCopy = Math.Min(bytesToCopy, availableBytesCounter);

                    // copy size bytes to the field buffer
                    Array.Copy(data, processedBytesCounter, sizeFieldBuffer, receivedPacketBytesCounter, bytesToCopy);
                    processedBytesCounter += bytesToCopy;
                    receivedPacketBytesCounter += bytesToCopy;

                    ProcessSizeBuffer();
                }
                else
                {
                    // size field has been decoded and we can read data now
                    // buffer is already created

                    // check if something was already received for the data field
                    int bytesToCopy = dataFieldBuffer.Length - receivedPacketBytesCounter;
                    bytesToCopy = Math.Min(bytesToCopy, availableBytesCounter);

                    // copy data bytes to the field buffer
                    Array.Copy(data, processedBytesCounter, dataFieldBuffer, receivedPacketBytesCounter, bytesToCopy);
                    processedBytesCounter += bytesToCopy;
                    receivedPacketBytesCounter += bytesToCopy;

                    ProcessDataBuffer();
                }
            }
        }

        private void ProcessSizeBuffer()
        {
            // check if whole field is available
            if (receivedPacketBytesCounter != SizeFieldLenght)
                return; // size field is not complete

            // get packet data field length
            int dataFieldLength = BitConverter.ToInt32(sizeFieldBuffer, 0);

            // reset bytes counter
            receivedPacketBytesCounter = 0;

            // check keepalive packet
            if (dataFieldLength == 0)
            {
                // raise event
                OnKeepalivePacketReveived();
            }
            else
            {
                // validate size field
                if (dataFieldLength < MinDataFieldLenght)
                {
                    throw new System.Net.ProtocolViolationException("Received packet data size is less than 0");
                }

                if (dataFieldLength > GetMaxDataFieldSize())
                {
                    throw new System.Net.ProtocolViolationException(
                        "Received packet data size is greeter than max: " + GetMaxDataFieldSize().ToString());
                }

                dataFieldBuffer = new byte[dataFieldLength];
            }
        }

        protected virtual void OnKeepalivePacketReveived()
        {
            if (KeepalivePacketReceived != null)
                KeepalivePacketReceived(this, EventArgs.Empty);
        }

        // TODO desc
        private void ProcessDataBuffer()
        {
            // check if whole field is available
            if (receivedPacketBytesCounter != dataFieldBuffer.Length)
                return; // data field is not complete

            // raise event
            OnDataPacketReceived();

            // reset state before next receiving
            dataFieldBuffer = null;
            receivedPacketBytesCounter = 0;
        }

        protected virtual void OnDataPacketReceived()
        {
            if (DataPacketReceived != null)
                DataPacketReceived(this, new DataPacketReceivedEventArgs() { Data = dataFieldBuffer });
        }

        // TODO desc
        private int GetMinimumPacketSize()
        {
            return SizeFieldLenght + MinDataFieldLenght;
        }
    }
}
