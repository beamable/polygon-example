using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Exceptions;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Functions.Models;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Storage;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Storage.Models;
using Beamable.Microservices.FederationMicroservice.Features.EthRpc;
using Beamable.Microservices.FederationMicroservice.Features.Minting;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler.Models;
using Nethereum.Web3;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts
{
    internal static class ContractService
    {
        private static readonly ConcurrentDictionary<string, Contract> ContractCache = new();

        public static async ValueTask<Contract> GetOrCreateContract(string name, string sourceCode)
        {
            return ContractCache.GetOrAdd(name, await CompileAndSaveContract(name, sourceCode));
        }

        private static async Task<Contract> CompileAndSaveContract(string name, string sourceCode)
        {
            var persistedContract = await ServiceContext.Database.GetContract(name);
            if (persistedContract is not null) return persistedContract;

            BeamableLogger.Log("Creating contract {contractName}", name);

            var compilerOutput = await Compile(sourceCode);
            var contractOutput = compilerOutput.Contracts.Contract.First().Value;
            var abi = contractOutput.GetAbi();
            var contractByteCode = contractOutput.GetBytecode();

            var rpcClient = new EthRpcClient(new Web3(ServiceContext.RealmAccount, Configuration.RPCEndpoint));
            var gas = await rpcClient.EstimateContractGasAsync(ServiceContext.RealmAccount, abi, contractByteCode, ServiceContext.RealmAccount.Address);
            var result = await rpcClient.DeployContractAsync(ServiceContext.RealmAccount, abi, contractByteCode, gas);

            var contract = new Contract
            {
                Name = name,
                PublicKey = result.ContractAddress
            };

            await SetBaseUri(contract);

            var insertSuccess = await ServiceContext.Database.TryInsertContract(contract);

            if (insertSuccess)
            {
                BeamableLogger.Log("Contract {contractName} created successfully. Address: {contractAddress}", name, result.ContractAddress);
                return contract;
            }

            BeamableLogger.LogWarning("Contract {contractName} already created, fetching again", name);
            return await GetOrCreateContract(name, sourceCode);
        }

        private static async Task SetBaseUri(Contract contract)
        {
            var uriString = await NtfExternalMetadataService.SaveExternalMetadata(new NftExternalMetadata());
            var uri = new Uri(uriString);
            // Remove the last segment
            var segments = uri.Segments.Take(uri.Segments.Length - 1);
            var baseUriString = $"{uri.Scheme}://{uri.Host}{string.Concat(segments)}";

            BeamableLogger.Log("Setting the base uri to {baseUri}", baseUriString);
            await ServiceContext.RpcClient.SendRequestAndWaitForReceiptAsync(contract.PublicKey, new ER1155SetUriFunctionMessage
            {
                NewUri = baseUriString
            });

            contract.BaseMetadataUri = baseUriString;
            ServiceContext.BaseMetadataUri = baseUriString;
        }

        private static async Task<SolidityCompilerOutput> Compile(string sourceCode)
        {
            var compilerInput = new SolidityCompilerInput(sourceCode, new[] { "abi", "evm.bytecode" });

            var compilerOutput = await Solc.Compile(compilerInput);

            if (compilerOutput.HasErrors)
            {
                BeamableLogger.LogError("Compile errors: {@compileErrors}", compilerOutput.Errors.Select(x => x.Message).ToList());
                throw new ContractCompilationException(compilerOutput.Errors);
            }

            return compilerOutput;
        }
    }
}