using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.RawMessageProcessing
{
    public class RawMessageReader
    {
        public delegate int Read(byte[] buffer, int count, int startIndex);
        private readonly Read _byteStreamReader;
        private readonly byte[] _messageBuffer;
        private readonly byte[] _messageLengthBuffer;

        public RawMessageReader(Read byteStreamReader)
        {
            _byteStreamReader = byteStreamReader;
            _messageBuffer = new byte[8 * 1024];
            _messageLengthBuffer = new byte[2];
        }
        public string GetNextMessageAsString()
        {
            int allBytesRead = 0;
            while (allBytesRead != 2)
            {
                int bytesRead = _byteStreamReader(_messageLengthBuffer, 2 - allBytesRead, allBytesRead);
                if (bytesRead == 0)
                {
                    throw new Exception($"Failed to read len bytes, allBytesRead={allBytesRead}");
                }
                allBytesRead += bytesRead;
            }
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(_messageLengthBuffer);
            int messageLength = BitConverter.ToUInt16(_messageLengthBuffer, 0);
            if (messageLength == 0) return String.Empty;
            allBytesRead = 0;
            while(allBytesRead != messageLength)
            {
                int bytesRead = _byteStreamReader(_messageBuffer, messageLength-allBytesRead, allBytesRead);
                if (bytesRead == 0)
                {
                    throw new Exception($"Failed to read mes bytes, allBytesRead={allBytesRead}," +
                        $" messageLength={messageLength}");
                }
                allBytesRead += bytesRead;
            }
            return Encoding.UTF8.GetString(_messageBuffer, 0, messageLength);
        }
    }
}
