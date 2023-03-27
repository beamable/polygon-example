using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.PolygonFederation.Features.Contracts.Storage.Models
{
    public record Contract
    {
        [BsonElement("_id")]
        public string Name { get; set; }
        public string PublicKey { get; set; }
        public string BaseMetadataUri { get; set; }
    }
}