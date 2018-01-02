using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{

    public interface IChatNotifierGrain : IGrainWithIntegerKey
    {
        Task NotifyMessage(ChatMessage message);
    }
}
