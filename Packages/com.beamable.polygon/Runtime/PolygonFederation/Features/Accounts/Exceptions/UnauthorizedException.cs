using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.PolygonFederation.Features.Accounts.Exceptions
{
    internal class UnauthorizedException : MicroserviceException

    {
        public UnauthorizedException() : base((int)HttpStatusCode.Unauthorized, "Unauthorized", "")
        {
        }
    }
}