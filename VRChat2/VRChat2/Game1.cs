using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace VRChat2
{
    enum Command{
        MoveMe,//Client => Server (newX,newY)
        MoveYou,// Server => Client (newX,newY,id)
        MoveOther, // Server => Client (newX,newY,id)
    }
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        /// <summary>
        /// List of drawn entities
        /// </summary>
        static public List<DrawnEnt> Ents;
        
        /// <summary>
        /// This Client's player
        /// </summary>
        static public DrawnEnt ply;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            string ip = GetIpAddress();


            if (ip == "129.21.50.210")
            {
                ip = "129.21.50.210";
                System.Console.WriteLine("Creating a server");
                Server server = new Server(IPAddress.Parse(ip), 8888);
            }
            else
            {
                ip = "129.21.50.210";
                System.Console.WriteLine("Creating a client");
                Client client = new Client(IPAddress.Parse(ip), 8888);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

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
