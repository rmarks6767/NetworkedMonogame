using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;


struct MoveYou
{
    public int x;
    public int y;
    public int id;

    public MoveYou(int x, int y, int id)
    {
        this.x = x;
        this.y = y;
        this.id = id;
    }


}

struct MoveOther
{
    public int x;
    public int y;
    public int id;

    public MoveOther(int x, int y, int id)
    {
        this.x = x;
        this.y = y;
        this.id = id;
    }
}

struct SendingClientInfo
{
    
    public int x;
    public int y;
    public int w;
    public int h;
    public int id;
    public int spriteID;///CHANGE THIS TO ENUM AFTER WE MERGE
    public Color color;

    public SendingClientInfo(int x,int y, int w, int h, int id, int spriteID,Color color)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        this.id = id;
        this.spriteID = spriteID;
        this.color = color;
    }
}

namespace VRChat2
{
    public enum Command
    {
        MoveMe,//Client => Server (newX,newY)
        MoveYou,// Server => Client (newX,newY,id)
        MoveOther, // Server => Client (newX,newY,id)
        SendingClientInfo,
        AddPlayer,
        RemovePlayer,
    }
    
    public enum Assets
    {
        circle,
        triangle,
        square,
    }
    
    public enum GameState
    {
        waiting,
        playing,
    }

    
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameState gameState;

        static public bool SERVER;


        /// <summary>
        /// ClientCurrentCommand Being sent from the server
        /// </summary>
        static public string CurrentCommand = null;

        /// <summary>
        /// The relational database of textures
        /// </summary>
        static public Dictionary<Assets, Texture2D> assestsDict;
        
        /// <summary>
        /// List of drawn entities
        /// </summary>
        static public List<DrawnEnt> Ents;
        
        /// <summary>
        /// This Client's player
        /// </summary>
        static public DrawnEnt ply;

        /// <summary>
        /// ClientSide Client object 
        /// </summary>
        Client MyClient;

        public Game1()
        {
            gameState = GameState.waiting;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            string ip = GetIpAddress();


            if (ip == "129.21.50.210")
            {
                ip = "129.21.50.210";
                SERVER = true;
                System.Console.WriteLine("Creating a server");
                Server server = new Server(IPAddress.Parse(ip), 8888);
            }
            else
            {
                ip = "129.21.50.210";
                SERVER = false;
                System.Console.WriteLine("Creating a client");
                MyClient = new Client(IPAddress.Parse(ip), 8888);
            }

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Ents = new List<DrawnEnt>();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            assestsDict = new Dictionary<Assets, Texture2D>
            {
                { Assets.circle, Content.Load<Texture2D>("WhiteCircle") },
                { Assets.square, Content.Load<Texture2D>("WhiteSquare") },
                { Assets.triangle, Content.Load<Texture2D>("WhiteTriangle") }
            };


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();



            if (SERVER)
            {
                System.Console.WriteLine("You're the server");
                //IDK WHAT YOU DO HERE RIVER...
            }
            else
            {
                //PARSING INCOMMING DATA IF ANY

                MyClient.Receive();
                List<SendingClientInfo> clientInfo = null;
                MoveOther moveOther;
                MoveYou moveYou;

                //Parse the data sent by the server
                if (CurrentCommand != "" && CurrentCommand != null)
                {
                    System.Console.WriteLine(CurrentCommand);
                    string[] tempCmdArgs = CurrentCommand.Split(',');
                    int cmdHead = int.Parse(tempCmdArgs[0]);

                    switch ((Command)cmdHead)
                    {
                        case Command.MoveOther:
                            moveOther = new MoveOther(
                                int.Parse(tempCmdArgs[1]),
                                int.Parse(tempCmdArgs[2]),
                                int.Parse(tempCmdArgs[3])
                                );

                            break;

                        case Command.MoveYou:
                            moveYou = new MoveYou(
                                int.Parse(tempCmdArgs[1]), 
                                int.Parse(tempCmdArgs[2]), 
                                int.Parse(tempCmdArgs[3])
                                );

                            break;

                        case Command.SendingClientInfo:
                            clientInfo = new List<SendingClientInfo>();
                            for (int i = 1; i < tempCmdArgs.Length; i++)
                            {
                                string[] ply = tempCmdArgs[i].Split(':');
                                SendingClientInfo plyStruct = new SendingClientInfo(
                                    int.Parse(ply[0]),
                                    int.Parse(ply[1]),
                                    int.Parse(ply[2]),
                                    int.Parse(ply[3]),
                                    int.Parse(ply[4]),
                                    int.Parse(ply[5]),
                                    new Color(int.Parse(ply[6]),int.Parse(ply[7]),int.Parse(ply[8]))
                                    );
                                clientInfo.Add(plyStruct);
                            }
                            break;

                        case Command.MoveMe:
                            System.Console.WriteLine("Why did the server send me this?");
                            break;
                    }
                    
                }
                
                switch (gameState)
                {
                    case GameState.waiting:
                        
                        
                        //If we got client info from the server, lets populate drawnEnts array
                        if (clientInfo != null)
                        {
                            System.Console.WriteLine("Got data");
                            gameState = GameState.playing;
                            foreach(SendingClientInfo s in clientInfo)
                            {
                                Ents.Add(
                                    new DrawnEnt(
                                        new Rectangle(s.x, s.y, s.w, s.h),
                                        assestsDict[(Assets)s.spriteID],
                                        s.color,
                                        s.id
                                        ));
                                        
                            }
                        }

                        break;
                    case GameState.playing:
                        
                        //MyClient.Send();
                        break;
                }
                //Clear the current command.
                //CurrentCommand = null;
            }

            //client.

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            spriteBatch.Begin(SpriteSortMode.BackToFront, null);
            foreach(DrawnEnt ent in Ents)
            {
                ent.Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }


        protected string GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach(var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "oh fug";
        }
    }
}
