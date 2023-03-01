using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler.Models;
using Beamable.Server;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;

namespace Beamable.Microservices.FederationMicroservice
{
	[Microservice("FederationMicroservice")]
	public class FederationMicroservice : Microservice
	{
		const string RPCEndpoint = "https://sepolia.infura.io/v3/0381878048f64d6c9ab3d0fc17b45a69";
		
		[ClientCallable]
		public async Task<string> DeployContract(string sourceCode, string privateKey)
		{
			var input = new SolidityCompilerInput(sourceCode, new[] { "abi", "evm.bytecode" });
			var output = await Solc.Compile(input);
			
			var account = new Account(privateKey);
			var web3 = new Web3(account, RPCEndpoint);
			if (output.HasErrors)
			{
				BeamableLogger.LogError("We have errors");
				throw new Exception("We have errors");
			}
			
			var contractOutput = output.Contracts.Contract.First().Value;

			var abi =contractOutput.GetAbi();
			var contractByteCode = contractOutput.GetBytecode();

			BeamableLogger.Log("Estimating gas");
			var gas = await web3.Eth.DeployContract.EstimateGasAsync(abi, contractByteCode, account.Address);
			BeamableLogger.Log("Gas is {g}", gas);
			BeamableLogger.Log("Sending contract");
			var result = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(abi, contractByteCode, account.Address, gas, CancellationToken.None);
			var responseString = JsonConvert.SerializeObject(result);
			BeamableLogger.Log("Response: {r}", responseString);

			return responseString;
		}
	}
}
