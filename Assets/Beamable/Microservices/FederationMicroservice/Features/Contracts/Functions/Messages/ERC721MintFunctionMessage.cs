using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts.Functions.Messages
{
    [Function("safeMint")]
    internal class ERC721MintFunctionMessage
    {
        [Parameter("address", "to")] public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 2)] public virtual BigInteger TokenId { get; set; }

        [Parameter("string", "uri", 3)] public virtual string Uri { get; set; }
    }
}