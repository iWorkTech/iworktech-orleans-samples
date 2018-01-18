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
        internal static readonly string REQUEST_CONTEXT_KEY = "RequestContextField";
        internal static readonly string REQUEST_CONTEXT_VALUE = "JustAString";
        private int _numProducedItems;
        private IAsyncStream<int> _producer;
        private IDisposable _producerTimer;

        public Task BecomeProducer(Guid streamId, string streamNamespace, string providerToUse)
        {
            Console.WriteLine("StreamingProducerGrain - BecomeProducer " + IdentityString);
            var streamProvider = GetStreamProvider(providerToUse);
            _producer = streamProvider.GetStream<int>(streamId, streamNamespace);
            return Task.CompletedTask;
        }

        public Task StartPeriodicProducing()
        {
            Console.WriteLine("StreamingProducerGrain - StartPeriodicProducing " + IdentityString);
            _producerTimer = RegisterTimer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
            return Task.CompletedTask;
        }

        public Task StopPeriodicProducing()
        {
            Console.WriteLine("StopPeriodicProducing - GetNumberProduced " + IdentityString);
            _producerTimer.Dispose();
            _producerTimer = null;
            return Task.CompletedTask;
        }

        public Task<int> GetNumberProduced()
        {
            Console.WriteLine("StreamingProducerGrain - GetNumberProduced " + IdentityString);
            Console.WriteLine("GetNumberProduced {0}", _numProducedItems);
            return Task.FromResult(_numProducedItems);
        }

        public Task ClearNumberProduced()
        {
            _numProducedItems = 0;
            Console.WriteLine("StreamingProducerGrain - ClearNumberProduced " + IdentityString);
            return Task.CompletedTask;
        }

        public Task Produce()
        {
            return Fire();
        }

        public override Task OnActivateAsync()
        {
            Console.WriteLine("StreamingProducerGrain - OnActivateAsync " + IdentityString);
            _numProducedItems = 0;
            return Task.CompletedTask;
        }

        private Task TimerCallback(object state)
        {
            Console.WriteLine("StreamingProducerGrain - TimerCallback " + IdentityString);
            return _producerTimer != null ? Fire() : Task.CompletedTask;
        }

        private async Task Fire([CallerMemberName] string caller = null)
        {
            RequestContext.Set(REQUEST_CONTEXT_KEY, REQUEST_CONTEXT_VALUE);
            await _producer.OnNextAsync(_numProducedItems);
            _numProducedItems++;
            Console.WriteLine("StreamingProducerGrain - Fire " + IdentityString);
            Console.WriteLine("{0} (item={1})", caller, _numProducedItems);
        }

        public override Task OnDeactivateAsync()
        {
            Console.WriteLine("StreamingProducerGrain - OnActivateAsync " + IdentityString);
            return Task.CompletedTask;
        }
    }
}