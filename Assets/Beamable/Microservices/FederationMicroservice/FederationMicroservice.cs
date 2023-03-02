using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.FederationMicroservice.Features.Accounts;
using Beamable.Microservices.FederationMicroservice.Features.Accounts.Exceptions;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler.Models;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;

namespace Beamable.Microservices.FederationMicroservice
{
    [Microservice("FederationMicroservice")]
    public class FederationMicroservice : Microservice, IFederatedLogin<PolygonCloudIdentity>
    {
        public static RealmConfig RealmConfig;
        
        [InitializeServices]
        public static async Task Initialize(IServiceInitializer initializer)
        {
            var realmConfigService = initializer.GetService<IMicroserviceRealmConfigService>();
            RealmConfig = await realmConfigService.GetRealmConfigSettings();
        }
        
        public async Promise<FederatedAuthenticationResponse> Authenticate(string token, string challenge, string solution)
        {
            if (RealmConfig.GetSetting("federation_polygon", "allowManagedWallets", "true") == "true")
            {
                if (string.IsNullOrEmpty(token) && Context.UserId != 0L)
                {
                    // Create new account for player if token is empty
                    var db = await Storage.FederationStorageDatabase();
                    var account = await AccountsService.GetOrCreateAccount(db, Context.UserId.ToString());
                    return new FederatedAuthenticationResponse
                    {
                        user_id = account.Address
                    };
                }
            }

            // Challenge-based authentication
            if (!string.IsNullOrEmpty(challenge) && !string.IsNullOrEmpty(solution))
            {
                if (AuthenticationService.IsSignatureValid(token, challenge, solution))
                {
                    // User identity is confirmed
                    return new FederatedAuthenticationResponse
                    {
                        user_id = token
                    };
                }
                // Signature is invalid, user identity isn't confirmed
                BeamableLogger.LogWarning(
                    "Invalid signature {signature} for challenge {challenge} and account {account}", solution,
                    challenge, token);
                throw new UnauthorizedException();
            }
            // Generate a challenge
            return new FederatedAuthenticationResponse
            {
                challenge = $"Please sign this random message to authenticate: {Guid.NewGuid()}",
                challenge_ttl = Configuration.AuthenticationChallengeTtlSec
            };
        }

        [ClientCallable]
        public async Task<string> DeployContract(string sourceCode, string privateKey)
        {
            var input = new SolidityCompilerInput(sourceCode, new[] { "abi", "evm.bytecode" });
            var output = await Solc.Compile(input);

            var account = new Account(privateKey);
            var web3 = new Web3(account, Configuration.RPCEndpoint);
            if (output.HasErrors)
            {
                BeamableLogger.LogError("We have errors - {@output}", output);
                throw new Exception("We have errors");
            }

            var contractOutput = output.Contracts.Contract.First().Value;

            var abi = contractOutput.GetAbi();
            var contractByteCode = contractOutput.GetBytecode();

            BeamableLogger.Log("Estimating gas");
            var gas = await web3.Eth.DeployContract.EstimateGasAsync(abi, contractByteCode, account.Address);
            BeamableLogger.Log("Gas is {g}", gas);

            BeamableLogger.Log("Sending contract");
            var result = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(abi, contractByteCode,
                account.Address, gas, CancellationToken.None);
            var responseString = JsonConvert.SerializeObject(result);
            BeamableLogger.Log("Response: {r}", responseString);

            return responseString;
        }
    }
}