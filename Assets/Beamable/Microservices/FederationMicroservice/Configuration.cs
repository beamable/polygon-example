using System;

namespace Beamable.Microservices.FederationMicroservice
{
    internal static class Configuration
    {
        public const string RPCEndpoint = "https://sepolia.infura.io/v3/0381878048f64d6c9ab3d0fc17b45a69";
        public static readonly string RealmSecret = Environment.GetEnvironmentVariable("SECRET");
        public const int AuthenticationChallengeTtlSec = 600;
    }
}