using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.PolygonFederation.Features.Minting.Storage.Models
{
    public class Counter
    {
        [BsonElement("_id")]
        public string Name { get; set; }
        public uint State { get; set; }
    }
}