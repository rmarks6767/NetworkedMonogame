using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace VRChat2
{
    class Server
    {
        /// <summary>
        /// The event that is associated with whether the multithreading process is done
        /// </summary>
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        /// <summary>
        /// The address the server is running at
        /// </summary>
        IPAddress address;

        /// <summary>
        /// The port at which the server is open
        /// </summary>
        int port;

        /// <summary>
        /// The server that will listen on a given port
        /// </summary>
        Socket server;

        /// <summary>
        /// The clients that have connected to the server
        /// </summary>
        List<Client> clients;

        /// <summary>
        /// Used for sending and receiving data
        /// </summary>
        Byte[] bytes;

        /// <summary>
        /// 
        /// </summary>
        NetworkStream ns;

        /// <summary>
        /// The number of clients connected to the server
        /// </summary>
        int globalNextId;

        /// <summary>
        /// To see if the server is still on or not
        /// </summary>
        bool serverOn;

        /// <summary>
        /// Used to make the initial server and take requests from the clients
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public Server(IPAddress address, int port)
        {
            //Assign the address and port to the server
            this.address = address;
            this.port = port;

            //A list of clients that have connected to the server
            clients = new List<Client>();

            //The data that can be sent back and forth between the server and the client
            bytes = new byte[256];

            //The server is a socket in which the clients may connect
            server = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //Set the server on and run the server
            serverOn = true;
            Console.WriteLine("Listening on port: {0}", port);
            Run();
        }

        /// <summary>
        /// Server opens up to receive input from the clients
        /// </summary>
        public void Run()
        {
            try
            {
                //Make an endpoint based on the address and port 
                IPEndPoint endpoint = new IPEndPoint(address, port);

                //Bind that Ip and port to the server 
                server.Bind(endpoint);

                //Start listening for any connections to the server
                server.Listen(port);

                //The server will run while it is on duh
                while (serverOn)
                {
                    //reset the async thingie
                    allDone.Reset();

                    //Open up the server to accept callbacks using the AcceptCallback function
                    Console.WriteLine("Damn connect already...");
                    server.BeginAccept(new AsyncCallback(AcceptCallback), server);

                    //make the async thing wait
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("OOF: " + e.Message);
            }
        }

        /// <summary>
        /// Sets the server to be something that can receive client requests asyncronously
        /// </summary>
        /// <param name="ar"></param>
        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                //tell the main thread to start again
                allDone.Set();

                //Get the socket that handles the client request
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                //Create the state object
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                //Read data from the client socket
                int read = handler.EndReceive(ar);

                //if there is any data from the client
                if (read > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    if (state.sb.Length > 1)
                    {
                        string content = state.sb.ToString();
                        Console.WriteLine("Read {0} from the socket", content);
                    }
                    handler.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            
        }
    }
}







/* Client client = AddClients();
                    NetworkStream ns = client.TClient.GetStream();

                    int i;
                    // Loop to receive all the data sent by the client.
                    while ((i = ns.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        byte[] msg = Encoding.ASCII.GetBytes("Hello connection: " + client.ID);

                        // Send back a response.
                        ns.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: Hello connection: {0}", client.ID);
                        break;
                    }
                    Console.WriteLine("REEEEEE");
                    client.TClient.Close();
                    ns.Close();

    public Client AddClients()
        {
            try
            {
                //Listening for any new connections
                TcpClient client = server.AcceptTcpClient();

                //Checking to see if that connection already exists in the list of connections
                if (clients.Count > 0)
                {
                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (clients[i].TClient == client)
                        {
  
                            return clients[i];
                        }
                    }
                    clients.Add(new Client(client, clients.Count + 1));
                    Console.WriteLine("Client added to the list of clients");
                    return clients[clients.Count - 1];
                }
                else
                {
                    clients.Add(new Client(client, clients.Count + 1));
                    Console.WriteLine("Client added to the list of clients");
                    return clients[clients.Count - 1];
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error right here: " + e.Message);
            }
            return null;
        }

        public void RemoveDisconnectedClients()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (!clients[i].TClient.Connected)
                {
                    Console.WriteLine("Removing client with id: " + clients[i].ID);
                    clients.Remove(clients[i]);
                }
            }
        }
    }
}
*/
  