using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Common;
using Nethereum.Web3;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting
{
    internal static class MintingService
    {
        public static async Task Mint(string contractAddress, long amount, string toAddress, IDictionary<string, string> properties = null)
        {
            BeamableLogger.Log("Minting {amount} of {contract} to {owner}", amount, contractAddress, toAddress);

            var web3 = new Web3(ServiceContext.RealmAccount, Configuration.RPCEndpoint);

            // TODO: mint
        }
    }
}