﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;
using MongoDB.Driver;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting.Storage
{
    public static class MintCollection
    {
        private static IMongoCollection<MintMapping> _collection;

        private static async ValueTask<IMongoCollection<MintMapping>> Get(IMongoDatabase db)
        {
            if (_collection is null)
            {
                _collection = db.GetCollection<MintMapping>("mint");
                await _collection.Indexes.CreateManyAsync(new[]
                    {
                        new CreateIndexModel<MintMapping>(
                            Builders<MintMapping>.IndexKeys
                                .Ascending(x => x.OwnerAddress)
                                .Ascending(x => x.ContentId)
                                .Ascending(x => x.PublicKey),
                            new CreateIndexOptions { Unique = true }
                        )
                    }
                );
            }

            return _collection;
        }

        public static async Task<List<MintMapping>> GetMintMappingsFor(this IMongoDatabase db, string ownerAddress)
        {
            var collection = await Get(db);
            var mints = await collection
                .Find(x => x.OwnerAddress == ownerAddress)
                .ToListAsync();
            return mints;
        }

        public static async Task InsertMintMapping(this IMongoDatabase db, MintMapping mint)
        {
            var collection = await Get(db);
            try
            {
                await collection.InsertOneAsync(mint);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // Ignore duplicate key error
            }
        }
    }
}