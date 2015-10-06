using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;


/// <summary>
/// 
/// </summary>
namespace SimChatServer
{

    /// <summary>
    /// 
    /// </summary>
    public class Server
    {

        /// <summary>
        /// The done
        /// </summary>
        private ManualResetEvent Done = new ManualResetEvent(false);


        /// <summary>
        /// The end point
        /// </summary>
        private IPEndPoint EndPoint = new IPEndPoint(IPAddress.Loopback, 10000);


        /// <summary>
        /// The listener
        /// </summary>
        private Socket Listener;


        /// <summary>
        /// The clients
        /// </summary>
        private List<Connection> Clients = new List<Connection>(100);


        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        public Server()
        {
            
        }


        /// <summary>
        /// Listens this instance.
        /// </summary>
        public void Listen()
        {
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Listener.Bind(EndPoint);
                Listener.Listen(100);

                while (true)
                {
                    // Make the work signal nonsignaled
                    Done.Reset();

                    Listener.BeginAccept(AcceptCallback, Listener);

                    // Wait until we get a connection
                    Done.WaitOne();
                }
            }
            catch (Exception e)
            {
                
                Console.WriteLine(e.ToString());
            }
        }


        /// <summary>
        /// Accepts the callback.
        /// </summary>
        /// <param name="ar">The ar.</param>
        private void AcceptCallback(IAsyncResult ar)
        {
            Done.Set();

            Socket handler = Listener.EndAccept(ar);

            Connection conn = new Connection();
            conn.WorkSocket = handler;

            // Add the connect to our list so we can broadcast later
            Clients.Add(conn);

            handler.BeginReceive(conn.Buffer, 0, conn.Buffer.Length, 0, ReadCallback, conn);
        }


        /// <summary>
        /// Reads the callback.
        /// </summary>
        /// <param name="ar">The ar.</param>
        private void ReadCallback(IAsyncResult ar)
        {
            Connection conn = (Connection) ar.AsyncState;

            int bytesRead = conn.WorkSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                foreach (var client in Clients)
                {
                    client.WorkSocket.BeginSend(conn.Buffer, 0, bytesRead, 0, SendCallback, client);
                }
            }

            conn.WorkSocket.BeginReceive(conn.Buffer, 0, conn.Buffer.Length, 0, ReadCallback, conn);
        }


        /// <summary>
        /// Sends the callback.
        /// </summary>
        /// <param name="ar">The ar.</param>
        private void SendCallback(IAsyncResult ar)
        {
            Connection conn = (Connection) ar.AsyncState;

            int bytesWritten = conn.WorkSocket.EndSend(ar);
        }
    }
}