using System;
using MongoDB.Bson.Serialization.Attributes;
using Nethereum.KeyStore.Model;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.PolygonFederation.Features.Accounts.Storage.Models
{
    public record Vault
    {
        [BsonElement("_id")]
        public string Name { get; set; }
        public KeyStore<ScryptParams> Value { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public Account ToAccount()
        {
            var decryptedKeystore = AccountsService.KeystoreService.DecryptKeyStore(Configuration.RealmSecret, Value);
            return new Account(decryptedKeystore);
        }
    }
}