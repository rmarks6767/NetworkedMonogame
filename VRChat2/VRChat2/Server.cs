using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace VRChat2
{
    class Server
    {
        /// <summary>
        /// The server that will listen on a given port
        /// </summary>
        TcpListener server;

        /// <summary>
        /// The clients that have connected to the server
        /// </summary>
        List<TcpClient> clients;

        /// <summary>
        /// Used for sending and receiving data
        /// </summary>
        Byte[] bytes;

        /// <summary>
        /// 
        /// </summary>
        NetworkStream ns;

        /// <summary>
        /// Used to make the initial server and take requests from the clients
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public Server(IPAddress address, int port)
        {
            clients = new List<TcpClient>();
            bytes = new byte[256];
            server = new TcpListener(address, port);
            server.Start();
            Console.WriteLine("Listening on port: {0}", port);
            ListenForClients();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ListenForClients()
        {
            try
            {
                string data = null;
                while (true)
                {
                    Console.WriteLine("Damn connect already...");

                    TcpClient client = server.AcceptTcpClient();

                    if (!clients.Contains(client))
                    {
                        clients.Add(client);
                        Console.WriteLine("Client added to the list of clients");
                    }


                    NetworkStream ns = client.GetStream();

                    int i;
                    // Loop to receive all the data sent by the client.
                    while((i = ns.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        ns.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("OOF: " + e.Message);
            }
        }
    }
}
