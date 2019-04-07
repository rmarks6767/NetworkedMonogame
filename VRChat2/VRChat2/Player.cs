using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VRChat2
{
    class Player
    {
        /// <summary>
        /// The positioning of the player in the world
        /// </summary>
        Rectangle collisionBox;
        public Rectangle CollisionBox { get => collisionBox; set => collisionBox = value; }

        /// <summary>
        /// The relation to the enum that is in the game1 class
        /// </summary>
        Assets sprite;
        public Assets Sprite { get => sprite; set => sprite = value; }

        /// <summary>
        /// The color of the shape
        /// </summary>
        Color color;
        public Color Color { get { return color; } }

        /// <summary>
        /// Constructor for the player
        /// </summary>
        /// <param name="collisionBox">How to move the player</param>
        /// <param name="sprite">The sprite associated with the player</param>
        public Player(Rectangle collisionBox, Assets sprite, Color color)
        {
            this.collisionBox = collisionBox;
            this.sprite = sprite;
            this.color = color;
        }


    }
}
