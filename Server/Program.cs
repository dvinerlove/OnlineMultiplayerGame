using NetCoreServer;
using ServerLibrary;
using ServerLibrary.MessageComponents;
using ServerLibrary.UserComponents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{

    public static class ServerData
    {
        static List<User> Users = new List<User>();

        internal static void Remove(Guid id)
        {
            User user = Users.Where(x => x.id == id).FirstOrDefault();
            if (user != null)
            {
                Users.Remove(user);
                UpdateUsers();
            }
        }

        internal static void AddUser(User userData)
        {
            if (Users.Where(x => x.GetAddress() == userData.GetAddress()).FirstOrDefault() == null)
            {
                Users.Add(userData);
            }
            UpdateUsers();
        }
        static void UpdateUsers()
        {
            Console.Clear();
            Console.WriteLine("----------------------------------");
            foreach (var item in Users)
            {
                Console.WriteLine($"{item.GetName()} ({item.GetAddress()}) ");
                Console.WriteLine("----------------------------------");
            }
        }
        static bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        internal static void Remove(Socket socket)
        {
            if (!SocketConnected(socket))
            {
                Users.Remove(Users.Where(x => x.GetAddress() == socket.RemoteEndPoint.ToString()).FirstOrDefault());
            }
        }

        internal static void SetPosition(PlayerPosition playerPosition, string username)
        {
            Users.Where(x => x.GetName() == username).FirstOrDefault().SetPosition(playerPosition);
        }
        internal static PlayerPosition GetPosition(string username)
        {
            var user = Users.Where(x => x.GetName() == username).FirstOrDefault();
            if (user != null)
            {
                return user.GetPosition();
            }
            return null;
        }

        internal static string GetAllPositions()
        {
            return Users.ToList().ToJson();
        }

        internal static int GetUsersCount()
        {
            return Users.Count;
        }
    }
    class GameSession : TcpSession
    {

        public GameSession(TcpServer server) : base(server) { }

        protected override void OnConnected()
        {
        }

        protected override void OnDisconnected()
        {
            ServerData.Remove(Id);
            Console.WriteLine($"___ session with Id {Id} disconnected!");
            UpdatePositions();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            var a = message.Split("|");
            foreach (string item in a)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string str = item.Replace("|", "");
                    Message messageData = new Message(str);
                    Task.Factory.StartNew(() => { HandleRequest(messageData); });
                }
            }
        }

        private void HandleRequest(Message messageData)
        {
            if (messageData.userData != null)
            {
                switch (messageData.packetType)
                {
                    case PacketType.LoginRequest:
                        HandleLogin(messageData);
                        break;
                    case PacketType.SpawnRequest:
                        HandleSpawn(messageData);
                        Console.WriteLine(messageData.userData.GetName() + " " + messageData.packetType);
                        break;
                    case PacketType.MovePlayer:
                        HandleMove(messageData);
                        break;
                    case PacketType.StopPlayer:
                        UpdatePlayerPosition(messageData.packetType, messageData);
                        break;
                    case PacketType.UpdatePlayerPosition:
                        UpdatePlayerPosition(messageData.packetType, messageData);
                        break;
                    case PacketType.DropPotato:
                        Console.WriteLine(messageData.userData.GetName() + " " + messageData.packetType + " " + messageData.messageDataJson);
                        Server.Multicast(messageData.ToJson() + "|");
                        break;
                    case PacketType.CatchPotato:
                        Console.WriteLine(messageData.userData.GetName() + " " + messageData.packetType + " " + messageData.messageDataJson);
                        Server.Multicast(messageData.ToJson() + "|");
                        break;
                    default:
                        break;
                }
            }
        }

        static PlayerPosition SetPosition(Message messageData)
        {
            try
            {
                string[] position = messageData.messageDataJson.Split('_');
                PlayerPosition positionUser = ServerData.GetPosition(messageData.userData.GetName());
                positionUser.x = float.Parse(position[0]);
                positionUser.y = float.Parse(position[1]);
                ServerData.SetPosition(positionUser, messageData.userData.GetName());
                return positionUser;
            }
            catch
            {

                return null;
            }

        }
        void UpdatePlayerPosition(PacketType type, Message messageData)
        {
            messageData.packetType = type;
            messageData.messageDataJson = SetPosition(messageData).ToJson();
            Server.Multicast(messageData.ToJson() + "|");
        }
        private void HandleMove(Message messageData)
        {
            SetPosition(messageData);
            var position = ServerData.GetPosition(messageData.userData.GetName());
            if (position != null)
            {
                messageData.packetType = PacketType.MovePlayer;
                Server.Multicast(messageData.ToJson() + "|");
            }
        }

        Random Random = new Random();
        private void HandleSpawn(Message messageData)
        {
            PlayerPosition playerPosition = new PlayerPosition();
            playerPosition.x = Random.Next(512);
            playerPosition.y = Random.Next(512);
            ServerData.SetPosition(playerPosition, messageData.userData.GetName());
            UpdatePositions();
        }
        void UpdatePositions()
        {
            Message message = new();
            message.packetType = PacketType.AllPositionsUpdate;
            message.messageDataJson = ServerData.GetAllPositions();
            Console.WriteLine(ServerData.GetAllPositions());
            Server.Multicast(message.ToJson() + "|");
        }
        private void HandleLogin(Message messageData)
        {
            messageData.packetType = PacketType.LoginAccept;
            messageData.userData.id = Id;
            messageData.userData.PlayerStatsReset();
            if (ServerData.GetUsersCount() == 0)
            {
                messageData.userData.playerStats.isPotatoOwner = true;
                messageData.userData.playerStats.isHost = true;
            }
            ServerData.AddUser(messageData.userData);
            Console.WriteLine(messageData.userData.GetName());
            Console.WriteLine(messageData.userData.GetAddress());
            SendAsync(messageData.ToJson() + "|");
        }



        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
    }

    class GameServer : TcpServer
    {
        public GameServer(IPAddress address, int port) : base(address, port) { }

        protected override TcpSession CreateSession() { return new GameSession(this); }

        public void CheckLocalAddress()
        {
            foreach (var item in Sessions)
            {
                Console.WriteLine(item.Key + " " + item.Value);
            }
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            Console.WriteLine($"Chat TCP server caught an error with code {error}");
        }
    }

    public class Program
    {

        public static void Main(string[] args)
        {
            // TCP server port
            int port = 0;
            if (args.Length > 0)
                port = int.Parse(args[0]);
            if (port != 0)
            {
                Console.WriteLine($"TCP server port: {port}");

                Console.WriteLine();

                // Create a new TCP chat server
                //var server = new ChatServer(IPAddress.Parse("192.168.196.110"), port);
                var server = new GameServer(IPAddress.Any, port);
                // Start the server
                Console.Write("Server starting...");
                server.Start();
                Console.WriteLine("Done!");
                Console.WriteLine(server.Endpoint);
                Console.WriteLine(server.Endpoint.ToString());
                server.CheckLocalAddress();
                Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

                // Perform text input
                while (true)
                {
                    string line = Console.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        break;

                    // Restart the server
                    if (line == "!")
                    {
                        Console.Write("Server restarting...");
                        server.Restart();
                        Console.WriteLine("Done!");
                        continue;
                    }

                    // Multicast admin message to all sessions
                    line = "(admin) " + line;
                    server.Multicast(line);
                }

                // Stop the server
                Console.Write("Server stopping...");
                server.Stop();
                Console.WriteLine("Done!");
            }
            else
            {
                throw new Exception("error");
            }

        }
    }
}