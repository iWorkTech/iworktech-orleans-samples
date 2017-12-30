using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Grains
{
    [Reentrant]
    [StatelessWorker]
    public class PushNotifierGrain : Grain, IPushNotifierGrain
    {
        private readonly Dictionary<string, Tuple<HubConnection, IHubClients>> _hubs =
            new Dictionary<string, Tuple<HubConnection, IHubClients>>();

        private readonly List<VelocityMessage> _messageQueue = new List<VelocityMessage>();

        public Task SendMessage(VelocityMessage message)
        {
            // add a message to the send queue
            _messageQueue.Add(message);
            if (_messageQueue.Count > 25)
                Flush();
            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            // set up a timer to regularly flush the message queue
            RegisterTimer(FlushQueue, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

            // not in azure, the SignalR hub is running locally
            //await AddHub("http://localhost:60361/");
            await AddHub("https://localhost:44358/location");

            await base.OnActivateAsync();
        }

        private Task FlushQueue(object _)
        {
            Flush();
            return Task.CompletedTask;
        }

        private async Task AddHub(string address)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(address)
                .WithConsoleLogger()
                .Build();

            connection.On<string>("Message", data => { Console.WriteLine($"Received: {data}"); });

            await connection.StartAsync();
        }

        private void Flush()
        {
            if (_messageQueue.Count == 0) return;

            // send all messages to all SignalR hubs
            var messagesToSend = _messageQueue.ToArray();
            _messageQueue.Clear();

            var promises = new List<Task>();
            foreach (var hub in _hubs.Values)
                try
                {
                    //if (hub.Item1.StartAsync() == ConnectionState.Connecting)
                    //    hub.Item2.Invoke("LocationUpdates", new VelocityBatch {Messages = messagesToSend});
                    //else
                    //    hub.Item1.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
        }

        //private async Task RefreshHubs(object _)
        //{
        //    var addresses = new List<string>();
        //    var tasks = new List<Task>();

        //    // discover the current infrastructure
        //    foreach (var instance in RoleEnvironment.Roles["GPSTracker.Web"].Instances)
        //    {
        //        var endpoint = instance.InstanceEndpoints["InternalSignalR"];
        //        addresses.Add(string.Format("http://{0}", endpoint.IPEndpoint.ToString()));
        //    }
        //    var newHubs = addresses.Where(x => !hubs.Keys.Contains(x)).ToArray();
        //    var deadHubs = hubs.Keys.Where(x => !addresses.Contains(x)).ToArray();

        //    // remove dead hubs
        //    foreach (var hub in deadHubs)
        //    {
        //        hubs.Remove(hub);
        //    }

        //    // add new hubs
        //    foreach (var hub in newHubs)
        //    {
        //        tasks.Add(AddHub(hub));
        //    }

        //    await Task.WhenAll(tasks);
        //}
    }
}