using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{
    public interface IDeviceNotifierGrain : IGrainWithIntegerKey
    {
        Task SendMessage(VelocityMessage message);
    }
}
