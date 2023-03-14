using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;
using MongoDB.Driver;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting.Storage
{
    internal static class CounterCollection
    {
        private static IMongoCollection<Counter> _collection;

        private static async ValueTask<IMongoCollection<Counter>> Get(IMongoDatabase db)
        {
            if (_collection is null)
            {
                _collection = db.GetCollection<Counter>("counter");
                await _collection.Indexes.CreateManyAsync(new[]
                    {
                        new CreateIndexModel<Counter>(
                            Builders<Counter>.IndexKeys
                                .Ascending(x => x.Name),
                            new CreateIndexOptions { Unique = true }
                        )
                    }
                );
            }

            return _collection;
        }

        public static async Task<IEnumerable<int>> GetNextCounterValues(this IMongoDatabase db, IClientSessionHandle session, string counterName, int N)
        {
            var collection = await Get(db);
            var update = Builders<Counter>.Update.Inc(x => x.State, (uint)N);
            
            var options = new FindOneAndUpdateOptions<Counter>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };
            
            var updated = await collection.FindOneAndUpdateAsync<Counter>(session, x => x.Name == counterName, update, options);

            var from = updated.State - N;
            return Enumerable.Range((int)from, N);
        }
    }
}