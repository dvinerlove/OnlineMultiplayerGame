using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2.Components
{
    class Sprite : IEntity
    {
        public IShapeF Bounds { get; set; }
        public Vector2 Origin;
        public RectangleF rectParams;

        public Sprite(RectangleF rectangleF)
        {
            Origin = new Vector2(rectangleF.X, rectangleF.Y);
            Bounds = new RectangleF(Origin.X - rectangleF.X / 2, Origin.Y - rectangleF.Y / 2, rectangleF.Width, rectangleF.Height);
            rectParams = rectangleF;
        }

        public object Clone()
        {
            return null;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red,10);
        }

        public virtual void OnCollision(CollisionEventArgs collisionInfo)
        {
            //throw new NotImplementedException();
        }

        public virtual void Update(GameTime gameTime)
        {
            //var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //throw new NotImplementedException();
        }
    }
}
