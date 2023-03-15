using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Functions.Models;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting
{
    internal static class MintingService
    {
        public static async Task Mint(string toAddress, IList<MintRequest> requests)
        {
            var db = ServiceContext.Database;

            var nonUniqueContentIds = requests
                .Where(x => !x.IsUnique)
                .Select(x => x.ContentId)
                .ToHashSet();

            var existingMints = (await db.GetTokenMappingsForContent(Configuration.DefaultContractName, nonUniqueContentIds))
                .ToDictionary(x => x.ContentId, x => x);

            var tokenIds = new List<BigInteger>();
            var tokenAmounts = new List<BigInteger>();
            var tokenMetadataHashes = new List<string>();

            var mongoSession = await db.Client.StartSessionAsync();
            var isMongoStandalone = mongoSession.Client.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.Standalone;
            
            if (!isMongoStandalone)
                mongoSession.StartTransaction();

            var mints = new List<Mint>();

            foreach (var request in requests)
            {
                var maybeExistingMint = existingMints.GetValueOrDefault(request.ContentId);
                var tokenId = maybeExistingMint switch
                {
                    { } m => m.TokenId,
                    _ => await db.GetNextCounterValue(mongoSession, Configuration.DefaultContractName)
                };
                var metadataHash = request.IsUnique ? await SaveMetadata(request) : "";

                tokenAmounts.Add(request.Amount);
                tokenMetadataHashes.Add(metadataHash);
                tokenIds.Add(tokenId);

                mints.Add(new Mint
                {
                    ContentId = request.ContentId,
                    ContractName = Configuration.DefaultContractName,
                    TokenId = tokenId
                });
                BeamableLogger.Log("Generated mint: {@mint}", new { request.ContentId, request.Amount, request.Properties, request.IsUnique, TokenId = tokenId, MetadataHash = metadataHash });
            }

            var functionMessage = new ERC1155BatchMintFunctionMessage
            {
                To = toAddress,
                TokenIds = tokenIds,
                Amounts = tokenAmounts,
                MetadataHashes = tokenMetadataHashes
            };

            await ServiceContext.RpcClient.SendRequestAndWaitForReceiptAsync(ServiceContext.DefaultContract.PublicKey, functionMessage);

            await db.InsertMints(mongoSession, mints);
            
            if (mongoSession.IsInTransaction)
                await mongoSession.CommitTransactionAsync();
        }

        private static async Task<string> SaveMetadata(MintRequest request)
        {
            var uriString = await NtfExternalMetadataService.SaveExternalMetadata(new NftExternalMetadata(new Dictionary<string, string>(request.Properties)
            {
                { NftExternalMetadata.SpecialProperty.Name, request.ContentId }
            }));
            BeamableLogger.Log("Metadata URI: {uri}", uriString);
            var uri = new Uri(uriString);
            return uri.Segments.Last();
        }
    }

    internal class MintRequest
    {
        public string ContentId { get; set; }
        public uint Amount { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public bool IsUnique { get; set; }
    }
}