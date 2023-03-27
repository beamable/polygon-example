using System;
using System.Threading.Tasks;
using Beamable.Microservices.PolygonFederation.Features.Transactions.Storage.Models;
using MongoDB.Driver;

namespace Beamable.Microservices.PolygonFederation.Features.Transactions.Storage
{
    internal static class TransactionCollection
    {
        private static IMongoCollection<TransactionRecord> _collection;

        private static async ValueTask<IMongoCollection<TransactionRecord>> Get(IMongoDatabase db)
        {
            if (_collection is null)
            {
                _collection = db.GetCollection<TransactionRecord>("transaction");

                await _collection.Indexes.CreateOneAsync(
                    new CreateIndexModel<TransactionRecord>(
                        Builders<TransactionRecord>.IndexKeys
                            .Ascending(x => x.ExpireAt),
                        new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }
                    )
                );
            }

            return _collection;
        }

        public static async Task<bool> TryInsertTransaction(this IMongoDatabase db, string transactionId)
        {
            var collection = await Get(db);
            try
            {
                await collection.InsertOneAsync(new TransactionRecord
                {
                    Id = transactionId
                });
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return false;
            }
        }

        public static async Task DeleteTransaction(this IMongoDatabase db, string transactionId)
        {
            var collection = await Get(db);
            await collection.DeleteOneAsync(x => x.Id == transactionId);
        }
    }
}