﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts.Storage.Models
{
    public record Contract
    {
        [BsonElement("_id")]
        public ObjectId ID { get; set; } = ObjectId.GenerateNewId();
        public string Name { get; set; }
        public string PublicKey { get; set; }
    }
}