using Beamable.Common.Content.Contracts;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Exceptions;
using Nethereum.Web3;

namespace Beamable.Microservices.FederationMicroservice.Features.Contracts
{
    internal static class ContractInteractionServiceFactory
    {
        public static IContractInteractionService GetContractInteractionService(this ContractType contractType, Web3 web3)
        {
            return contractType switch
            {
                ContractType.ERC20 => new Erc20ContractInteractionService(web3),
                ContractType.ERC721 => new Erc721ContractInteractionService(web3),
                _ => throw new UnsupportedContractException($"Content type {contractType} is not supported")
            };
        }
    }
}