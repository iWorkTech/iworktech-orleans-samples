namespace iWorkTech.Orleans.Common
{
    public class VelocityMessage : DeviceMessage
    {
        public VelocityMessage()
        {
        }

        public VelocityMessage(DeviceMessage deviceMessage, double velocity)
        {
            Latitude = deviceMessage.Latitude;
            Longitude = deviceMessage.Longitude;
            MessageId = deviceMessage.MessageId;
            DeviceId = deviceMessage.DeviceId;
            Timestamp = deviceMessage.Timestamp;
            Velocity = velocity;
        }

        public double Velocity { get; set; }
    }

    public class VelocityBatch
    {
        public VelocityMessage[] Messages;
    }
}
