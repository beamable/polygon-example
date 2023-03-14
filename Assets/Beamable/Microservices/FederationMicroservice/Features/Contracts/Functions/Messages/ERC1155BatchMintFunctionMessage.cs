using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts.Functions.Messages
{
    [Function("batchMint")]
    internal class ERC1155BatchMintFunctionMessage : FunctionMessage
    {
        [Parameter("address", "to")] public virtual string To { get; set; }
        [Parameter("uint256[]", "tokenIds", 2)] public virtual List<BigInteger> TokenIds { get; set; }
        [Parameter("uint256[]", "amounts", 3)] public virtual List<BigInteger> Amounts { get; set; }
        [Parameter("string[]", "metadataHashes", 4)] public virtual List<string> MetadataHashes { get; set; }
    }
}