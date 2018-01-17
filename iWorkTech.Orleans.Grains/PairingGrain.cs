using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Grains
{
    /// <summary>
    ///     Orleans grain implementation class GameGrain
    /// </summary>
    [Reentrant]
    public class PairingGrain : Grain, IPairingGrain
    {
        private MemoryCache _cache;
        private Dictionary<Guid, string> _games;

        public Task AddGame(Guid gameId, string name)
        {
            _games.Add(gameId, name);
            _cache.Set(CacheKeys.Games, _games);
            return Task.CompletedTask;
        }

        public Task RemoveGame(Guid gameId)
        {
            _cache.Remove(gameId.ToString());
            return Task.CompletedTask;
        }

        public Task<PairingSummary[]> GetGames()
        {
            _cache.TryGetValue(CacheKeys.Games, out Dictionary<Guid, string> games);
            return Task.FromResult(games.Select(x => new PairingSummary {GameId = x.Key, Name = x.Value}).ToArray());
        }

        public override Task OnActivateAsync()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();

            provider.GetService<IMemoryCache>();

            _cache = new MemoryCache(new MemoryCacheOptions());
            _games = new Dictionary<Guid, string>();
            _cache.CreateEntry(CacheKeys.Games);
            return base.OnActivateAsync();
        }
    }
}