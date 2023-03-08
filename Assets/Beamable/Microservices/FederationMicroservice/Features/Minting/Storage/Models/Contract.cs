using Beamable.Common.Content.Contracts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models
{
    public record Contract
    {
        [BsonElement("_id")] public ObjectId ID { get; set; } = ObjectId.GenerateNewId();

        public string ContentId { get; set; }
        public string PublicKey { get; set; }
        public ContractType Type { get; set; }
    }
}