using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.PolygonFederation.Features.Minting.Exceptions
{
    internal class ContentNotMintable : MicroserviceException
    {
        public ContentNotMintable(string message) : base((int)HttpStatusCode.BadRequest, "ContentNotMintable",
            message)
        {
        }
    }
}