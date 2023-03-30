using System;
using System.Text;
using Beamable.Common;
using Beamable.Microservices.PolygonFederation.Features.Accounts.Exceptions;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;

namespace Beamable.Microservices.PolygonFederation.Features.Accounts
{
    internal class AuthenticationService
    {
        public static bool IsSignatureValid(string publicKey, string challenge, string signature)
        {
            try
            {
                var challengeBytes = Encoding.UTF8.GetBytes(challenge);
                var signatureBytes = Convert.FromBase64String(signature);

                if (!EthECDSASignature.IsValidDER(signatureBytes))
                {
                    BeamableLogger.LogWarning("Signature {signature} is not a valid DER");
                }

                var key = new EthECKey(publicKey.HexToByteArray(), false);
                return key.Verify(challengeBytes, EthECDSASignature.FromDER(signatureBytes));
            }
            catch (Exception ex)
            {
                BeamableLogger.LogError(ex);
                throw new UnauthorizedException();
            }
        }
    }
}