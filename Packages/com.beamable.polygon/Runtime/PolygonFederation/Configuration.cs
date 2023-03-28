using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Beamable.Server;
using Beamable.Server.Api.RealmConfig;

namespace Beamable.Microservices.PolygonFederation
{
    internal static class Configuration
    {
        private const string ConfigurationNamespace = "federation_polygon";

        public static readonly string RealmSecret = Environment.GetEnvironmentVariable("SECRET");

        public static RealmConfig RealmConfig { get; internal set; }

        public static string RPCEndpoint => GetValue(nameof(RPCEndpoint), "");
        public static bool AllowManagedAccounts => GetValue(nameof(AllowManagedAccounts), true);
        public static int AuthenticationChallengeTtlSec => GetValue(nameof(AuthenticationChallengeTtlSec), 600);
        public static int ReceiptPoolIntervalMs => GetValue(nameof(ReceiptPoolIntervalMs), 200);
        public static int ReceiptPoolTimeoutMs => GetValue(nameof(ReceiptPoolTimeoutMs), 20000);

        private static T GetValue<T>(string key, T defaultValue) where T : IConvertible
        {
            var namespaceConfig = RealmConfig.GetValueOrDefault(ConfigurationNamespace) ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
            var value = namespaceConfig.GetValueOrDefault(key);
            if (value is null)
                return defaultValue;
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }

    class ConfigurationException : MicroserviceException
    {
        public ConfigurationException(string message) : base((int)HttpStatusCode.BadRequest, "ConfigurationError", message)
        {
        }
    }
}