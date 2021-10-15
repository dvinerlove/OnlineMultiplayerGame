using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collisions;
using Project2.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2.Components
{
    public static class GameData
    {
        static Client client;
        public static Client Client { get => client; set => client = value; }
        static List<IEntity> Entities = new List<IEntity>();

        internal static List<Player> GetPlayersExcept(IEntity player)
        {
            return Entities.Where(x => x != player).OfType<Player>().ToList();
        }

        internal static void AddEntity(IEntity entity)
        {
            Entities.Add(entity);
        }

        internal static Player GetPlayerByName(string name)
        {
            return Entities.OfType<Player>().Where(x => x.GetUserData().GetName() == name).FirstOrDefault();
        }

        internal static void ClearPlayers()
        {
            foreach (Player item in GameData.Entities.OfType<Player>().ToList())
            {
                GameData.Entities.Remove(item);
            }
        }

        internal static Player GetCurrentPlayer()
        {
            var player = Entities.OfType<Player>().Where(x => x.GetUserData().GetName() == Client.User.GetName()).FirstOrDefault();
            return player;
        }

        internal static void CollisionComponentUpdate(CollisionComponent collisionComponent)
        {


            foreach (IEntity entity in GameData.Entities)
            {
                collisionComponent.Insert(entity);
            }
        }

        internal static void RemoveEntity(IEntity entity)
        {
            Entities.Remove(entity);
        }

        internal static List<Player> GetPlayers()
        {
            return Entities.OfType<Player>().ToList();
        }

        internal static void EntitiesUpdate(GameTime updateTime)
        {
            foreach (IEntity entity in Entities.ToArray())
                entity.Update(updateTime);
        }

        internal static void EntitiesDraw(SpriteBatch spriteBatch)
        {
            foreach (Sprite sprite in Entities.ToArray())
            {
                sprite.Draw(spriteBatch);
            }
        }

        internal static void SetPotatoToPlayer(string name)
        {
            foreach (Player sprite in Entities.OfType<Player>().ToArray())
            {
                sprite.Potato = null;
                sprite.GetUserData().playerStats.isPotatoOwner = sprite.GetUserData().GetName() == name;
            }
        }
    }
}
