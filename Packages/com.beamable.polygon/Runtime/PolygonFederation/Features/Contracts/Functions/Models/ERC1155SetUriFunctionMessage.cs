using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.PolygonFederation.Features.Contracts.Functions.Models
{
    [Function("setURI")]
    internal class ER1155SetUriFunctionMessage : FunctionMessage
    {
        [Parameter("string", "newUri")]
        public virtual string NewUri { get; set; }
    }
}