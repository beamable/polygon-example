using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.PolygonFederation.Features.SolcWrapper.Exceptions;
using Beamable.Microservices.PolygonFederation.Features.SolcWrapper.Models;
using Newtonsoft.Json;

namespace Beamable.Microservices.PolygonFederation.Features.SolcWrapper
{
    public static class Solc
    {
        private static bool _initialized;
        
        private const string SolcDirectory = "Solidity/Solc";
        private const string ExecutableAmd64 = "solc-linux-amd64-v0.8.19";
        private const string ExecutableArm64 = "solc-linux-arm64-v0.8.19";
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
                var outputText = await Execute(GetExecutable(), $"--standard-json {inputFilePath}", SolcDirectory);
                var output = JsonConvert.DeserializeObject<SolidityCompilerOutput>(outputText);
                return output!;
            }
            catch (Exception e)
            {
                BeamableLogger.LogError(e);
                throw new SolcException(e.Message);
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
                await ExecuteShell("command -v apk >/dev/null 2>&1 && apk add gcompat");
                
                BeamableLogger.Log("Changing permissions of Solidity/Solc");
                await ExecuteShell("chmod -R 755 Solidity/Solc");
                _initialized = true;
            }
        }

        private static async Task<string> ExecuteShell(string command, string workingDirectory = null)
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
            
            process.WaitForExit(ProcessTimeoutMs);

            if (!string.IsNullOrEmpty(outputError))
            {
                BeamableLogger.LogError("Process error: {processOutput}", outputError);
                throw new SolcException(outputError);
            }

            BeamableLogger.Log("Process output: {processOutput}", outputText);
            return outputText;
        }

        private static string GetExecutable()
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => Path.Combine(SolcDirectory, ExecutableAmd64),
                Architecture.Arm64 => Path.Combine(SolcDirectory, ExecutableArm64),
                _ => throw new SolcException($"{RuntimeInformation.ProcessArchitecture} is not supported")
            };
        }
    }
}