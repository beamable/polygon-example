using Beamable.Common.Api;
using Beamable.Microservices.FederationMicroservice.Features.Contracts.Storage.Models;
using Beamable.Microservices.FederationMicroservice.Features.EthRpc;
using MongoDB.Driver;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.FederationMicroservice
{
    internal static class ServiceContext
    {
        public static IMongoDatabase Database;
        public static Account RealmAccount;
        public static Contract DefaultContract;
        public static EthRpcClient RpcClient;
        public static IBeamableRequester Requester;
    }
}