using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace iWorkTech.Orleans.Interfaces
{
    /// <summary>
    ///     Data about the current state of the game
    /// </summary>
    [Serializable]
    public class GameStatus
    {
        public GameStatus()
        {
            Players = new HashSet<Guid>();
        }

        public HashSet<Guid> Players { get; }
        public string Score { get; set; }
    }

    /// <summary>
    ///     Heartbeat data for a game session
    /// </summary>
    [Serializable]
    public class HeartbeatData
    {
        public HeartbeatData()
        {
            Status = new GameStatus();
        }

        public Guid Game { get; set; }
        public GameStatus Status { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Heartbeat:");
            sb.Append(",Game=").Append(Game);
            var playerList = Status.Players.ToArray();
            for (var i = 0; i < playerList.Length; i++) sb.AppendFormat(",Player{0}=", i + 1).Append(playerList[i]);
            sb.AppendFormat(",Score={0}", Status.Score);
            return sb.ToString();
        }
    }


    /// <summary>
    ///     This class encapsulates serialization/deserialization of HeartbeatData.
    ///     It is used only to simulate the real life scenario where data comes in from devices in the binary.
    ///     If an instance of HeartbeatData is passed as an argument to a grain call, no such serializer is necessary
    ///     because Orleans auto-generates efficient serializers for all argument types.
    /// </summary>
    public static class HeartbeatDataDotNetSerializer
    {
        private static readonly BinaryFormatter formatter = new BinaryFormatter();

        public static byte[] Serialize(object o)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, o);
                memoryStream.Flush();
                bytes = memoryStream.ToArray();
            }

            return bytes;
        }

        public static HeartbeatData Deserialize(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                return (HeartbeatData) formatter.Deserialize(memoryStream);
            }
        }
    }
}