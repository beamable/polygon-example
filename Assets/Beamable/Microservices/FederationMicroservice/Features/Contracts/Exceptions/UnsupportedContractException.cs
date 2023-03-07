using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts.Exceptions
{
    internal class UnsupportedContractException: MicroserviceException
    {
        public UnsupportedContractException(string message) : base((int)HttpStatusCode.InternalServerError, "UnsupportedContractError",
            message)
        {
        }
    }
}