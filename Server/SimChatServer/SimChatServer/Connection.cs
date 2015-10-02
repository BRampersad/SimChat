using System.Net.Sockets;

namespace SimChatServer
{
    public class Connection
    {
        public Socket WorkSocket;
        public byte[] Buffer = new byte[1024];
    }
}