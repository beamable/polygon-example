using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts.Functions.Messages
{
    [Function("mint")]
    public class ERC20MintFunctionMessage : FunctionMessage
    {
        [Parameter("address", "to")] public virtual string To { get; set; }

        [Parameter("uint256", "amount", 2)] public virtual BigInteger Amount { get; set; }
    }
}