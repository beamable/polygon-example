using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts.Functions.Messages
{
    [Function("getInventory", "uint256[], uint256[]")]
    internal class ER1155GetInventoryFunctionMessage : FunctionMessage
    {
        [Parameter("address", "account")] public virtual string Account { get; set; }
    }
}