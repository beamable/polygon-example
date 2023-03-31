using System;
using Beamable.Common;
using Beamable.Microservices.PolygonFederation.Features.Accounts.Exceptions;
using Nethereum.Signer;

namespace Beamable.Microservices.PolygonFederation.Features.Accounts
{
    internal class AuthenticationService
    {
        private static readonly EthereumMessageSigner Signer = new();
        
        // Documentation: https://docs.nethereum.com/en/latest/Nethereum.Workbooks/docs/nethereum-signing-messages/
        public static bool IsSignatureValid(string address, string challenge, string signature)
        {
            try
            {
                var recoveredAddress = Signer.EncodeUTF8AndEcRecover(challenge, signature);
                return (recoveredAddress == address);
            }
            catch (Exception ex)
            {
                BeamableLogger.LogError(ex);
                throw new UnauthorizedException();
            }
        }
    }
}