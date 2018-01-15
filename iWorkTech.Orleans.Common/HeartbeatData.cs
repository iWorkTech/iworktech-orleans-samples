using System;
using System.Linq;
using System.Text;

namespace iWorkTech.Orleans.Common
{
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
            sb.Append(",Game=").Append((object) Game);
            var playerList = Enumerable.ToArray<Guid>(Status.Players);
            for (var i = 0; i < playerList.Length; i++) sb.AppendFormat(",Player{0}=", i + 1).Append(playerList[i]);
            sb.AppendFormat((string) ",Score={0}", (object) Status.Score);
            return sb.ToString();
        }
    }
}