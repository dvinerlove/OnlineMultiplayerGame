using ServerApp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;
using Project2.Components;
using Project2.Connection;
using ServerLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ServerLibrary.MessageComponents;
using ServerLibrary.UserComponents;

namespace Project2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;
        Random Random = new Random();
        TiledMap _tiledMap;
        TiledMapRenderer _tiledMapRenderer;
        private CollisionComponent _collisionComponent;
        int counter = 0;
        private OrthographicCamera _camera;
        private Matrix transformMatrix;
        string responce = "";
        internal Player Player { get; private set; }
        public DisplayMode Display { get; private set; }
        int MapWidth;
        int MapHeight;
        string serverAddress = "";
        string clientUsername;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            clientUsername = "aaaaaaaaaaa" + Random.Next(33333).ToString();
            serverAddress = "192.168.196.110:1111";
        }
        protected override void Initialize()
        {
            _graphics.PreferMultiSampling = true;
            Display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth = Display.Width;
            _graphics.PreferredBackBufferHeight = Display.Height;
            _graphics.IsFullScreen = false;
            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();
            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _camera = new OrthographicCamera(viewportAdapter);
            _graphics.ApplyChanges();
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font1");
            _tiledMap = Content.Load<TiledMap>("samplemap");
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);
            Potato potato = new Potato(new RectangleF(300, 300, 0, 0));
            GameData.AddEntity(potato);
            ConnectToServer();

        }

        public void SetAddress(string address)
        {
            serverAddress = address;
        }
        public void SetUsername(string name)
        {
            clientUsername = name;
        }
        private void ConnectToServer()
        {
            GameData.Client = new Client();
            GameData.Client.MessageRecieved += Client_MessageRecieved;
            GameData.Client.ClientConnected += Client_ClientConnected;
            GameData.Client.Start(serverAddress, clientUsername);
        }
        private void Client_ClientConnected(object sender, System.EventArgs e)
        {
        }

        private void Client_MessageRecieved(object sender, System.EventArgs e)
        {
            Message message = (Message)sender;
            responce = message.packetType.ToString();
            Player player = new Player(new RectangleF(), null);
            if (message.userData != null)
            {
                player = GameData.GetPlayerByName(message.userData.GetName());
            }
            switch (message.packetType)
            {
                case PacketType.AllPositionsUpdate:
                    List<User> playerPosition = (List<User>)message.messageDataJson.ToObject(typeof(List<User>));
                    MapWidth = _tiledMap.WidthInPixels;
                    MapHeight = _tiledMap.HeightInPixels;

                    GameData.ClearPlayers();

                    foreach (User item in playerPosition)
                    {
                        player = GameData.GetPlayerByName(item.GetName());
                        if (player == null)
                        {
                            Player sprite = new Player(new RectangleF(new Vector2(item.GetPosition().x, item.GetPosition().y), new Size2(32, 32)), item)
                            {
                                font = font
                            };
                            GameData.AddEntity(sprite);
                            sprite.SetPosition(item.GetPosition());
                        }
                        else
                        {
                            player.SetPosition(item.GetPosition());
                        }

                        if (GameData.GetCurrentPlayer() != null)
                        {
                            Player = GameData.GetCurrentPlayer();
                            Player.SetColor(Color.DarkRed);
                        }
                    }
                    _collisionComponent = new CollisionComponent(new RectangleF(0, 0, MapWidth, MapHeight));
                    GameData.CollisionComponentUpdate(_collisionComponent);
                    break;
                case PacketType.LoginAccept:
                    GameData.Client.Send(PacketType.SpawnRequest);
                    break;
                case PacketType.StopPlayer:
                    var position = (PlayerPosition)message.messageDataJson.ToObject(typeof(PlayerPosition));
                    Debug.WriteLine(position);
                    if (player != null)
                    {
                        player.SetPosition(position);
                        player.Stop();
                    }
                    break;
                case PacketType.UpdatePlayerPosition:
                    foreach (var item in GameData.GetPlayers())
                    {
                        if (item.GetUserData().GetName() != GameData.GetCurrentPlayer().GetUserData().GetName())
                        {
                            player.SetPosition((PlayerPosition)message.messageDataJson.ToObject(typeof(PlayerPosition)));
                        }
                    }
                    break;
                case PacketType.MovePlayer:
                    if (player != null && message.messageDataJson != null)
                    {
                        player.SetMoveDirection(message.messageDataJson);
                    }
                    break;
                case PacketType.DropPotato:
                    if (player != null && message.messageDataJson != null)
                    {
                        player.DropPotatoTo(message.messageDataJson);
                    }
                    break;
                case PacketType.CatchPotato:
                    GameData.SetPotatoToPlayer(message.messageDataJson);
                    break;
                default:
                    break;
            }
        }
        bool isPlayerStoped = false;

        static bool IsAllKeysUp(KeyboardState keyboardState)
        {
            return (keyboardState.IsKeyUp(Keys.D) &&
                    keyboardState.IsKeyUp(Keys.A) &&
                    keyboardState.IsKeyUp(Keys.S) &&
                    keyboardState.IsKeyUp(Keys.W));
        }
        protected override void Update(GameTime gameTime)
        {
            FixedTimeUpdate(gameTime);
            base.Update(gameTime);
        }

        private void FixedTimeUpdate(GameTime updateTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
                GameData.Client.Disconnect();
            }

            _tiledMapRenderer.Update(updateTime);
            GameData.EntitiesUpdate(updateTime);



            if (Player != null)
            {
                MoveCamera(updateTime);
                PlayerInput();
                MovePlayer();

            }
            counter++;
            if (counter > 200)
            {
                GameData.Client.MoveUpdate(Player.GetPostiton());
                counter = 0;
            }

        }
        KeyboardState currentKeyboardState = Keyboard.GetState();
        KeyboardState previousKeyboardState = Keyboard.GetState();

        private void PlayerInput()
        {
            currentKeyboardState = Keyboard.GetState();
            if (Player.CanDropPotato())
                if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    GameData.Client.Send(PacketType.DropPotato, Player.GetCloserPlayer().GetOriginString());
                }
            previousKeyboardState = Keyboard.GetState();

        }
        private void MovePlayer()
        {

            bool isPlayerMoved = GameData.Client.Move(Keyboard.GetState(), Player.GetPostiton());

            if (!isPlayerMoved && IsAllKeysUp(Keyboard.GetState()) == true &&
               IsAllKeysUp(Keyboard.GetState()) != isPlayerStoped)
            {
                GameData.Client.MoveStop(Player.GetPostiton());
            }
            isPlayerStoped = IsAllKeysUp(Keyboard.GetState());
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            transformMatrix = _camera.GetViewMatrix();
            transformMatrix *= Matrix.CreateTranslation(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2, 0);

            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
            _tiledMapRenderer.Draw(transformMatrix);
            _spriteBatch.End();

            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.AnisotropicWrap, transformMatrix: transformMatrix);
            _spriteBatch.DrawString(font, $" {responce}", new Vector2(20, 20), Color.Black);
            GameData.EntitiesDraw(_spriteBatch);

            _spriteBatch.End();

            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.AnisotropicWrap);
            List<Player> list = GameData.GetPlayers();
            for (int i = 0; i < list.Count; i++)
            {
                Player item = list[i];
                _spriteBatch.DrawString(font, $" {item.GetUserData().GetName()}", new Vector2(40, 60 + 20 * i), Color.White);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
        private void MoveCamera(GameTime gameTime)
        {
            float speed = (float)(Player.GetSpeed() * 0.8f);
            Vector2 movementDirection = new(Player.GetPostiton().X + 16,
                Player.GetPostiton().Y + 16);
            Vector2 moveDir1;
            moveDir1 = movementDirection - _camera.Position;
            if (Vector2.Distance(movementDirection, _camera.Position) > 10)
            {
                moveDir1.Normalize();
                _camera.Position += moveDir1 * ((speed) * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            _camera.Zoom = 1f;
        }
    }
}
