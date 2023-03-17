using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts.Exceptions
{
    internal class ContractNotLoadedException : MicroserviceException
    {
        public ContractNotLoadedException(string message) : base((int)HttpStatusCode.BadRequest, "ContractNotLoadedError", message)
        {
        }
    }
}