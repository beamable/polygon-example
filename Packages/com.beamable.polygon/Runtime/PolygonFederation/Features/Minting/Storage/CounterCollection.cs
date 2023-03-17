using System.Threading.Tasks;
using Beamable.Microservices.PolygonFederation.Features.Minting.Storage.Models;
using MongoDB.Driver;

namespace Beamable.Microservices.PolygonFederation.Features.Minting.Storage
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

        public static async Task<uint> GetNextCounterValue(this IMongoDatabase db, string counterName)
        {
            var collection = await Get(db);
            var update = Builders<Counter>.Update.Inc(x => x.State, (uint)1);

            var options = new FindOneAndUpdateOptions<Counter>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var updated = await collection.FindOneAndUpdateAsync<Counter>(x => x.Name == counterName, update, options);

            return updated.State - 1;
        }
    }
}