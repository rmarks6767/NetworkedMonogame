using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;

namespace VRChat2
{
    class Client
    {
        /// <summary>
        /// The Socket that the client is connected to
        /// </summary>
        Socket client;
        public Socket ClientSocket { get { return client; } set { client = value; } }

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
        /// The current address we have to connect to
        /// </summary>
        IPAddress address;

        /// <summary>
        /// The port at which we do that connecting
        /// </summary>
        int port;

        /// <summary>
        /// The reference to the player that the 
        /// </summary>
        Player ply;
        public Player Ply { get { return ply; } }

        /// <summary>
        /// The Client that will be created off of the given address and the port
        /// </summary>
        /// <param name="address">The address that the server is at</param>
        /// <param name="port">The port the address is at, the parking space</param>
        public Client(IPAddress address, int port)
        {
            this.address = address;
            this.port = port;

            Random rng = new Random();
            client = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Connect();
        }

        /// <summary>
        /// Creating a reference to the actual client that exists out there somewhere in the world
        /// </summary>
        /// <param name="client">The socket that the client is connected to</param>
        /// <param name="clientNum">The id of that client so we can find it</param>
        public Client(Socket socket, int id)
        {
            Random rng = new Random();

            this.client = socket;
            this.id = id;
            ply = new Player(new Rectangle(rng.Next(0, 200), rng.Next(0, 200), 50, 50), Assets.circle, Color.Black);

        }

        /// <summary>
        /// Establish a connection so we may send and recieve data
        /// </summary>
        public void Connect()
        {
            try
            {
                Console.WriteLine("CONNECTING");
                client.Connect(address, port);
            }
            catch(Exception e)
            {
                Console.WriteLine("Couldn't establish a connection: " + e.Message);
            }
        }

        /// <summary>
        /// Send data to the server and wait for a response
        /// </summary>
        public void Send(string data)
        {
            try
            {
                byte[] byteData = Encoding.ASCII.GetBytes(data);
                client.Send(byteData);
                Receive();
            }
            catch(Exception e)
            {
                Console.WriteLine("Data could not be sent, connection refused: Error: " + e.Message);
            }
            
        }

        public void Receive()
        {
            try
            {
                byte[] bytes = new byte[1024];
                int bytesRec = client.Receive(bytes);

                Game1.CurrentCommand = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not recieve data: Error: " + e.Message);
            }
        }




    }
}




/*
       

       

       
       

      

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
       public void Send(String data)
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
       public void Receive()
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

                   Game1.CurrentCommand = state.sb.ToString();
                   Console.WriteLine(state.sb.ToString());
               }
           }
           catch (Exception e)
           {
               Console.WriteLine("Error: " + e);
           }
       }*/
