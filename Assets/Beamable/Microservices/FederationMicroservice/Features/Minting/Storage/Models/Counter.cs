using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models
{
    public class Counter
    {
        [BsonElement("_id")] public ObjectId ID { get; set; } = ObjectId.GenerateNewId();

        public string Name { get; set; }
        public uint State { get; set; }
    }
}