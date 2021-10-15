using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using ServerLibrary.MessageComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2.Components
{
    class Potato : Sprite
    {
        public Potato(RectangleF rectangleF) : base(rectangleF)
        {

        }
        Player parent = null;
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Direction != Vector2.Zero)
            {
                spriteBatch.DrawCircle(new CircleF(rectParams.Position, 10), 20, Color.Black, 30, 1);
            }
        }
        double startTime = -1;
        public override void Update(GameTime gameTime)
        {
            if (Direction != Vector2.Zero)
            {
                if (startTime == -1)
                {
                    startTime = gameTime.TotalGameTime.TotalSeconds;
                }
                double currentTime = gameTime.TotalGameTime.TotalSeconds;
                if (currentTime - startTime > 0.45)
                {
                    Vector2 moveDir1;
                    moveDir1 = parent.Origin - new Vector2(rectParams.X, rectParams.Y);
                    moveDir1.Normalize();
                    rectParams.Position += moveDir1 * (speed * 1.5f * (float)gameTime.ElapsedGameTime.TotalSeconds);
                    if (Vector2.Distance(parent.Origin, rectParams.Position) < 10)
                    {
                        parent.Potato = null;
                        GameData.RemoveEntity(this);
                    }
                }
                else
                {
                    rectParams.Position += Direction * (speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
                if (GameData.GetCurrentPlayer().IsPotatoOwner())
                    foreach (var item in GameData.GetPlayersExcept(parent))
                    {
                        if (new CircleF(rectParams.Position, 30).Intersects(item.Bounds))
                        {
                            GameData.Client.Send(PacketType.CatchPotato, item.GetUserData().GetName());
                        }
                    }


            }
        }
        Vector2 Direction = Vector2.Zero;
        private int speed = 800;

        internal void SetDirection(Vector2 vector)
        {
            Vector2 moveDir1;
            moveDir1 = vector - new Vector2(Origin.X, Origin.Y);
            moveDir1.Normalize();
            Direction = moveDir1;
            startTime = -1;
        }
        internal void SetParent(Player player)
        {
            parent = player;
        }
    }
}
