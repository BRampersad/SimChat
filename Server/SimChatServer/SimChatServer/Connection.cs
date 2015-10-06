using System.Net.Sockets;


namespace SimChatServer
{

    /// <summary>
    /// 
    /// </summary>
    public class Connection
    {

        /// <summary>
        /// The work socket
        /// </summary>
        public Socket WorkSocket;

        /// <summary>
        /// The buffer
        /// </summary>
        public byte[] Buffer = new byte[1024];
    }
}