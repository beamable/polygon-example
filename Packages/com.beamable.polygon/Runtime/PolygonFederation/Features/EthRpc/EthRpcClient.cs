using System;
using System.Threading;
using System.Threading.Tasks;
using Assets.Beamable.Microservices.PolygonFederation.Features.EthRpc;
using Beamable.Common;
using Beamable.Microservices.PolygonFederation.Features.EthRpc.Exceptions;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.PolygonFederation.Features.EthRpc
{
    internal class EthRpcClient
    {
        private readonly Web3 _web3;

        public EthRpcClient(Web3 web3)
        {
            _web3 = web3;
        }

        public async Task<HexBigInteger> EstimateContractGasAsync(Account realmAccount, string abi, string contractByteCode)
        {
            using (new Measure("EstimateContractGasAsync"))
            {
                var gas = await _web3.Eth.DeployContract.EstimateGasAsync(abi, contractByteCode, realmAccount.Address);
                BeamableLogger.Log("Gas is {g}", gas);
                return gas;
            }
        }

        public async Task<TransactionReceipt> DeployContractAsync(Account realmAccount, string abi, string contractByteCode, HexBigInteger gas)
        {
            using (new Measure("DeployContractAsync"))
            {
                string transactionHash;
                try
                {
                    transactionHash = await _web3.Eth.DeployContract.SendRequestAsync(abi, contractByteCode, realmAccount.Address, gas);
                }
                catch (Exception ex)
                {
                    BeamableLogger.LogWarning("Resetting nonce due to error: {error}", ex.Message);
                    await _web3.TransactionManager.Account.NonceService.ResetNonceAsync();
                    throw;
                }

                BeamableLogger.Log("Transaction hash is {transactionHash}", transactionHash);
                var receipt = await FetchReceiptAsync(transactionHash);
                BeamableLogger.Log("Response: {@response}", receipt);
                if (!receipt.Succeeded()) throw new ContractDeployException("Contract deployment failed. Check microservice logs.");
                return receipt;
            }
        }

        public async Task<TransactionReceipt> SendTransactionAndWaitForReceiptAsync<TContractMessage>(string contractAddress, TContractMessage functionMessage = null)
            where TContractMessage : FunctionMessage, new()
        {
            using (new Measure("SendTransactionAndWaitForReceiptAsync"))
            {
                var handler = _web3.Eth.GetContractTransactionHandler<TContractMessage>();

                string transactionHash;
                try
                {
                    transactionHash = await handler.SendRequestAsync(contractAddress, functionMessage);
                }
                catch (Exception ex)
                {
                    BeamableLogger.LogWarning("Resetting nonce due to error: {error}", ex.Message);
                    await _web3.TransactionManager.Account.NonceService.ResetNonceAsync();
                    throw;
                }

                BeamableLogger.Log("Transaction hash is {transactionHash}", transactionHash);
                var receipt = await FetchReceiptAsync(transactionHash);
                BeamableLogger.Log("Response: {@response}", receipt);
                if (!receipt.Succeeded()) throw new ContractException("Transaction failed. Check microservice logs.");
                return receipt;
            }
        }

        public async Task<TFunctionOutput> SendFunctionQueryAsync<TContractMessage, TFunctionOutput>(string contractAddress, TContractMessage functionMessage = null)
            where TContractMessage : FunctionMessage, new()
            where TFunctionOutput : IFunctionOutputDTO, new()
        {
            using (new Measure("SendFunctionQueryAsync"))
            {
                var handler = _web3.Eth.GetContractQueryHandler<TContractMessage>();

                try
                {
                    return await handler.QueryDeserializingToObjectAsync<TFunctionOutput>(functionMessage, contractAddress);
                }
                catch (Exception ex)
                {
                    throw new ContractException(ex.Message);
                }
            }
        }

        private async Task<TransactionReceipt> FetchReceiptAsync(string transactionHash)
        {
            using (new Measure("FetchReceiptAsync"))
            {
                var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

                var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(Configuration.ReceiptPoolTimeoutMs));
                while (receipt == null)
                {
                    tokenSource.Token.ThrowIfCancellationRequested();
                    await Task.Delay(Configuration.ReceiptPoolIntervalMs, tokenSource.Token);
                    receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                }

                return receipt;
            }
        }
    }
}