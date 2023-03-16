using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.FederationMicroservice.Features.SolcWrapper.Models;
using Newtonsoft.Json;

namespace Beamable.Microservices.FederationMicroservice.Features.SolcWrapper
{
    public static class Solc
    {
        private static bool _initialized;
        
        private const string SolcDirectory = "Solidity/Solc";
        private const string Executable = "solc-linux-amd64-v0.8.19+commit.7dd6d404";
        public const int ProcessTimeoutMs = 5000;

        public static async Task<SolidityCompilerOutput> Compile(SolidityCompilerInput input)
        {
            await Initialize();
            
            BeamableLogger.Log("Compiling with solc");

            var inputText = JsonConvert.SerializeObject(input);
            var inputFilePath = Path.GetTempFileName();

            await File.WriteAllTextAsync(inputFilePath, inputText);

            try
            {
                var outputText = await Execute(Path.Combine(SolcDirectory, Executable), $"--standard-json {inputFilePath}", SolcDirectory);
                var output = JsonConvert.DeserializeObject<SolidityCompilerOutput>(outputText);
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

        private static async ValueTask Initialize()
        {
            if (!_initialized)
            {
                BeamableLogger.Log("Adding gcompat compatibility layer package");
                var result = await ExecuteBash("apk add gcompat");
                _initialized = true;
            }
        }

        private static async Task<string> ExecuteBash(string command, string workingDirectory = null)
        {
            return await Execute("/bin/sh", $"-c \"{command}\"", workingDirectory);
        }
        
        private static async Task<string> Execute(string program, string args, string workingDirectory = null)
        {
            using var process = new Process();
            process.StartInfo =
                new ProcessStartInfo(program, args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
            
            if (workingDirectory is not null)
                process.StartInfo.WorkingDirectory = workingDirectory;
            
            process.Start();
            
            var outputText = await process.StandardOutput.ReadToEndAsync();
            var outputError = await process.StandardError.ReadToEndAsync();
            var output = $"{outputText}{outputError}";
            
            process.WaitForExit(ProcessTimeoutMs);

            BeamableLogger.Log("Process output: {processOutput}", output);
            return output;
        }
    }
}