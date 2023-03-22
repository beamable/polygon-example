using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Inventory;
using Beamable.Microservices.PolygonFederation.Features.Accounts;
using Beamable.Microservices.PolygonFederation.Features.Accounts.Exceptions;
using Beamable.Microservices.PolygonFederation.Features.Contracts;
using Beamable.Microservices.PolygonFederation.Features.Contracts.Exceptions;
using Beamable.Microservices.PolygonFederation.Features.Contracts.Functions.Models;
using Beamable.Microservices.PolygonFederation.Features.EthRpc;
using Beamable.Microservices.PolygonFederation.Features.Minting;
using Beamable.Microservices.PolygonFederation.Features.Minting.Storage;
using Beamable.Polygon.Common;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;
using Nethereum.Web3;

namespace Beamable.Microservices.PolygonFederation
{
    [Microservice("PolygonFederation", CustomAutoGeneratedClientPath =
        "Packages/com.beamable.polygon/Runtime/Client/Autogenerated/PolygonFederationClient")]
    public class PolygonFederation : Microservice, IFederatedInventory<PolygonCloudIdentity>
    {
        private static bool _initialized;
        
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

        [ClientCallable("polygon/get-realm-account")]
        public async Promise<string> GetRealmAccount()
        {
            var realmAccount = await AccountsService.GetOrCreateRealmAccount();
            return realmAccount.Address;
        }

        public async Promise<FederatedInventoryProxyState> StartInventoryTransaction(string id, string transaction, Dictionary<string, long> currencies, List<ItemCreateRequest> newItems)
        {
            await InitializeDefaultContract();

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
            await InitializeDefaultContract();

            var inventoryResponse = await ServiceContext.RpcClient
                .SendFunctionQueryAsync<ER1155GetInventoryFunctionMessage, ERC1155GetInventoryFunctionOutput>(
                    ServiceContext.DefaultContract.PublicKey,
                    new ER1155GetInventoryFunctionMessage
                    {
                        Account = id
                    });

            var existingMints = (await ServiceContext.Database.GetTokenMappingsForTokens(ContractService.DefaultContractName, inventoryResponse.TokenIds))
                .ToDictionary(x => x.TokenId, x => x);

            var currencies = new Dictionary<string, long>();
            var items = new List<(string, FederatedItemProxy)>();

            for (var i = 0; i < inventoryResponse.TokenIds.Count; i++)
            {
                var tokenId = inventoryResponse.TokenIds[i];
                var contentId = existingMints[tokenId].ContentId;
                var hash = inventoryResponse.MetadataHashes[i];
                var amount = inventoryResponse.TokenAmounts[i];

                if (contentId.StartsWith("currency.")) currencies.Add(contentId, amount);

                if (contentId.StartsWith("items."))
                    items.Add((contentId, new FederatedItemProxy
                    {
                        proxyId = tokenId.ToString(),
                        properties = await NtfExternalMetadataService.LoadItemProperties(hash)
                    }));
            }

            var itemGroups = items
                .GroupBy(i => i.Item1)
                .ToDictionary(g => g.Key, g => g.Select(i => i.Item2).ToList());

            return new FederatedInventoryProxyState
            {
                currencies = currencies,
                items = itemGroups
            };
        }

        [InitializeServices]
        public static async Task Initialize(IServiceInitializer initializer)
        {
            try
            {
                var storage = initializer.GetService<IStorageObjectConnectionProvider>();
                var database = await storage.PolygonStorageDatabase();
                ServiceContext.Database = database;
                ServiceContext.Requester = initializer.GetService<IBeamableRequester>();

                // Load realm account/wallet
                var realmAccount = await AccountsService.GetOrCreateRealmAccount();
                ServiceContext.RealmAccount = realmAccount;
                
                // Load realm configuration
                var realmConfigService = initializer.GetService<IMicroserviceRealmConfigService>();
                Configuration.RealmConfig = await realmConfigService.GetRealmConfigSettings();
                
                // Validate configuration
                if (string.IsNullOrEmpty(Configuration.RPCEndpoint))
                {
                    throw new ConfigurationException($"{nameof(Configuration.RPCEndpoint)} is not defined in realm config. Please apply the configuration and restart the service to make it operational.");
                }
                
                // Set the RPC client
                ServiceContext.RpcClient = new EthRpcClient(new Web3(realmAccount, Configuration.RPCEndpoint, default, null));

                await InitializeDefaultContract();
            }
            catch (Exception ex)
            {
                BeamableLogger.LogException(ex);
                BeamableLogger.LogWarning("Service initialization failed. Please fix the issues before using the service.");
            }
        }

        private static async ValueTask InitializeDefaultContract()
        {
            if (ServiceContext.DefaultContract is null)
            {
                try
                {
                    // Load the default contract
                    var defaultContract = await ContractService.GetOrCreateDefaultContract();
                    ServiceContext.DefaultContract = defaultContract;
                }
                catch (Exception ex)
                {
                    BeamableLogger.LogException(ex);
                    throw new ContractNotLoadedException("Default contract can't be loaded. Please fix the issues and try again.");
                }
            }
        }
    }
}