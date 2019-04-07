using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

struct RemovePlayer
{
    public int id;

    public RemovePlayer(int id)
    {
        this.id = id;
    }


}

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
        Null,//Nothing is being sent
        MoveMe,//Client => Server (newX,newY)
        MoveYou,// Server => Client (newX,newY,id)
        MoveOther, // Server => Client (newX,newY,id)
        SendingClientInfo,
        RemovePlayer,
        AddPlayer,
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
        const int UniversalSpeed = 5;
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
            else//CLIENT
            {
                //PARSING INCOMMING DATA IF ANY

                MyClient.Receive();
                List<SendingClientInfo> clientInfo = null;
                MoveOther moveOther = new MoveOther();
                MoveYou moveYou = new MoveYou();
                RemovePlayer removePlayer = new RemovePlayer();
                Command cmdHead = Command.Null;

                string command = CurrentCommand;

                //PARSE DATA SENT BY THE SERVER FOR EASY READING LATER
                if (command != "" && command != null)
                {
                    System.Console.WriteLine(command);
                    string[] tempCmdArgs = command.Split(',');
                    cmdHead = (Command)int.Parse(tempCmdArgs[0]);

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
                                string[] arg = tempCmdArgs[i].Split(':');
                                SendingClientInfo plyStruct = new SendingClientInfo(
                                    int.Parse(arg[0]),
                                    int.Parse(arg[1]),
                                    int.Parse(arg[2]),
                                    int.Parse(arg[3]),
                                    int.Parse(arg[4]),
                                    int.Parse(arg[5]),
                                    new Color(int.Parse(arg[6]),int.Parse(arg[7]),int.Parse(arg[8]))
                                    );
                                clientInfo.Add(plyStruct);
                            }
                            break;

                        case Command.MoveMe:
                            System.Console.WriteLine("Why did the server send me this?");
                            break;
                        case Command.RemovePlayer:
                            removePlayer = new RemovePlayer(int.Parse(tempCmdArgs[1]));
                            break;
                    }
                    Client.receiveDone = new System.Threading.ManualResetEvent(false);
                }
                
                switch (gameState)
                {
                    case GameState.waiting:


                        //If we got client info from the server, lets populate drawnEnts array

                        if (cmdHead == Command.SendingClientInfo)
                        {
                            System.Console.WriteLine("Got initial client info");
                            gameState = GameState.playing;
                            GetClientInfo(clientInfo);
                        }

                        break;
                    case GameState.playing:
                        switch(cmdHead){
                            case Command.Null:
                                //NOTHING SHOULD EVER HAPPEN WHEN WE'RE NULL
                                break;
                            case Command.SendingClientInfo:
                                System.Console.WriteLine("Got client info!");
                                
                                ///I don't know how we could get here with clientInfo being null but you
                                ///can never be too safe
                                if (clientInfo != null)
                                {
                                    GetClientInfo(clientInfo);
                                }
                                break;

                            case Command.MoveMe:
                                System.Console.WriteLine("We should not have gotten this. Wtf is the server doing?");
                                break;

                            case Command.MoveYou:
                                System.Console.WriteLine("Got a move you command");
                               ply.Position = new Rectangle(
                                    moveYou.x,
                                    moveYou.y,
                                    ply.Position.Width,
                                    ply.Position.Height
                                    );

                                break;
                            case Command.MoveOther:
                                System.Console.WriteLine("Got a move other command");
                                for(int i = 0; i < Ents.Count; i++)
                                {
                                    //If we found an ent with the id of the ent we're supposed to move, move it
                                    if (Ents[i].Id == moveOther.id)
                                    {
                                        Ents[i].Position = new Rectangle(
                                            moveOther.x,
                                            moveOther.y,
                                            Ents[i].Position.Width,
                                            Ents[i].Position.Height);
                                        break;
                                    }
                                }
                                break;
                            case Command.RemovePlayer:
                                System.Console.WriteLine("Got a move player command");
                                for(int i = 0; i < Ents.Count; i++)
                                {
                                    if (Ents[i].Id == removePlayer.id)
                                    {
                                        Ents.RemoveAt(i);
                                        break;
                                    }
                                }
                                break;


                        }

                        //CHECK FOR KEY PRESSES NOW
                        if (Keyboard.GetState().IsKeyDown(Keys.A))
                        {
                            MyClient.Send(string.Format(
                                "{0},{1},{2},{3},{4}",
                                Command.MoveMe, ply.Position.X - UniversalSpeed,
                                ply.Position.Y,ply.Position.Width,
                                ply.Position.Height));
                        }
                        else if (Keyboard.GetState().IsKeyDown(Keys.D))
                        {
                            MyClient.Send(string.Format(
                                "{0},{1},{2},{3},{4}",
                                Command.MoveMe,
                                ply.Position.X + UniversalSpeed,
                                ply.Position.Y,
                                ply.Position.Width,
                                ply.Position.Height));
                        }
                        else if (Keyboard.GetState().IsKeyDown(Keys.W))
                        {
                            MyClient.Send(string.Format(
                                "{0},{1},{2},{3},{4}",
                                Command.MoveMe,
                                ply.Position.X,
                                ply.Position.Y - UniversalSpeed,
                                ply.Position.Width,
                                ply.Position.Height));
                        }
                        else if (Keyboard.GetState().IsKeyDown(Keys.S))
                        {
                            MyClient.Send(string.Format(
                                "{0},{1},{2},{3},{4}",
                                Command.MoveMe,
                                ply.Position.X,
                                ply.Position.Y + UniversalSpeed,
                                ply.Position.Width,
                                ply.Position.Height));
                        }
                        break;
                }
                //Clear the current command.
                command = null;
                CurrentCommand = command;
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

        void GetClientInfo(List<SendingClientInfo> clientInfo)
        {
            System.Console.WriteLine("Got data");
            gameState = GameState.playing;
            foreach (SendingClientInfo s in clientInfo)
            {
                Ents.Add(
                    new DrawnEnt(
                        new Rectangle(s.x, s.y, s.w, s.h),
                        assestsDict[(Assets)s.spriteID],
                        s.color,
                        s.id
                        ));
                if(s.id == MyClient.ID)
                {
                    ply = s;
                }

            }
        }
    }
}
