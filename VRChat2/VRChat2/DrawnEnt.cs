using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

enum EntType
{
    player,
}

namespace VRChat2
{
    /// <summary>
    /// A client entity this drawn for all clients
    /// </summary>
    public class DrawnEnt
    {
        Rectangle position;
        Texture2D sprite;
        Color tint;
        int id;
        float depth;
        public Rectangle Position { get => position; set => position = value; }
        public Texture2D Sprite { get => sprite; set => sprite = value; }
        public Color Tint { get => tint; set => tint = value; }
        public float Depth { get => depth; set => depth = value; }
        public int Id { get => id; set => id = value; }

        public DrawnEnt(Rectangle position, Texture2D sprite,Color tint)
        {
            this.position = position;
            this.sprite = sprite;
            this.tint = tint;
            depth = 0;
        }

        public void Draw(SpriteBatch sb)
        {
            Rectangle onScreenPos = new Rectangle(
                this.position.X - Camera.position.X,
                this.position.Y - Camera.position.Y, 
                position.Width, position.Height);
            sb.Draw(sprite, onScreenPos,new Rectangle(0,0,sprite.Width,sprite.Height), tint, 0, new Vector2(0,0),SpriteEffects.None,depth);
        }
        
    }
}
