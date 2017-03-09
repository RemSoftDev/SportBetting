using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace SportRadar.Common.Windows
{
    public class MyStreamClass
    {
        public NetworkStream NetworkStream;
        public SslStream SslStream;

        public int Read(byte[] buffer, int offset, int length)
        {
            if (NetworkStream != null)
                return NetworkStream.Read(buffer, offset, length);
            return SslStream.Read(buffer, offset, length);
        }

        public void Write(byte[] buffer, int offset, int length)
        {
            if (NetworkStream != null)
                NetworkStream.Write(buffer, offset, length);
            else { SslStream.Write(buffer, offset, length); }

        }

        public Stream CurrentStream { get { return (Stream)SslStream ?? NetworkStream; } }

        public void Flush()
        {
            if (NetworkStream != null)
                NetworkStream.Flush();
            else
            {
                SslStream.Flush();

            }
        }

        public void Dispose()
        {
            if (NetworkStream != null)
                NetworkStream.Dispose();
            if (SslStream != null) { SslStream.Dispose(); }

        }
    }
}
