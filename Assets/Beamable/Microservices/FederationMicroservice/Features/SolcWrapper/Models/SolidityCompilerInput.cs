using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Beamable.Microservices.FederationMicroservice.Features.SolcWrapper.Models
{
    public class SolidityCompilerInput
    {
        public SolidityCompilerInput(string contractSourceCode, IEnumerable<string> contractOutputSelection)
        {
            Sources = new InputSource
            {
                Contract = new InputSource.InputContract
                {
                    Content = contractSourceCode
                }
            };
            Settings = new OutputSettings
            {
                OutputSelection = new OutputSettings.Selection
                {
                    Contract = new Dictionary<string, List<string>>
                    {
                        { "*", contractOutputSelection.ToList() }
                    }
                }
            };
        }

        [JsonProperty("language")]
        public string Language { get; set; } = "Solidity";

        [JsonProperty("sources")]
        public InputSource Sources { get; set; }

        [JsonProperty("settings")]
        public OutputSettings Settings { get; set; }

        public class InputSource
        {
            [JsonProperty("contract")]
            public InputContract Contract { get; set; } = null!;

            public class InputContract
            {
                [JsonProperty("content")]
                public string Content { get; set; } = null!;
            }
        }

        public class OutputSettings
        {
            [JsonProperty("outputSelection")]
            public Selection OutputSelection { get; set; } = null!;

            public class Selection
            {
                [JsonProperty("contract")]
                public IDictionary<string, List<string>> Contract { get; set; } = null!;
            }
        }
    }
}