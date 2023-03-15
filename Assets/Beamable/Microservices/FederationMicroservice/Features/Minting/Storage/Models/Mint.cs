using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models
{
    public record Mint : TokenIdMapping
    {
        [BsonElement("_id")] public ObjectId ID { get; set; } = ObjectId.GenerateNewId();
        public string ContractName { get; set; }
    }
}