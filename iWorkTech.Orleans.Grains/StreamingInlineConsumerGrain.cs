using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Interfaces;
using Orleans;
using Orleans.Streams;

namespace iWorkTech.Orleans.Grains
{
    public class StreamingInlineConsumerGrain : Grain, IStreamingInlineConsumerGrain
    {
        private IAsyncObservable<int> _consumer;
        private StreamSubscriptionHandle<int> _consumerHandle;
        internal int NumConsumedItems;

        public async Task BecomeConsumer(Guid streamId, string streamNamespace, string providerToUse)
        {
            Console.WriteLine("BecomeConsumer");
            var streamProvider = GetStreamProvider(providerToUse);
            _consumer = streamProvider.GetStream<int>(streamId, streamNamespace);
            _consumerHandle = await _consumer.SubscribeAsync(OnNextAsync, OnErrorAsync, OnCompletedAsync);
        }

        public async Task StopConsuming()
        {
            Console.WriteLine("StopConsuming");
            if (_consumerHandle != null)
            {
                await _consumerHandle.UnsubscribeAsync();
                //consumerHandle.Dispose();
                _consumerHandle = null;
            }
        }

        public Task<int> GetNumberConsumed()
        {
            return Task.FromResult(NumConsumedItems);
        }

        public override Task OnActivateAsync()
        {
            Console.WriteLine("SampleStreaming_InlineConsumerGrain " + IdentityString);
            Console.WriteLine("OnActivateAsync");
            NumConsumedItems = 0;
            _consumerHandle = null;
            return Task.CompletedTask;
        }

        public Task OnNextAsync(int item, StreamSequenceToken token = null)
        {
            Console.WriteLine("OnNextAsync({0}{1})", item, token != null ? token.ToString() : "null");
            NumConsumedItems++;
            return Task.CompletedTask;
        }

        public Task OnCompletedAsync()
        {
            Console.WriteLine("OnCompletedAsync()");
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            Console.WriteLine("OnErrorAsync({0})", ex);
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            Console.WriteLine("OnDeactivateAsync");
            return Task.CompletedTask;
        }
    }
}