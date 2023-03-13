using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models
{
    public record Mint
    {
        [BsonElement("_id")]
        public ObjectId ID { get; set; } = ObjectId.GenerateNewId();
        public string ContractName { get; set; }
        public string ContentId { get; set; }
        public uint TokenId { get; set; }
    }
}