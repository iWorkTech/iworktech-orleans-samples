using System.Threading.Tasks;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{
    /// <summary>
    ///     Defines an interface for sending binary updates without knowing the specific game ID.
    ///     Simulates what game consoles do when they send data to the cloud.
    /// </summary>
    public interface IPresenceGrain : IGrainWithIntegerKey
    {
        Task Heartbeat(byte[] data);
    }
}