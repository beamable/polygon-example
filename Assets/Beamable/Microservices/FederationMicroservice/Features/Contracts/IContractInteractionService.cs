using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts
{
    public interface IContractInteractionService
    {
        Task<TransactionReceipt> Mint(Contract contract, long amount, string ownerAddress, IDictionary<string, string> properties);
    }
}