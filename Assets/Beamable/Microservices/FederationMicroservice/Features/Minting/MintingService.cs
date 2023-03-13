using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Microservices.FederationMicroservice.Features.Contracts;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Exceptions;
using Beamable.Microservices.FederationMicroservice.Features.EthRpc;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Exceptions;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler.Models;
using Nethereum.Web3;
using Stubble.Core;
using Stubble.Core.Builders;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting
{
    internal static class MintingService
    {
        private static readonly StubbleVisitorRenderer Stubble = new StubbleBuilder().Build();

        public static async Task<Contract> CreateContract(IContentObject contentObject, IDictionary<string, string> properties = null)
        {
            BeamableLogger.Log("Creating contract for {contentId}", contentObject.Id);

            if (contentObject is not IBlockchainContent) throw new ContentNotMintable($"The type {contentObject.GetType().FullName} is not supported");

            var contractTemplate = await ((IBlockchainContent)contentObject).GetContractTemplate();

            var compilerOutput = await Compile(contentObject.Id, contractTemplate.GetTemplate(), properties);
            var contractOutput = compilerOutput.Contracts.Contract.First().Value;
            var abi = contractOutput.GetAbi();
            var contractByteCode = contractOutput.GetBytecode();

            var rpcClient = new EthRpcClient(new Web3(ServiceContext.RealmAccount, Configuration.RPCEndpoint));
            var gas = await rpcClient.EstimateContractGasAsync(ServiceContext.RealmAccount, abi, contractByteCode, ServiceContext.RealmAccount.Address);
            var result = await rpcClient.DeployContractAsync(ServiceContext.RealmAccount, abi, contractByteCode, gas);

            var contract = new Contract
            {
                ContentId = contentObject.Id,
                PublicKey = result.ContractAddress,
                Type = contractTemplate.GetContractType()
            };

            await ServiceContext.Database.TryInsertContract(contract);
            
            return contract;
        }

        private static async Task<SolidityCompilerOutput> Compile(string contentId, string contractTemplate, IDictionary<string, string> properties = null)
        {
            // ReSharper disable once MethodHasAsyncOverload
            var contractSource = Stubble.Render(contractTemplate, new { ContentId = contentId, Properties = properties });

            var compilerInput = new SolidityCompilerInput(contractSource, new[] { "abi", "evm.bytecode" });
            
            var compilerOutput = await Solc.Compile(compilerInput);

            if (compilerOutput.HasErrors)
            {
                BeamableLogger.LogError("Compile errors: {@compileErrors}", compilerOutput.Errors.Select(x => x.Message).ToList());
                throw new ContractCompilationException(compilerOutput.Errors);
            }

            return compilerOutput;
        }

        public static async Task<MintMapping> Mint(Contract contract, long amount, string toAddress, IDictionary<string, string> properties = null)
        {
            BeamableLogger.Log("Minting {amount} of {contract} to {owner}", amount, contract.PublicKey, toAddress);

            var newMint = new MintMapping
            {
                ContentId = contract.ContentId,
                PublicKey = contract.PublicKey,
                OwnerAddress = toAddress
            };

            var web3 = new Web3(ServiceContext.RealmAccount, Configuration.RPCEndpoint);
            var contractInteractionService = contract.Type.GetContractInteractionService(web3);

            await contractInteractionService.Mint(contract, amount, toAddress, properties);

            await ServiceContext.Database.InsertMintMapping(newMint);
            
            return newMint;
        }
    }
}