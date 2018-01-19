using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Interfaces
{
    public interface IStreamingProducerGrain : IGrainWithGuidKey
    {
        Task BecomeProducer(Guid streamId, string streamNamespace, string providerToUse);
        Task StartPeriodicProducing();
        Task StopPeriodicProducing();
        Task<int> GetNumberProduced();
        Task ClearNumberProduced();
        Task Produce();
    }
}