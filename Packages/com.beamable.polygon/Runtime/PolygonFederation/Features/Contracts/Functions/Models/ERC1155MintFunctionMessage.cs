using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.PolygonFederation.Features.Contracts.Functions.Models
{
    [Function("mint")]
    internal class ERC1155MintFunctionMessage : FunctionMessage
    {
        [Parameter("address", "to")]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 2)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "amount", 3)]
        public virtual BigInteger Amount { get; set; }

        [Parameter("string", "metadataHash", 4)]
        public virtual string MetadataHash { get; set; }
    }
}