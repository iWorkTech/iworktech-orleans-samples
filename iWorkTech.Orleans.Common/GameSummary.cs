using System;

namespace iWorkTech.Orleans.Common
{
    public struct GameSummary
    {
        public GameState State { get; set; }
        public bool YourMove { get; set; }
        public int NumMoves { get; set; }
        public GameOutcome Outcome { get; set; }
        public int NumPlayers { get; set; }
        public Guid GameId { get; set; }
        public string[] Usernames { get; set; }
        public string Name { get; set; }
        public bool GameStarter { get; set; }
    }
}