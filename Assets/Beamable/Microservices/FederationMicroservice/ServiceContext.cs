using MongoDB.Driver;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.FederationMicroservice
{
    internal static class ServiceContext
    {
        public static IMongoDatabase Database;
        public static Account RealmAccount;
    }
}