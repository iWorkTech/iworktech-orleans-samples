using System;
using System.Collections.Generic;

namespace iWorkTech.Orleans.Common
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
}