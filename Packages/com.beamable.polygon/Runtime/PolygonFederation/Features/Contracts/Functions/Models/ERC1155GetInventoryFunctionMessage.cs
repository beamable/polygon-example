using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.PolygonFederation.Features.Contracts.Functions.Models
{
    [Function("getInventory", "uint256[], uint256[]")]
    internal class ER1155GetInventoryFunctionMessage : FunctionMessage
    {
        [Parameter("address", "account")]
        public virtual string Account { get; set; }
    }
}