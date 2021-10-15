using ServerLibrary.UserComponents;
using System;
using System.Diagnostics;

namespace ServerLibrary.MessageComponents
{
    public enum PacketType
    {
        LoginRequest,
        LoginAccept,
        SpawnRequest,
        MovePlayer,
        StopPlayer,
        UpdatePlayerPosition,
        AllPositionsUpdate,
        DropPotato,
        Unknown,
        CatchPotato,
    }

 
    public class Message
    {
        public int id;
        public PacketType packetType;
        public string messageDataJson;
        public User userData;

        public Message()
        {
        }

        public Message(string json)
        {
            try
            {
                Message message = (Message)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(Message));
                try
                {
                    userData = message.userData;
                    packetType = message.packetType;
                    this.messageDataJson = message.messageDataJson;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            catch 
            {
                try
                {
                    Message message = (Message)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(Message));

                }
                catch (Exception ex)
                {

                    Debug.WriteLine(ex.Message);
                }
            }
            
        }
    }
   

    
}
