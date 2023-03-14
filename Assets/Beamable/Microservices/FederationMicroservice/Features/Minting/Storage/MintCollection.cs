using System.Collections.Generic;
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
                _collection = db.GetCollection<Mint>("mint");
                await _collection.Indexes.CreateManyAsync(new[]
                    {
                        new CreateIndexModel<Mint>(
                            Builders<Mint>.IndexKeys
                                .Ascending(x => x.ContractName)
                                .Ascending(x => x.ContentId)
                                .Ascending(x => x.TokenId),
                            new CreateIndexOptions { Unique = false }
                        )
                    }
                );
            }

            return _collection;
        }

        public static async Task<List<Mint>> GetMints(this IMongoDatabase db, string contractName)
        {
            var collection = await Get(db);
            var mints = await collection
                .Find(x => x.ContractName == contractName)
                .ToListAsync();
            return mints;
        }

        public static async Task<List<Mint>> GetMintsForContent(this IMongoDatabase db, string contractName, string contentId)
        {
            var collection = await Get(db);
            var mints = await collection
                .Find(x => x.ContractName == contractName && x.ContentId == contentId)
                .ToListAsync();
            return mints;
        }

        public static async Task InsertMint(this IMongoDatabase db, Mint mint)
        {
            var collection = await Get(db);
            await collection.InsertOneAsync(mint);
        }

        public static async Task InsertMints(this IMongoDatabase db, IEnumerable<Mint> mints)
        {
            var collection = await Get(db);
            await collection.InsertManyAsync(mints);
        }
    }
}