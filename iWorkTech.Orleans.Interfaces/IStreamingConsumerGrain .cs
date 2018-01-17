using System;
using System.Threading.Tasks;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{
    public interface IStreamingConsumerGrain : IGrainWithGuidKey
    {
        Task BecomeConsumer(Guid streamId, string streamNamespace, string providerToUse);
        Task StopConsuming();
        Task<int> GetNumberConsumed();
    }

    public interface IStreamingInlineConsumerGrain : IStreamingConsumerGrain
    {
    }
}