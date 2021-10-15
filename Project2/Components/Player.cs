using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using ServerLibrary;
using ServerLibrary.UserComponents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2.Components
{
    internal class Player : Sprite
    {
        User userData;
        public SpriteFont font;
        private string direction;
        private Color color = Color.Yellow;
        private bool stop;
        private CircleF circle = new CircleF(Point2.Zero, 0);
        private int circleRadius = 200;
        Vector2 NextPoint = Vector2.Zero;
        public Potato Potato;

        public void SetColor(Color color)
        {
            this.color = color;
            circle = new CircleF(Origin, circleRadius);

        }
        public User GetUserData()
        {
            return userData;
        }

        internal bool IsPotatoOwner()
        {
            return userData.playerStats.isPotatoOwner;
        }

        public Player(RectangleF rectangleF, User userData) : base(rectangleF)
        {
            if (userData != null)
            {
                this.userData = userData;
                rectParams = rectangleF;

                UpdateBounds(rectangleF.X, rectangleF.Y);

            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, color, 10);
            spriteBatch.DrawString(font, $"{userData.GetName()}", new Vector2(Bounds.Position.X - 32, Bounds.Position.Y - 35), Color.Black);

            if (Potato != null)
            {
                Potato.Draw(spriteBatch);
            }
            if (userData.playerStats.isPotatoOwner)
            {
                if (this == GameData.GetCurrentPlayer())
                    spriteBatch.DrawCircle(circle, 15, Color.White);
                spriteBatch.DrawCircle(new CircleF(new Point2(Origin.X, Origin.Y - 16), 10), 15, Color.LightYellow, 20);
            }
        }
        public override void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            CheckIntersect();
            Move(delta, gameTime);
            if (Potato != null)
            {
                Potato.Update(gameTime);
            }
        }

        private void Move(float delta, GameTime gameTime)
        {
            float currentSpeed;
            if (NextPoint != Vector2.Zero && Vector2.Distance(Bounds.Position, NextPoint) > 10)
            {
                Vector2 moveDir1;
                moveDir1 = NextPoint - new Vector2(GetPostiton().X, GetPostiton().Y);
                moveDir1.Normalize();
                Bounds.Position += moveDir1 * (userData.GetSpeed() * 2 * (float)gameTime.ElapsedGameTime.TotalSeconds);
                UpdateBounds(Bounds.Position.X, Bounds.Position.Y);
            }
            else
            if (direction != null && !stop)
            {
                NextPoint = Vector2.Zero;
                if (direction.Length > 1)
                    currentSpeed = userData.GetSpeed() / 1.5f;
                else
                    currentSpeed = userData.GetSpeed();
                if (userData.playerStats.isPotatoOwner)
                    currentSpeed = currentSpeed * 1.2f;
                foreach (char item in direction)
                    switch (item.ToString())
                    {
                        case "W":
                            rectParams.Y -= delta * currentSpeed;
                            break;
                        case "A":
                            rectParams.X -= delta * currentSpeed;
                            break;
                        case "S":
                            rectParams.Y += delta * currentSpeed;
                            break;
                        case "D":
                            rectParams.X += delta * currentSpeed;
                            break;
                    }
            }
            stop = false;
            UpdateBounds(rectParams.X, rectParams.Y);
        }

        private void CheckIntersect()
        {
            foreach (var item in GameData.GetPlayersExcept(this))
            {
                if (item.Bounds.Intersects(Bounds))
                {
                    CheckCollision(item);
                    break;
                }
            }
        }

        void UpdateBounds(float x, float y)
        {
            Bounds.Position = new Point2(x, y);
            rectParams.X = x;
            rectParams.Y = y;
            Origin = new Vector2(rectParams.X + (rectParams.Width / 2),
                                 rectParams.Y + (rectParams.Height / 2));
            if (circle.Center != Point2.Zero)
            {
                circle = new CircleF(Origin, circleRadius);
            }
        }

        internal void SetPosition(PlayerPosition playerPosition)
        {
            if (playerPosition != null)
            {
                if (Vector2.Distance(Bounds.Position, playerPosition.GetVector()) > 10)
                {
                    NextPoint = playerPosition.GetVector();
                }
                else
                {
                    UpdateBounds(playerPosition.x, playerPosition.y);

                }
            }
        }
        internal void Stop()
        {
            direction = "";
        }
        internal void SetMoveDirection(string messageDataJson)
        {
            if (messageDataJson.Trim() != "" && direction != messageDataJson)
            {
                direction = messageDataJson;
            }
        }
        internal Point2 GetPostiton()
        {
            return Bounds.Position;
        }
        internal double GetSpeed()
        {
            return userData.GetSpeed();
        }
        public override void OnCollision(CollisionEventArgs collisionInfo)
        {
            Sprite sprite = (Sprite)collisionInfo.Other;
            CheckCollision(sprite);
        }
        private void CheckCollision(Sprite sprite)
        {
            if (sprite != null && direction != null)
            {
                foreach (char item in direction)
                    switch (item.ToString())
                    {
                        case "W":
                            if (Origin.Y > sprite.Origin.Y)
                            {
                                stop = true;
                            }
                            break;
                        case "A":
                            if (Origin.X > sprite.Origin.X)
                            {
                                stop = true;
                            }
                            break;
                        case "S":
                            if (Origin.Y < sprite.Origin.Y)
                            {
                                stop = true;
                            }
                            break;
                        case "D":
                            if (Origin.X < sprite.Origin.X)
                            {
                                stop = true;
                            }
                            break;
                    }
            }
        }

        internal void DropPotatoTo(string messageDataJson)
        {
            Vector2 vector = (Vector2)Newtonsoft.Json.JsonConvert.DeserializeObject(messageDataJson, typeof(Vector2));
            Potato = new Potato(new RectangleF(Origin, Size2.Empty));
            Potato.SetDirection(vector);
            Potato.SetParent(this);
        }

        internal bool CanDropPotato()
        {
            bool intersects = false;
            foreach (var player in GameData.GetPlayersExcept(this))
            {
                if (circle.Intersects(player.Bounds))
                {
                    intersects = true;
                    break;
                }
            }
            return userData.playerStats.isPotatoOwner && intersects && Potato == null;
        }

        internal string GetOriginString()
        {
            return Origin.ToJson();
        }

        internal Player GetCloserPlayer()
        {
            float distance = float.MaxValue;
            Player player = null;
            foreach (var item in GameData.GetPlayersExcept(this))
            {
                if (Vector2.Distance(Origin, item.Origin) < distance)
                {
                    player = item;
                    distance = Vector2.Distance(Origin, item.Origin);
                }
            }
            return player;
        }
    }
}
