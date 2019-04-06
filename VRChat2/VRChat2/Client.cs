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
        /// The threading part for the client
        /// </summary>
        public static ManualResetEvent receiveDone;

        /// <summary>
        /// The threading part for the client
        /// </summary>
        public static ManualResetEvent sendDone;

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
            connectDone = new ManualResetEvent(false);
            receiveDone = new ManualResetEvent(false);
            sendDone = new ManualResetEvent(false);

            client = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //Make an endpoint based on the address and port 
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);

            //Bind that Ip and port to the server 
            client.Bind(endpoint);

            //Start listening for any connections to the server
            client.Listen(port);
            Connect();
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
                client = (Socket)ar.AsyncState;

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

        /// <summary>
        /// Now we can send data back and forth between two endpoints
        /// </summary>
        /// <param name="data"></param>
        private void Send(String data)
        {
            //Turn the string data into bytes
            byte[] byteData = new byte[10];//Encoding. .GetBytes(data);

            byteData[0] = 6;
            //Begin sending the data to the device
            client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
        }

        /// <summary>
        /// Connects to the server socket and sends the data 
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                //retrieve the socket from the stateobject
                client = (Socket)ar.AsyncState;

                //Complete sending the data to the remote device
                int bytesSent = client.EndSend(ar);

                //tell the thread that all bytes have been sent
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        /// <summary>
        /// How we recieve the data from the sever to make the characters move
        /// </summary>
        private void Receive()
        {
            try
            {
                //Create the state object
                StateObject state = new StateObject();
                state.workSocket = client;

                //Get the data from the 
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //Retrieve the state object 
                StateObject state = (StateObject)ar.AsyncState;
                client = state.workSocket;

                //Read the data from the remote device
                int bytesRead = client.EndReceive(ar);
                
                //See what the data is
                if (bytesRead > 0)
                {
                    Console.WriteLine(bytesRead);

                    //Make the bytes a string and add
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    //Keep getting data until there is no more data
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    //all the data has been received put it together
                    if (state.sb.Length > 0)
                    {
                        Console.WriteLine(state.sb.ToString());
                    }
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }
    }
}
