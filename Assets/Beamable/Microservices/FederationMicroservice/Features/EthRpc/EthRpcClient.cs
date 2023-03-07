using System.Threading;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.FederationMicroservice.Features.EthRpc.Exceptions;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;

namespace Beamable.Microservices.FederationMicroservice.Features.EthRpc
{
    internal class EthRpcClient
    {
        private readonly Web3 _web3;

        public EthRpcClient(Web3 web3)
        {
            _web3 = web3;
        }
        
        public async Task<HexBigInteger> EstimateContractGasAsync(Account realmAccount, string abi, string contractByteCode, string from, params object[] values)
        {
            BeamableLogger.Log("Estimating gas");
            var gas = await _web3.Eth.DeployContract.EstimateGasAsync(abi, contractByteCode, realmAccount.Address);
            BeamableLogger.Log("Gas is {g}", gas);
            return gas;
        }

        public async Task<TransactionReceipt> DeployContractAsync(Account realmAccount, string abi, string contractByteCode, HexBigInteger gas)
        {
            BeamableLogger.Log("Sending contract");
            var result = await _web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(abi, contractByteCode,
                realmAccount.Address, gas, CancellationToken.None);
            
            BeamableLogger.Log("Response: {@response}", result);
            
            if (!result.Succeeded())
            {
                throw new ContractDeployException("Contract deployment failed. Check microservice logs.");
            }
            
            return result;
        }

        public async Task<TransactionReceipt> SendRequestAndWaitForReceiptAsync<TContractMessage>(string contractAddress, TContractMessage functionMessage = null, CancellationTokenSource tokenSource = null) where TContractMessage : FunctionMessage, new()
        {
            BeamableLogger.Log("Sending contract transaction");
            var handler = _web3.Eth.GetContractTransactionHandler<TContractMessage>();
            var result =  await handler.SendRequestAndWaitForReceiptAsync(contractAddress, functionMessage);
            
            BeamableLogger.Log("Response: {@response}", result);
            
            if (!result.Succeeded())
            {
                throw new TransactionFailedException("Transaction failed. Check microservice logs.");
            }
            
            return result;
        }
    }
}