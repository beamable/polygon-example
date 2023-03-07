using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Functions.Messages;
using Beamable.Microservices.FederationMicroservice.Features.EthRpc;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts
{
    internal class Erc20ContractInteractionService : IContractInteractionService
    {
        private readonly EthRpcClient _rpcClient;

        public Erc20ContractInteractionService(Web3 web3)
        {
            _rpcClient = new EthRpcClient(web3);
        }
        
        public async Task<TransactionReceipt> Mint(Contract contract, long amount, string ownerAddress, IDictionary<string, string> properties)
        {
            var receipt =  await _rpcClient.SendRequestAndWaitForReceiptAsync(contract.PublicKey, new ERC20MintFunctionMessage
            {
                To = ownerAddress,
                Amount = Web3.Convert.ToWei(amount)
            });

            return receipt;
        }
    }
}