using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;
using MongoDB.Driver;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting.Storage
{
    public static class MintCollection
    {
        private static IMongoCollection<Mint> _collection;

        private static async ValueTask<IMongoCollection<Mint>> Get(IMongoDatabase db)
        {
            if (_collection is null)
            {
                _collection = db.GetCollection<Mint>("contract");
                await _collection.Indexes.CreateManyAsync(new[]
                    {
                        new CreateIndexModel<Mint>(
                            Builders<Mint>.IndexKeys
                                .Ascending(x => x.OwnerAddress)
                                .Ascending(x => x.ContentId)
                                .Ascending(x => x.PublicKey),
                            new CreateIndexOptions { Unique = true }
                        ),
                        new CreateIndexModel<Mint>(
                            Builders<Mint>.IndexKeys
                                .Ascending(x => x.ContentId)
                                .Ascending(x => x.PublicKey),
                            new CreateIndexOptions { Unique = false }
                        )
                    }
                );
            }

            return _collection;
        }

        public static async Task<List<Contract>> GetContractsFor(this IMongoDatabase db, IEnumerable<string> contentIds)
        {
            var collection = await Get(db);
            return await collection.Aggregate()
                .Group(x => new { x.ContentId, x.PublicKey },
                    x => new Contract
                    {
                        ContentId = x.Key.ContentId,
                        PublicKey = x.Key.PublicKey
                    })
                .Match(x => contentIds.Contains(x.ContentId))
                .ToListAsync();
        }

        public static async Task<List<Mint>> GetMintsFor(this IMongoDatabase db, string ownerAddress)
        {
            var collection = await Get(db);
            var mints = await collection
                .Find(x => x.OwnerAddress == ownerAddress)
                .ToListAsync();
            return mints;
        }

        public static async Task UpsertMints(this IMongoDatabase db, IEnumerable<Mint> mints)
        {
            var collection = await Get(db);
            var ops = mints
                .Select(mint => new ReplaceOneModel<Mint>
                    (Builders<Mint>.Filter.Where(x => x.OwnerAddress == mint.OwnerAddress && x.ContentId == mint.ContentId && x.PublicKey == mint.PublicKey), mint)
                    { IsUpsert = true })
                .ToList();
            await collection.BulkWriteAsync(ops);
        }
    }
}