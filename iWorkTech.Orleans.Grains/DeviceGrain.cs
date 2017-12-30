using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Grains
{
    /// <summary>
    ///     Orleans grain implementation class.
    /// </summary>
    [Reentrant]
    public class DeviceGrain : Grain, IDeviceGrain
    {
        public DeviceMessage LastMessage { get; set; }

        public async Task ProcessMessage(DeviceMessage message)
        {

            if (null == LastMessage || LastMessage.Latitude != message.Latitude ||
                LastMessage.Longitude != message.Longitude)
            {
                // only sent a notification if the position has changed
                var notifier = GrainFactory.GetGrain<IPushNotifierGrain>(0);
                var speed = GetSpeed(LastMessage, message);

                // record the last message
                LastMessage = message;

                // forward the message to the notifier grain
                var velocityMessage = new VelocityMessage(message, speed);
                await notifier.SendMessage(velocityMessage);
            }
            else
            {
                // the position has not changed, just record the last message
                LastMessage = message;
            }

            Console.WriteLine($"Device message received: Lat:{0} :: Lon:{1} :: DeviceId:{2}", LastMessage.Latitude, LastMessage.Longitude, message.DeviceId);
        }

        private static double GetSpeed(DeviceMessage message1, DeviceMessage message2)
        {
            // calculate the speed of the device, using the interal state of the grain
            if (message1 == null) return 0;
            if (message2 == null) return 0;

            const double r = 6371 * 1000;
            var x = (message2.Longitude - message1.Longitude) * Math.Cos((message2.Latitude + message1.Latitude) / 2);
            var y = message2.Latitude - message1.Latitude;
            var distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) * r;
            var time = (message2.Timestamp - message1.Timestamp).TotalSeconds;
            if (Math.Abs(time) < 1) return 0;
            return distance / time;
        }
    }
}