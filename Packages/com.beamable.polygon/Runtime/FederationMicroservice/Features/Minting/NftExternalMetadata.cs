using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting
{
    [Serializable]
    public class NftExternalMetadata
    {
        public NftExternalMetadata(Dictionary<string, string> properties)
        {
            SpecialProperties = new Dictionary<string, object>();
            Properties = new Dictionary<string, string>();

            foreach (var property in properties)
                if (property.Key.StartsWith("$"))
                    SpecialProperties.Add(property.Key.TrimStart('$'), property.Value);
                else
                    Properties.Add(property.Key, property.Value);
        }

        [JsonExtensionData]
        public Dictionary<string, object> SpecialProperties { get; }

        [JsonProperty("properties")]
        private Dictionary<string, string> Properties { get; }

        public Dictionary<string, string> GetProperties()
        {
            var properties = new Dictionary<string, string>();

            foreach (var data in SpecialProperties) properties.Add($"${data.Key}", data.Value.ToString() ?? "");

            foreach (var property in Properties) properties.Add(property.Key, property.Value);

            return properties;
        }

        public static class SpecialProperty
        {
            public const string Name = "$name";
            public const string Image = "$image";
            public const string Description = "$description";
            public const string Uri = "$uri";
        }
    }
}