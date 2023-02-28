using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Beamable.Microservices.FederationMicroservice.SolidityCompiler.Models
{
    public class SolidityCompilerOutput
    {
        [JsonProperty("errors")] public Collection<OutputError>? Errors { get; set; }
        [JsonProperty("sources")] public Dictionary<string, OutputSource> Sources { get; set; } = null!;
        [JsonProperty("contracts")] public OutputContracts Contracts { get; set; } = null!;

        public class OutputError
        {
            [JsonProperty("type")] public string Type { get; set; } = null!;
            [JsonProperty("component")] public string Component { get; set; } = null!;
            [JsonProperty("severity")] public string Severity { get; set; } = null!;
            [JsonProperty("errorCode")] public string ErrorCode { get; set; } = null!;
            [JsonProperty("message")] public string Message { get; set; } = null!;
            [JsonProperty("formattedMessage")] public string FormattedMessage { get; set; } = null!;
        }

        public class OutputSource
        {
            [JsonProperty("id")] public int Id { get; set; }
        }

        public class OutputContracts
        {
            [JsonProperty("contract")] public Dictionary<string, OutputContract> Contract { get; set; } = null!;

            public class OutputContract
            {
                [JsonExtensionData] public Dictionary<string, JToken> Data { get; set; } = null!;
            }
        }

        public bool HasErrors => Errors?.Any(x => x.Severity == "error") == true;
    }
}