using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Interfaces;
using Orleans;
using Orleans.Streams;

namespace iWorkTech.Orleans.Grains
{
    public class StreamingConsumerGrain : Grain, IStreamingConsumerGrain
    {
        private IAsyncObservable<int> _consumer;
        private StreamSubscriptionHandle<int> _consumerHandle;
        private IAsyncObserver<int> _consumerObserver;
        protected internal int NumConsumedItems;

        public async Task BecomeConsumer(Guid streamId, string streamNamespace, string providerToUse)
        {
            Console.WriteLine("BecomeConsumer");
            _consumerObserver = new ConsumerObserver<int>(this);
            var streamProvider = GetStreamProvider(providerToUse);
            _consumer = streamProvider.GetStream<int>(streamId, streamNamespace);
            _consumerHandle = await _consumer.SubscribeAsync(_consumerObserver);
        }

        public async Task StopConsuming()
        {
            Console.WriteLine("StopConsuming");
            if (_consumerHandle != null)
            {
                await _consumerHandle.UnsubscribeAsync();
                _consumerHandle = null;
            }
        }

        public Task<int> GetNumberConsumed()
        {
            return Task.FromResult(NumConsumedItems);
        }

        public override Task OnActivateAsync()
        {
            Console.WriteLine("SampleStreaming_ConsumerGrain " + IdentityString);
            Console.WriteLine("OnActivateAsync");
            NumConsumedItems = 0;
            _consumerHandle = null;
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            Console.WriteLine("OnDeactivateAsync");
            return Task.CompletedTask;
        }
    }
}