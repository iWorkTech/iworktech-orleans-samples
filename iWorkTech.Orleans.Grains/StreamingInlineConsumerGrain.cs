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
            Console.WriteLine("StreamingInlineConsumerGrain - BecomeConsumer " + IdentityString);
            var streamProvider = GetStreamProvider(providerToUse);
            _consumer = streamProvider.GetStream<int>(streamId, streamNamespace);
            _consumerHandle = await _consumer.SubscribeAsync(OnNextAsync, OnErrorAsync, OnCompletedAsync);
        }

        public async Task StopConsuming()
        {
            Console.WriteLine("StreamingInlineConsumerGrain - StopConsuming " + IdentityString);
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
            Console.WriteLine("StreamingInlineConsumerGrain - OnActivateAsync " + IdentityString);
            NumConsumedItems = 0;
            _consumerHandle = null;
            return Task.CompletedTask;
        }

        public Task OnNextAsync(int item, StreamSequenceToken token = null)
        {
            Console.WriteLine("StreamingInlineConsumerGrain - OnNextAsync " + IdentityString);
            Console.WriteLine("OnNextAsync({0}{1})", item, token != null ? token.ToString() : "null");
            NumConsumedItems++;
            return Task.CompletedTask;
        }

        public Task OnCompletedAsync()
        {
            Console.WriteLine("StreamingInlineConsumerGrain - OnCompletedAsync " + IdentityString);
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            Console.WriteLine("StreamingInlineConsumerGrain - OnErrorAsync " + IdentityString);
            Console.WriteLine("StreamingInlineConsumerGrain - OnErrorAsync {0}" + ex.Message);
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            Console.WriteLine("StreamingInlineConsumerGrain - OnDeactivateAsync " + IdentityString);
            return Task.CompletedTask;
        }
    }
}