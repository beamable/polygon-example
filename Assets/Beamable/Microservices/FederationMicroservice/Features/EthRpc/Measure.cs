using System;
using System.Diagnostics;
using Beamable.Common;

namespace Assets.Beamable.Microservices.FederationMicroservice.Features.EthRpc
{
    internal class Measure : IDisposable
    {
        private readonly string operationName;
        private readonly Stopwatch watch;

        public Measure(string operationName)
        {
            this.operationName = operationName;
            BeamableLogger.Log("Starting {operation}", operationName);
            watch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            watch.Stop();
            BeamableLogger.Log("Done executing {operation} in {elapsedSec} sec", operationName, watch.Elapsed.TotalSeconds.ToString("0.####"));
        }
    }
}