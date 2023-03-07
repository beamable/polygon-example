using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts
{
    internal class Erc721ContractInteractionService : IContractInteractionService
    {
        private readonly Web3 _web3;

        public Erc721ContractInteractionService(Web3 web3)
        {
            _web3 = web3;
        }
        
        public Task<TransactionReceipt> Mint(Contract contract, long amount, string ownerAddress, IDictionary<string, string> properties)
        {
            throw new System.NotImplementedException();
        }
    }
}