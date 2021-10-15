using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary.UserComponents
{
    public class User 
    {
        int defaultSpeed = 500;
        public readonly string username;
        public readonly string address;
        public Guid id { get; set; }
        public PlayerStats playerStats = new();

        public User(string username, string address)
        {
            this.username = username;
            this.address = address;
        }

        public float GetSpeed()
        {
            return playerStats.speed;
        }
        public string GetName()
        {
            return username;
        }
        public void PlayerStatsReset()
        {
            playerStats = new PlayerStats
            {
                speed = defaultSpeed
            };
        }
        public PlayerPosition GetPosition()
        {
            return playerStats.playerPosition;
        }
        public string GetAddress()
        {
            return address;
        }
        public void SetPosition(PlayerPosition playerPosition)
        {
            playerStats.playerPosition = playerPosition;
        }
        public User Clone()
        {
            var user = new User(username, address);
            user.id = id;
            user.playerStats.speed = playerStats.speed;
            user.playerStats.playerPosition = playerStats.playerPosition;
            return user;
        }
    }
}
