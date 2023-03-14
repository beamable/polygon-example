using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Microservices.FederationMicroservice.Features.Accounts;
using Beamable.Microservices.FederationMicroservice.Features.Accounts.Exceptions;
using Beamable.Microservices.FederationMicroservice.Features.Contracts;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Functions.Models;
using Beamable.Microservices.FederationMicroservice.Features.EthRpc;
using Beamable.Microservices.FederationMicroservice.Features.Minting;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;
using Nethereum.Web3;
using ItemCreateRequest = Beamable.Common.Api.Inventory.ItemCreateRequest;

namespace Beamable.Microservices.FederationMicroservice
{
    [Microservice("FederationMicroservice")]
    public class FederationMicroservice : Microservice, IFederatedInventory<PolygonCloudIdentity>
    {
        /// TODO:
        ///  - support for items
        ///  - metadata special props -> $image, $... (plus reverse mapping)
        ///  - add supply tracking to contract
        ///  - remove "amount" from Mint collection and make the combination (contract, token, content) unique -> simplify queries, remove aggregation

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
            if (currencies.Any() || newItems.Any())
            {
                var currencyMints = currencies.Select(c => new MintRequest
                {
                    ContentId = c.Key,
                    Amount = (uint)c.Value,
                    Properties = new Dictionary<string, string>(),
                    IsUnique = false
                });

                var itemMints = newItems.Select(i => new MintRequest
                {
                    ContentId = i.contentId,
                    Amount = 1,
                    Properties = i.properties,
                    IsUnique = true
                });

                await MintingService.Mint(id, currencyMints.Union(itemMints).ToList());
            }

            return await GetInventoryState(id);
        }

        public async Promise<FederatedInventoryProxyState> GetInventoryState(string id)
        {
            var inventoryResponse = await ServiceContext.RpcClient
                .SendFunctionQueryAsync<ER1155GetInventoryFunctionMessage, ERC1155GetInventoryFunctionOutput>(
                    ServiceContext.DefaultContract.PublicKey,
                    new ER1155GetInventoryFunctionMessage
                    {
                        Account = id
                    });
            
            var existingMints = (await ServiceContext.Database.GetTokenMappingsForTokens(Configuration.DefaultContractName, inventoryResponse.TokenIds))
                .ToDictionary(x => x.TokenId, x => x);
            
            var currencies = new Dictionary<string, long>();

            for (int i = 0; i < inventoryResponse.TokenIds.Count; i++)
            {
                var tokenId = inventoryResponse.TokenIds[i];
                var contentId = existingMints[tokenId].ContentId;
                var hash = inventoryResponse.MetadataHashes[i];
                var amount = inventoryResponse.TokenAmounts[i];

                if (contentId.StartsWith("currency."))
                {
                    currencies.Add(contentId, amount);
                }
            }
            
            return new FederatedInventoryProxyState
            {
                currencies = currencies
            };
        }
    }
}