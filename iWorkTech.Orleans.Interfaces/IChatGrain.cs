using iWorkTech.Orleans.Common;
using System.Threading.Tasks;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{
    public interface IChatGrain : IGrainWithIntegerKey
    {
        Task ProcessMessage(ChatMessage message);
    }
}