using iWorkTech.Orleans.Common;
using System.Threading.Tasks;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{
    public interface IDeviceGrain : IGrainWithIntegerKey
    {
        Task ProcessMessage(DeviceMessage message);
    }
}