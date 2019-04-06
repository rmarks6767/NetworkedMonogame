using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace VRChat2
{
    class Client
    {
        /// <summary>
        /// This will be the client that connects to the server
        /// </summary>
        TcpClient client;

        /// <summary>
        /// To handle receiving data
        /// </summary>
        byte[] received;

        /// <summary>
        /// How to actually send and receive data
        /// </summary>
        NetworkStream ns;

        /// <summary>
        /// The Client that will be created off of the given address and the port
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public Client(IPAddress address, int port)
        {
            client = new TcpClient(address.ToString(), port);
            EstablishConnection();
        }

        /// <summary>
        /// Used to send data to the server
        /// </summary>
        public void SendString(string s, NetworkStream netstr)
        {
            Byte [] bytes = Encoding.ASCII.GetBytes(s);
            netstr.Write(bytes, 0, bytes.Length);

            string bet;
            int stuff;

            while ((stuff = netstr.Read(bytes, 0, bytes.Length)) != 0)
            {
                bet = Encoding.ASCII.GetString(bytes, 0, stuff);
                Console.WriteLine(bet);
            }
        }

        /// <summary>
        /// Used in a trycatch because we may not find the server
        /// </summary>
        public void EstablishConnection()
        {
            try
            {
                ns = client.GetStream();
                SendString("Connection", ns);
            }
            catch (Exception e)
            {
                Console.WriteLine("Server not found: " + e.Message);
            }
        }


    }
}
