using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using iWorkTech.Orleans.Interfaces;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;

namespace iWorkTech.Orleans.Grains
{
    public class StreamingProducerGrain : Grain, IStreamingProducerGrain
    {
        internal static readonly string RequestContextKey = "RequestContextField";
        internal static readonly string RequestContextValue = "JustAString";
        private int numProducedItems;
        private IAsyncStream<int> producer;
        private IDisposable producerTimer;

        public Task BecomeProducer(Guid streamId, string streamNamespace, string providerToUse)
        {
            Console.WriteLine("BecomeProducer");
            var streamProvider = GetStreamProvider(providerToUse);
            producer = streamProvider.GetStream<int>(streamId, streamNamespace);
            return Task.CompletedTask;
        }

        public Task StartPeriodicProducing()
        {
            Console.WriteLine("StartPeriodicProducing");
            producerTimer = RegisterTimer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
            return Task.CompletedTask;
        }

        public Task StopPeriodicProducing()
        {
            Console.WriteLine("StopPeriodicProducing");
            producerTimer.Dispose();
            producerTimer = null;
            return Task.CompletedTask;
        }

        public Task<int> GetNumberProduced()
        {
            Console.WriteLine("GetNumberProduced {0}", numProducedItems);
            return Task.FromResult(numProducedItems);
        }

        public Task ClearNumberProduced()
        {
            numProducedItems = 0;
            return Task.CompletedTask;
        }

        public Task Produce()
        {
            return Fire();
        }

        public override Task OnActivateAsync()
        {
            Console.WriteLine("SampleStreaming_ProducerGrain " + IdentityString);
            Console.WriteLine("OnActivateAsync");
            numProducedItems = 0;
            return Task.CompletedTask;
        }

        private Task TimerCallback(object state)
        {
            return producerTimer != null ? Fire() : Task.CompletedTask;
        }

        private async Task Fire([CallerMemberName] string caller = null)
        {
            RequestContext.Set(RequestContextKey, RequestContextValue);
            await producer.OnNextAsync(numProducedItems);
            numProducedItems++;
            Console.WriteLine("{0} (item={1})", caller, numProducedItems);
        }

        public override Task OnDeactivateAsync()
        {
            Console.WriteLine("OnDeactivateAsync");
            return Task.CompletedTask;
        }
    }
}