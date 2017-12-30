using System;

namespace iWorkTech.Orleans.FakeDeviceGateway
{

    public static class ExtensionMethods
    {

        public static double NextDouble(this Random rand, double min, double max)
        {
            return (rand.NextDouble() * (max - min)) + min;
        }

        public static double Cap(this double value, double min, double max)
        {
            return Math.Min(max, Math.Max(min, value));
        }

    }
}