using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Beamable.Server.Api.RealmConfig;

namespace Beamable.Microservices.FederationMicroservice
{
    internal static class Configuration
    {
        private const string ConfigurationNamespace = "federation_polygon";
        private const string DefaultERC1155Path = "federationmicroservice/Beamable.Microservice.FederationMicroservice/Features/Contracts/SourceCode/DefaultERC1155.sol";

        public static readonly string RealmSecret = Environment.GetEnvironmentVariable("SECRET");

        public static RealmConfig RealmConfig { get; internal set; }

        public static string RPCEndpoint => GetValue(nameof(RPCEndpoint), "https://rpc-mumbai.maticvigil.com/v1/5e502a11ee9bdc54143ac84efdb16b0c47bbfd8c");
        public static string DefaultContractSource => GetValue(nameof(DefaultContractSource), File.ReadAllText(DefaultERC1155Path));
        public static string DefaultContractName => GetValue(nameof(DefaultContractName), "default");
        public static bool AllowManagedAccounts => GetValue(nameof(AllowManagedAccounts), false);
        public static int AuthenticationChallengeTtlSec => GetValue(nameof(AuthenticationChallengeTtlSec), 600);

        private static T GetValue<T>(string key, T defaultValue) where T : IConvertible
        {
            var namespaceConfig = RealmConfig.GetValueOrDefault(ConfigurationNamespace) ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
            var value = namespaceConfig.GetValueOrDefault(key);
            if (value is null)
                return defaultValue;
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}