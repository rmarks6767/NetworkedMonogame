using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace VRChat2
{
    class Client
    {
        /// <summary>
        /// The threading part for the client
        /// </summary>
        public static ManualResetEvent connectDone;

        /// <summary>
        /// This will be the client that connects to the server
        /// </summary>
        Socket client;
        public Socket ClientSocket { get { return client; } }

        /// <summary>
        /// The end point of the address
        /// </summary>
        EndPoint ep;
        public EndPoint Ep { get { return ep; } }

        /// <summary>
        /// To handle receiving data
        /// </summary>
        byte[] received;

        /// <summary>
        /// How to actually send and receive data
        /// </summary>
        NetworkStream ns;

        /// <summary>
        /// The number that is associated with the client
        /// </summary>
        int id;
        public int ID { get { return id; } }

        /// <summary>
        /// The Client that will be created off of the given address and the port
        /// </summary>
        /// <param name="address">The address that the server is at</param>
        /// <param name="port">The port the address is at, the parking space</param>
        public Client(IPAddress address, int port)
        {
            client = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.IPv4);
        }

        /// <summary>
        /// Creating a reference to the actual client that exists out there somewhere in the world
        /// </summary>
        /// <param name="client">The socket that the client is connected to</param>
        /// <param name="clientNum">The id of that client so we can find it</param>
        public Client(Socket socket, int id)
        {
            this.client = socket;
            this.id = id;
        }

        public void Connect()
        {
            client.BeginAccept(new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();
        }

        /// <summary>
        /// How we establish a connection with the server, a constant callback
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                //Get the socket from the stateobject
                Socket client = (Socket)ar.AsyncState;

                //Complete the connection
                client.EndConnect(ar);

                //Set the thread to the current state
                connectDone.Set();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        private static void Send(String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
        }
    }
}
