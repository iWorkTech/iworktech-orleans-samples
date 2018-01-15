using System;

namespace iWorkTech.Orleans.Common
{
    public struct GameMove
    {
        public Guid PlayerId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}