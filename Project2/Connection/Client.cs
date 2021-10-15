using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServerLibrary;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using ServerLibrary.MessageComponents;
using ServerLibrary.UserComponents;

namespace Project2.Connection
{
    class GameClient : TcpClient
    {
        public GameClient(string address, int port) : base(address, port) { }

        public void DisconnectAndStop()
        {
            _stop = true;
            DisconnectAsync();
            while (IsConnected)
                Thread.Yield();
        }

        protected override void OnConnected()
        {
            Debug.WriteLine($"Chat TCP client connected a new session with Id {Id}");
        }

        protected override void OnDisconnected()
        {
            Debug.WriteLine($"Chat TCP client disconnected a session with Id {Id}");

            Thread.Sleep(1000);

            if (!_stop)
                ConnectAsync();
        }
        public event EventHandler MessageRecieved;
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            Debug.WriteLine(Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
            MessageRecieved?.Invoke(Encoding.UTF8.GetString(buffer, (int)offset, (int)size), EventArgs.Empty);
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            Console.WriteLine($"Chat TCP client caught an error with code {error}");
        }

        private bool _stop;
    }
    public class Client
    {
        //events
        public event EventHandler MessageRecieved;
        public event EventHandler ClientConnected;

        bool isClientAccepted = false;
        public User User { get; set; }
        Message message;
        private GameClient client;
        public async void Start(string severIp, string name)
        {
            string address = severIp.Split(':')[0];
            int port = int.Parse(severIp.Split(':')[1]);
            client = new GameClient(address, port);
            client.MessageRecieved += Client_MessageRecieved;
            client.ConnectAsync();
            User = new User(name, client.Socket.LocalEndPoint.ToString());
            Send(PacketType.LoginRequest);
            while (!isClientAccepted)
            {
                await Task.Delay(50);
            }
            ClientConnected?.Invoke(null, EventArgs.Empty);
        }

        private void Client_MessageRecieved(object sender, EventArgs e)
        {
            foreach (var item in sender.ToString().Split('|'))
            {
                if (!string.IsNullOrEmpty(item))
                {
                    Console.WriteLine(item);
                    Debug.WriteLine(item);
                    Message responce = (Message)Newtonsoft.Json.JsonConvert.DeserializeObject(item.Replace("|", ""), typeof(Message));
                    CheckLoginAccept(responce);
                    MessageRecieved?.Invoke(responce, EventArgs.Empty);
                }
            }
        }

        private void CheckLoginAccept(Message responce)
        {
            if (responce.packetType == PacketType.LoginAccept)
            {
                User = responce.userData.Clone();
                message.userData = responce.userData.Clone();
                isClientAccepted = true;
            }
        }

        List<string> lastKey = new List<string>();
        internal bool Move(KeyboardState keyboardState, MonoGame.Extended.Point2 position)
        {
            message = new Message();
            message.packetType = PacketType.MovePlayer;
            bool isMoving = false;
            if (keyboardState.GetPressedKeys().Length > 0)
            {
                List<string> corrrentkey = new List<string>();
                foreach (var item in keyboardState.GetPressedKeys())
                {
                    if (item == Keys.D || item == Keys.A || item == Keys.S || item == Keys.W)
                        corrrentkey.Add(item.ToString());
                }
                if (lastKey != corrrentkey)
                {
                    if (corrrentkey.Count > 0)
                    {
                        string str = "";
                        foreach (var item in corrrentkey)
                        {
                            str += item;
                        }
                        message.messageDataJson = str;
                        message.userData = User;
                        client.SendAsync(message.ToJson() + "|");
                        isMoving = true;
                        lastKey = corrrentkey;
                    }

                }
            }
            return isMoving;
        }

        internal async void MoveStop(Point2 position)
        {
            message.packetType = PacketType.StopPlayer;
            message.userData = User;
            message.messageDataJson = position.X + "_" + position.Y;
            client.SendAsync(message.ToJson() + "|");
            await Task.Delay(50);
            client.SendAsync(message.ToJson() + "|");
            client.SendAsync(message.ToJson() + "|");
            lastKey = new List<string>();
        }

        internal void MoveUpdate(Point2 position)
        {
            message.packetType = PacketType.UpdatePlayerPosition;
            message.userData = User;
            message.messageDataJson = position.X + "_" + position.Y;
            client.SendAsync(message.ToJson() + "|");
        }
        internal void Send(PacketType type = PacketType.Unknown, string data = "data")
        {
            message = new Message();
            message.id = 1;
            message.packetType = type;
            message.messageDataJson = data;
            message.userData = User.Clone();
            client.SendAsync(message.ToJson());
        }

        internal void Disconnect()
        {
            client.DisconnectAndStop();
        }
    }
}
