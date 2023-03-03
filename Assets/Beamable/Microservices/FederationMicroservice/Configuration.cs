﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Beamable.Server.Api.RealmConfig;

namespace Beamable.Microservices.FederationMicroservice
{
    internal static class Configuration
    {
        private const string ConfigurationNamespace = "federation_polygon";

        public static RealmConfig RealmConfig { get; internal set; }
        
        public static readonly string RealmSecret = Environment.GetEnvironmentVariable("SECRET");
        
        public static string RPCEndpoint => GetValue(nameof(RPCEndpoint), "https://rpc-mumbai.maticvigil.com/v1/5e502a11ee9bdc54143ac84efdb16b0c47bbfd8c"); 
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