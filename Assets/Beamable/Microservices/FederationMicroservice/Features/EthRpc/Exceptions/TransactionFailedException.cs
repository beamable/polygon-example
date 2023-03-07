using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.FederationMicroservice.Features.EthRpc.Exceptions
{
    internal class TransactionFailedException : MicroserviceException
    {
        public TransactionFailedException(string message) : base((int)HttpStatusCode.InternalServerError, "TransactionFailedError",
            message)
        {
        }
    }
}