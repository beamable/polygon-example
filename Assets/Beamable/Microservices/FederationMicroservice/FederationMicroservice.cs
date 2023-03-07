using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Api.Inventory;
using Beamable.Microservices.FederationMicroservice.Features.Accounts;
using Beamable.Microservices.FederationMicroservice.Features.Accounts.Exceptions;
using Beamable.Microservices.FederationMicroservice.Features.Minting;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;

namespace Beamable.Microservices.FederationMicroservice
{
    [Microservice("FederationMicroservice")]
    public class FederationMicroservice : Microservice, IFederatedInventory<PolygonCloudIdentity>
    {
        [InitializeServices]
        public static async Task Initialize(IServiceInitializer initializer)
        {
            // Load realm configuration
            var realmConfigService = initializer.GetService<IMicroserviceRealmConfigService>();
            Configuration.RealmConfig = await realmConfigService.GetRealmConfigSettings();
        }

        public async Promise<FederatedAuthenticationResponse> Authenticate(string token, string challenge, string solution)
        {
            if (Configuration.AllowManagedAccounts)
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

        public async Promise<FederatedInventoryProxyState> GetInventoryState(string id)
        {
            var db = await Storage.FederationStorageDatabase();
            var userMints = await db.GetMintsFor(id);
            return await GetInventoryState(id, userMints);
        }

        public async Promise<FederatedInventoryProxyState> StartInventoryTransaction(string id, string transaction, Dictionary<string, long> currencies, List<ItemCreateRequest> newItems)
        {
            var db = await Storage.FederationStorageDatabase();

            var userMints = await db.GetMintsFor(id);

            var contracts = (await db.GetContractsFor(currencies.Keys.Union(newItems.Select(x => x.contentId))))
                .ToDictionary(x => x.ContentId, x => x);

            var realmAccount = await AccountsService.GetOrCreateRealmAccount(db);

            foreach (var currency in currencies)
            {
                var content = await Services.Content.GetContent(currency.Key);
                if (!contracts.ContainsKey(currency.Key))
                {
                    var newCurrencyContract = await MintingService.CreateContract(currency.Key, realmAccount, content);
                    contracts[currency.Key] = newCurrencyContract;
                }

                var newMint = await MintingService.Mint(db, contracts[currency.Key], currency.Value, id, realmAccount);
                userMints.Add(newMint);
            }

            return await GetInventoryState(id, userMints);
        }

        private async Task<FederatedInventoryProxyState> GetInventoryState(string id, IEnumerable<Mint> userMints)
        {
            throw new NotImplementedException();
        }

        // [ClientCallable]
        // public async Task<string> DeployContract(string sourceCode, string privateKey)
        // {
        //     var input = new SolidityCompilerInput(sourceCode, new[] { "abi", "evm.bytecode" });
        //     var output = await Solc.Compile(input);
        //
        //     var account = new Account(privateKey);
        //     var web3 = new Web3(account, Configuration.RPCEndpoint);
        //     if (output.HasErrors)
        //     {
        //         BeamableLogger.LogError("We have errors - {@output}", output);
        //         throw new Exception("We have errors");
        //     }
        //
        //     var contractOutput = output.Contracts.Contract.First().Value;
        //
        //     var abi = contractOutput.GetAbi();
        //     var contractByteCode = contractOutput.GetBytecode();
        //
        //     BeamableLogger.Log("Estimating gas");
        //     var gas = await web3.Eth.DeployContract.EstimateGasAsync(abi, contractByteCode, account.Address);
        //     BeamableLogger.Log("Gas is {g}", gas);
        //
        //     BeamableLogger.Log("Sending contract");
        //     var result = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(abi, contractByteCode,
        //         account.Address, gas, CancellationToken.None);
        //     var responseString = JsonConvert.SerializeObject(result);
        //     BeamableLogger.Log("Response: {r}", responseString);
        //
        //     return responseString;
        // }
    }
}