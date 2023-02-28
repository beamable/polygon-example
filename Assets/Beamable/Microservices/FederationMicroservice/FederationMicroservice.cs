using System.Threading.Tasks;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler.Models;
using Beamable.Server;
using Newtonsoft.Json;

namespace Beamable.Microservices.FederationMicroservice
{
	[Microservice("FederationMicroservice")]
	public class FederationMicroservice : Microservice
	{
		[ClientCallable]
		public async Task<string> Compile(string sourceCode)
		{
			var input = new SolidityCompilerInput(sourceCode, new[] { "abi", "evm.bytecode" });
			var output = await Solc.Compile(input);
			return JsonConvert.SerializeObject(output);
		}
	}
}
