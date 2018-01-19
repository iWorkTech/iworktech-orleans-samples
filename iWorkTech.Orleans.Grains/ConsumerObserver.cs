using System;
using System.Threading.Tasks;
using Orleans.Streams;

namespace iWorkTech.Orleans.Grains
{
    internal class ConsumerObserver<T> : IAsyncObserver<T>
    {
        private readonly StreamingConsumerGrain _hostingGrain;

        internal ConsumerObserver(StreamingConsumerGrain hostingGrain)
        {
            _hostingGrain = hostingGrain;
        }

        public Task OnNextAsync(T item, StreamSequenceToken token = null)
        {
            Console.WriteLine("OnNextAsync(item={0}, token={1})", item, token != null ? token.ToString() : "null");
            _hostingGrain.NumConsumedItems++;
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
    }
}