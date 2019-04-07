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
        public static ManualResetEvent sendComplete = new ManualResetEvent(false);

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
            globalNextId = 0;

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
                    Console.WriteLine("Awaiting connection...");
                    server.BeginAccept(new AsyncCallback(AcceptCallback), server);

                    //make the async thing wait
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Server Running Error: " + e.Message);
            }
        }

        /// <summary>
        /// Sets the server to be something that can receive client requests asyncronously
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptCallback(IAsyncResult ar)
        {
            //Get the socket that handles the client request
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            try
            {
                //tell the main thread to start again
                allDone.Set();

                // We are now going to add the client connections to a list and create players for them if they don't already have a connection, we
                // will also see if they are disconnected or not
                if (!clients.Exists(c => c.ClientSocket == listener))
                {
                    clients.Add(new Client(handler, globalNextId));
                    globalNextId++;
                    Console.WriteLine("Client {0} connected", clients[clients.Count - 1].ID);
                    Send(Command.SendingClientInfo, clients[clients.Count - 1], handler);
                }
                else
                {
                    Console.WriteLine("REEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
                    //Create the state object
                    StateObject state = new StateObject();
                    state.workSocket = handler;
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("AcceptCallback Error: " + e.Message);
                RemoveClient(listener);
            }
            
        }

        /// <summary>
        /// Reads what the client says back to the server, this will handle what client sent stuff and 
        /// what player to move in corespondance to such a thing
        /// </summary>
        /// <param name="ar"></param>
        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("Now we're waiting at the ReadCallback");

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
                }
                handler.Close();

            }
            catch(Exception e)
            {
                Console.WriteLine("ReadCallback Error: " + e.Message);
            }

        }

        /// <summary>
        /// Used for sending data back to the client has connected
        /// </summary>
        /// <param name="ar"></param>
        public void Send(Command command, Client client, Socket handler)
        {
            Console.WriteLine("Now we're waiting at the Send");

            byte[] sendData = GetClientDataToSend(command, client);
            handler.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(SendCallback), handler);
            
        }

        public void SendCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("Now we're waiting at the SendCallback");

                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to the client", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("SendCallback Error: " + e.Message);
            }
        }

        /// <summary>
        /// Will check the connection and if it is disconnected it will remove it from the list
        /// </summary>
        /// <param name="client">The current client in question</param>
        public void CheckConnection(Socket client)
        {
            Console.WriteLine("Check connection");

            if (!client.Connected)
            {
                RemoveClient(client);
            }
        }

        /// <summary>
        /// Removes the client from the list, deleting the player as well
        /// </summary>
        /// <param name="client">The current client in question</param>
        public void RemoveClient(Socket client)
        {

            Client current = clients.Find(c => c.ClientSocket == client);
            Console.WriteLine("Client with id:{0} removed, it is disconnected", current.ID);
            clients.Remove(current);
        }

        public byte[] GetClientDataToSend(Command command, Client client)
        {
            Console.WriteLine("Getting the data to send");
            string data = "";

            switch (command)
            {
                case Command.MoveMe: // (Command, X, Y)
                    data += (int)Command.MoveMe;
                    data += ",";
                    data += client.Ply.CollisionBox.X;
                    data += ",";
                    data += client.Ply.CollisionBox.Y;
                    break;
                case Command.MoveOther: // (Command, X, Y, ID)
                    data += (int)Command.MoveMe;
                    data += ",";
                    data += client.Ply.CollisionBox.X;
                    data += ",";
                    data += client.Ply.CollisionBox.Y;
                    data += ",";
                    data += client.ID;
                    break;
                case Command.MoveYou: // (Command, X, Y, ID)
                    data += (int)Command.MoveMe;
                    data += ",";
                    data += client.Ply.CollisionBox.X;
                    data += ",";
                    data += client.Ply.CollisionBox.Y;
                    data += ",";
                    data += client.ID;
                    break;
                case Command.SendingClientInfo: // (Command, X:Y:Sprite:Color:ID, X:Y:Sprite:Color:ID, X:Y:Sprite:Color:ID...)
                    data += (int)Command.SendingClientInfo;
                    data += ",";
                    for (int i = 0; i < clients.Count; i++)
                    {
                        data += (clients[i].Ply.CollisionBox.X);
                        data += ":";
                        data += (clients[i].Ply.CollisionBox.Y);
                        data += ":";
                        data += (clients[i].Ply.CollisionBox.Width);
                        data += ":";
                        data += (clients[i].Ply.CollisionBox.Height);
                        data += ":";
                        data += (clients[i].ID);
                        data += ":";
                        data += (int)(clients[i].Ply.Sprite);
                        data += ":";
                        data += (clients[i].Ply.Color.R);
                        data += ":";
                        data += (clients[i].Ply.Color.G);
                        data += ":";
                        data += (clients[i].Ply.Color.B);
                        
                        if (i != clients.Count - 1)
                        {
                            data += ",";
                        }
                    }
                    break;
                case Command.AddPlayer: // (Command, X, Y, ID, Sprite, Color)
                    data += (int)Command.AddPlayer;
                    data += (client.Ply.CollisionBox.X);
                    data += ",";
                    data += (client.Ply.CollisionBox.Y);
                    data += ",";
                    data += (client.ID);
                    data += ",";
                    data += (int)(client.Ply.Sprite);
                    data += ",";
                    data += (client.Ply.Color);
                    break;
                case Command.RemovePlayer: // (Command, ID)
                    data += (int)Command.RemovePlayer;
                    data += ",";
                    data += (client.Ply.Color.R);
                    data += ",";
                    data += (client.Ply.Color.G);
                    data += ",";
                    data += (client.Ply.Color.B);
                    break;
                default:
                    Console.WriteLine("We shouldn't be here");
                    break;
            }
            Console.WriteLine("Sending data: {0}", data);

            byte[] byteData = Encoding.ASCII.GetBytes(data);

            return byteData;
        }
    }
}