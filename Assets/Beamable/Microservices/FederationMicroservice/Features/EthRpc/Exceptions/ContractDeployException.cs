﻿using System.Net;
using Beamable.Server;

namespace Beamable.Microservices.FederationMicroservice.Features.EthRpc.Exceptions
{
    internal class ContractDeployException : MicroserviceException
    {
        public ContractDeployException(string message) : base((int)HttpStatusCode.InternalServerError, "ContractDeployError",
            message)
        {
        }
    }
}