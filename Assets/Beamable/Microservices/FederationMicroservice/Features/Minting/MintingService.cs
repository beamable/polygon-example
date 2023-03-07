using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Content.Contracts;
using Beamable.Microservices.FederationMicroservice.Features.Contracts;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Exceptions;
using Beamable.Microservices.FederationMicroservice.Features.EthRpc;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Exceptions;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler.Models;
using MongoDB.Driver;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Stubble.Core;
using Stubble.Core.Builders;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting
{
    internal static class MintingService
    {
        private static readonly StubbleVisitorRenderer Stubble = new StubbleBuilder().Build();

        public static async Task<Contract> CreateContract(string contentId, Account realmAccount, IContentObject contentObject, IDictionary<string, string> properties = null)
        {
            if (contentObject is not IContractContent) throw new ContentNotMintable($"The type {contentObject.GetType().FullName} is not supported");

            var compilerOutput = await Compile(contentObject, properties);
            var contractOutput = compilerOutput.Contracts.Contract.First().Value;

            var abi = contractOutput.GetAbi();
            var contractByteCode = contractOutput.GetBytecode();

            var rpcClient = new EthRpcClient(new Web3(realmAccount, Configuration.RPCEndpoint));
            var gas = await rpcClient.EstimateContractGasAsync(realmAccount, abi, contractByteCode, realmAccount.Address);
            var result = await rpcClient.DeployContractAsync(realmAccount, abi, contractByteCode, gas);

            return new Contract
            {
                ContentId = contentId,
                PublicKey = result.ContractAddress,
                Type = ((IContractContent)contentObject).GetContractType()
            };
        }

        private static async Task<SolidityCompilerOutput> Compile(IContentObject contentObject, IDictionary<string, string> properties = null)
        {
            var contractTemplate = ((IContractContent)contentObject).GetTemplate();

            // ReSharper disable once MethodHasAsyncOverload
            var contractSource = Stubble.Render(contractTemplate, new { ContentId = contentObject.Id, Properties = properties });

            var compilerInput = new SolidityCompilerInput(contractSource, new[] { "abi", "evm.bytecode" });
            var compilerOutput = await Solc.Compile(compilerInput);

            if (compilerOutput.HasErrors)
            {
                BeamableLogger.LogError("Compile errors: {@compileErrors}", compilerOutput.Errors.Select(x => x.Message).ToList());
                throw new ContractCompilationException(compilerOutput.Errors);
            }

            return compilerOutput;
        }

        public static async Task<Mint> Mint(IMongoDatabase db, Contract contract, long amount, string ownerAddress, Account realmAccount, IDictionary<string, string> properties = null)
        {
            var newMint = new Mint
            {
                ContentId = contract.ContentId,
                PublicKey = contract.PublicKey,
                OwnerAddress = ownerAddress
            };

            var web3 = new Web3(realmAccount, Configuration.RPCEndpoint);
            var contractInteractionService = contract.Type.GetContractInteractionService(web3);

            await contractInteractionService.Mint(contract, amount, ownerAddress, properties);

            await db.UpsertMints(new[] { newMint });
            return newMint;
        }
    }
}