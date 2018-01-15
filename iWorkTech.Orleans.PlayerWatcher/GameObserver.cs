using System;
using iWorkTech.Orleans.Interfaces;

namespace iWorkTech.Orleans.PlayerWatcher
{
    internal partial class Program
    {
        /// <summary>
        ///     Observer class that implements the observer interface. Need to pass a grain reference to an instance of this class
        ///     to subscribe for updates.
        /// </summary>
        internal class GameObserver : IGameObserver
        {
            // Receive updates
            public void UpdateGameScore(string score)
            {
                Console.WriteLine("New game score: {0}", score);
            }
        }
    }
}