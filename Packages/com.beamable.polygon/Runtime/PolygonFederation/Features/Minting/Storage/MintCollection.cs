using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Microservices.PolygonFederation.Features.Minting.Storage.Models;
using MongoDB.Driver;

namespace Beamable.Microservices.PolygonFederation.Features.Minting.Storage
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
                            new CreateIndexOptions { Unique = true }
                        ),
                        new CreateIndexModel<Mint>(
                            Builders<Mint>.IndexKeys
                                .Ascending(x => x.ContractName)
                                .Ascending(x => x.TokenId)
                                .Ascending(x => x.ContentId),
                            new CreateIndexOptions { Unique = true }
                        )
                    }
                );
            }

            return _collection;
        }

        public static async Task<List<TokenIdMapping>> GetTokenMappingsForContent(this IMongoDatabase db, string contractName, IEnumerable<string> contentIds)
        {
            var collection = await Get(db);
            var mints = await collection
                .Find(x => x.ContractName == contractName && contentIds.Contains(x.ContentId))
                .Project(x => new TokenIdMapping
                {
                    ContentId = x.ContentId,
                    TokenId = x.TokenId
                })
                .ToListAsync();

            return mints;
        }

        public static async Task<List<TokenIdMapping>> GetTokenMappingsForTokens(this IMongoDatabase db, string contractName, IEnumerable<uint> tokenIds)
        {
            var collection = await Get(db);
            var mints = await collection
                .Find(x => x.ContractName == contractName && tokenIds.Contains(x.TokenId))
                .Project(x => new TokenIdMapping
                {
                    ContentId = x.ContentId,
                    TokenId = x.TokenId
                })
                .ToListAsync();

            return mints;
        }

        public static async Task InsertMints(this IMongoDatabase db, IEnumerable<Mint> mints)
        {
            var collection = await Get(db);
            var options = new InsertManyOptions
            {
                IsOrdered = false
            };
            try
            {
                await collection.InsertManyAsync(mints, options);
            }
            catch (MongoBulkWriteException e) when (e.WriteErrors.All(x => x.Category == ServerErrorCategory.DuplicateKey))
            {
                // Ignore
            }
        }
    }
}