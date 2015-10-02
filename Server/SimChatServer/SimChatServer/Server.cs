using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimChatServer
{
    public class Server
    {
        private ManualResetEvent Done = new ManualResetEvent(false);

        private IPEndPoint EndPoint = new IPEndPoint(IPAddress.Loopback, 10000);

        private Socket Listener;

        private List<Connection> Clients = new List<Connection>(100);

        public Server()
        {
            
        }

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
        }

        private void SendCallback(IAsyncResult ar)
        {
            Connection conn = (Connection) ar.AsyncState;

            conn.WorkSocket.EndSend(ar);
        }
    }
}