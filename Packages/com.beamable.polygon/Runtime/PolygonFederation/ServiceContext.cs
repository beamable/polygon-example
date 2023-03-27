using System;
using Beamable.Common.Api;
using Beamable.Microservices.PolygonFederation.Features.Contracts.Storage.Models;
using Beamable.Microservices.PolygonFederation.Features.EthRpc;
using MongoDB.Driver;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.PolygonFederation
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