using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.FederationMicroservice.SolidityCompiler.Models;
using Newtonsoft.Json;

namespace Beamable.Microservices.FederationMicroservice.SolidityCompiler
{
    public static class Solc
    {
        private const string SolcDirectory = "federationmicroservice/Beamable.Microservice.FederationMicroservice/SolidityCompiler/Solc";
        private const string WindowsExecutable = "solc-windows-amd64-v0.8.19+commit.7dd6d404.exe";
        private const string LinuxExecutable = "solc-linux-amd64-v0.8.19+commit.7dd6d404";

        public static async Task<SolidityCompilerOutput> Compile(SolidityCompilerInput input)
        {
            var inputText = JsonConvert.SerializeObject(input);
            var inputFilePath = Path.GetTempFileName();

            await File.WriteAllTextAsync(inputFilePath, inputText);

            try
            {
                using var process = new Process();
                process.StartInfo =
                    new ProcessStartInfo(GetExecutable(), $"--standard-json {inputFilePath}")
                    {
                        RedirectStandardOutput = true,
                        WorkingDirectory = SolcDirectory
                    };
                process.Start();

                var reader = process.StandardOutput;
                var outputText = await reader.ReadToEndAsync();

                var output = JsonConvert.DeserializeObject<SolidityCompilerOutput>(outputText);
                process.WaitForExit(5000);

                return output!;
            }
            catch (Exception e)
            {
                BeamableLogger.LogError(e);
                throw;
            }
            finally
            {
                File.Delete(inputFilePath);
            }
        }

        private static string GetExecutable()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return Path.Combine(SolcDirectory, LinuxExecutable);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Path.Combine(SolcDirectory, WindowsExecutable);

            throw new NotImplementedException(
                $"{nameof(SolidityCompiler)} is not implemented for {RuntimeInformation.OSDescription}");
        }
    }
}