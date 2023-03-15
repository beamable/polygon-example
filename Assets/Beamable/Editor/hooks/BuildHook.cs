using Beamable.Common.Dependencies;
using Beamable.Microservices.FederationMicroservice;
using Beamable.Server.Editor;

namespace Beamable.Editor.hooks
{
    public class FederationMicroserviceBuildHook : IMicroserviceBuildHook<FederationMicroservice>
    {
        const string SourceBasePath = "Assets/Beamable/Microservices/FederationMicroservice";
        
        public void Execute(IMicroserviceBuildContext ctx)
        {
            ctx.AddDirectory($"{SourceBasePath}/Solidity", "Solidity");
        }
    }
    
    [BeamContextSystem]
    public class Registrations
    {
        [RegisterBeamableDependencies(-1, RegistrationOrigin.EDITOR)]
        public static void Register(IDependencyBuilder builder)
        {
            builder.AddSingleton<IMicroserviceBuildHook<FederationMicroservice>, FederationMicroserviceBuildHook>();
        }
    }
}
