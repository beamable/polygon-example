using Beamable.Common.Dependencies;
using Beamable.Microservices.PolygonFederation;
using Beamable.Server.Editor;

namespace Beamable.Editor.Polygon.Hooks
{
    public class PolygonFederationBuildHook : IMicroserviceBuildHook<PolygonFederation>
    {
        const string SourceBasePath = "Packages/com.beamable.polygon/Runtime/PolygonFederation";
        
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
            builder.AddSingleton<IMicroserviceBuildHook<PolygonFederation>, PolygonFederationBuildHook>();
        }
    }
}
