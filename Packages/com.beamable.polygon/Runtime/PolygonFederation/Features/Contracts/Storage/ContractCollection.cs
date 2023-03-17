using System.Threading.Tasks;
using Beamable.Microservices.PolygonFederation.Features.Contracts.Storage.Models;
using MongoDB.Driver;

namespace Beamable.Microservices.PolygonFederation.Features.Contracts.Storage
{
    public static class ContractCollection
    {
        private static IMongoCollection<Contract> _collection;

        private static async ValueTask<IMongoCollection<Contract>> Get(IMongoDatabase db)
        {
            if (_collection is null)
            {
                _collection = db.GetCollection<Contract>("contract");
                await _collection.Indexes.CreateManyAsync(new[]
                    {
                        new CreateIndexModel<Contract>(
                            Builders<Contract>.IndexKeys
                                .Ascending(x => x.Name)
                                .Ascending(x => x.PublicKey),
                            new CreateIndexOptions { Unique = true }
                        )
                    }
                );
            }

            return _collection;
        }

        public static async Task<Contract> GetContract(this IMongoDatabase db, string name)
        {
            var collection = await Get(db);
            return await collection
                .Find(x => x.Name == name)
                .FirstOrDefaultAsync();
        }

        public static async Task<bool> TryInsertContract(this IMongoDatabase db, Contract contract)
        {
            var collection = await Get(db);
            try
            {
                await collection.InsertOneAsync(contract);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // Ignore duplicate key errors
                return false;
            }
        }
    }
}