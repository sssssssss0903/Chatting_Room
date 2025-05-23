using System;
using System.Net.Sockets;
using System.Text;

namespace WeChattingClient
{
    public static class TcpConnectionManager
    {
        private static TcpClient client;
        private static NetworkStream stream;

        public static bool IsConnected => client != null && client.Connected;
        public static TcpClient Client => client;
        public static NetworkStream Stream => stream;

        public static bool Connect(string host = "127.0.0.1", int port = 8888)
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    client = new TcpClient();
                    client.Connect(host, port);
                    stream = client.GetStream();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Send(byte[] data)
        {
            if (stream != null && stream.CanWrite)
                stream.Write(data, 0, data.Length);
        }

        public static int Read(byte[] buffer, int offset, int size)
        {
            int totalRead = 0;
            while (totalRead < size)
            {
                int bytesRead = stream.Read(buffer, offset + totalRead, size - totalRead);
                if (bytesRead == 0)
                {
                    // 对方断开连接
                    throw new Exception("连接已关闭");
                }
                totalRead += bytesRead;
            }
            return totalRead;
        }


        public static void Close()
        {
            stream?.Close();
            client?.Close();
            client = null;
            stream = null;
        }
    }

}
