using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Api.Autogenerated.Models;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Content;
using Beamable.Microservices.FederationMicroservice.Features.Accounts;
using Beamable.Microservices.FederationMicroservice.Features.Accounts.Exceptions;
using Beamable.Microservices.FederationMicroservice.Features.Contracts;
using Beamable.Microservices.FederationMicroservice.Features.EthRpc;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;
using Nethereum.Web3;
using ItemCreateRequest = Beamable.Common.Api.Inventory.ItemCreateRequest;

namespace Beamable.Microservices.FederationMicroservice
{
    [Microservice("FederationMicroservice")]
    public class FederationMicroservice : Microservice, IFederatedInventory<PolygonCloudIdentity>
    {
        [InitializeServices]
        public static async Task Initialize(IServiceInitializer initializer)
        {
            var storage = initializer.GetService<IStorageObjectConnectionProvider>();
            var database = await storage.FederationStorageDatabase();
            ServiceContext.Database = database;
            ServiceContext.Requester = initializer.GetService<IBeamableRequester>();
            
            // Load realm configuration
            var realmConfigService = initializer.GetService<IMicroserviceRealmConfigService>();
            Configuration.RealmConfig = await realmConfigService.GetRealmConfigSettings();

            // Load realm account/wallet
            var realmAccount = await AccountsService.GetOrCreateRealmAccount();
            ServiceContext.RealmAccount = realmAccount;
            
            // Set the RPC client
            ServiceContext.RpcClient = new EthRpcClient(new Web3(realmAccount, Configuration.RPCEndpoint));

            // Load the default contract
            var defaultContract = await ContractService.GetOrCreateContract(Configuration.DefaultContractName, Configuration.DefaultContractSource);
            ServiceContext.DefaultContract = defaultContract;
        }

        public async Promise<FederatedAuthenticationResponse> Authenticate(string token, string challenge, string solution)
        {
            if (Configuration.AllowManagedAccounts)
                if (string.IsNullOrEmpty(token) && Context.UserId != 0L)
                {
                    // Create new account for player if token is empty
                    var account = await AccountsService.GetOrCreateAccount(Context.UserId.ToString());
                    return new FederatedAuthenticationResponse
                    {
                        user_id = account.Address
                    };
                }

            // Challenge-based authentication
            if (!string.IsNullOrEmpty(challenge) && !string.IsNullOrEmpty(solution))
            {
                if (AuthenticationService.IsSignatureValid(token, challenge, solution))
                    // User identity is confirmed
                    return new FederatedAuthenticationResponse
                    {
                        user_id = token
                    };

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

        public async Promise<FederatedInventoryProxyState> StartInventoryTransaction(string id, string transaction, Dictionary<string, long> currencies, List<ItemCreateRequest> newItems)
        {
            var db = ServiceContext.Database;

            var contentIds = currencies.Keys
                .Union(newItems.Select(x => x.contentId))
                .ToHashSet();

            var content = await ResolveContent(contentIds);

            // var contracts = (await db.GetContractsFor(contentIds))
            //     .ToDictionary(x => x.Name, x => x);
            //
            // var tasks = new List<Task>();
            //
            // foreach (var currency in currencies)
            // {
            //     tasks.Add(Task.Run(async () =>
            //     {
            //         try
            //         {
            //             if (!contracts.ContainsKey(currency.Key))
            //             {
            //                 var newCurrencyContract = await MintingService.CreateContract(content[currency.Key]);
            //                 contracts[currency.Key] = newCurrencyContract;
            //             }
            //
            //             await MintingService.Mint(contracts[currency.Key], currency.Value, id);
            //         }
            //         catch (Exception ex)
            //         {
            //             BeamableLogger.LogError(ex);
            //         }
            //     }));
            // }
            //
            // var requester = Requester;
            // var userId = Context.UserId;
            // _ = Task.WhenAll(tasks).ContinueWith(_ => ReportBackTheState(id, userId, requester));

            return new FederatedInventoryProxyState //TODO: treat a NULL response as "I will report later/don't change the state yet"? 
            {
                currencies = null,
                items = null
            };
        }

        public async Promise<FederatedInventoryProxyState> GetInventoryState(string id)
        {
            var db = ServiceContext.Database;
            throw new NotImplementedException();
        }

        private async Task ReportBackTheState(string id, long userId, IBeamableRequester requester)
        {
            BeamableLogger.Log("Reporting back state for {account}", id);
            try
            {
                var state = await GetInventoryState(id);
                await requester.Request<CommonResponse>(Method.PUT, $"/object/inventory/{userId}/proxy/state", state);
            }
            catch (Exception ex)
            {
                BeamableLogger.LogError(ex);
            }
        }

        private async Task<IDictionary<string, IContentObject>> ResolveContent(HashSet<string> contentIds)
        {
            var content = new Dictionary<string, IContentObject>();

            foreach (var contentId in contentIds)
            {
                var contentObject = await Services.Content.GetContent(contentId);
                content[contentId] = contentObject;
            }

            return content;
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