using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{

    public interface IPushNotifierGrain : IGrainWithIntegerKey
    {
        Task SendMessage(VelocityMessage message);
    }
}
