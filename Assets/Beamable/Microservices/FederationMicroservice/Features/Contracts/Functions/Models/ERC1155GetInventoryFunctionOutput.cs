using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts.Functions.Models
{
    [FunctionOutput]
    internal class ERC1155GetInventoryFunctionOutput : IFunctionOutputDTO
    {
        [Parameter("uint256[]", "tokenIds")] public virtual List<uint> TokenIds { get; set; }

        [Parameter("uint256[]", "tokenAmounts", 2)]
        public virtual List<uint> TokenAmounts { get; set; }

        [Parameter("string[]", "metadataHashes", 3)]
        public virtual List<string> MetadataHashes { get; set; }
    }
}