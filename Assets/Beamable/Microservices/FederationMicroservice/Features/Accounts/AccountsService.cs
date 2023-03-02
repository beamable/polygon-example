using System.Threading.Tasks;
using Beamable.Microservices.FederationMicroservice.Features.Accounts.Storage;
using Beamable.Microservices.FederationMicroservice.Features.Accounts.Storage.Models;
using MongoDB.Driver;
using Nethereum.KeyStore;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.FederationMicroservice.Features.Accounts
{
    internal static class AccountsService
    {
        public static readonly KeyStoreScryptService KeystoreService = new();

        public static async Task<Account> GetOrCreateAccount(IMongoDatabase db, string accountName)
        {
            var maybeExistingAccount = (await db.GetValutByName(accountName))?.ToAccount();
            if (maybeExistingAccount is not null)
            {
                return maybeExistingAccount;
            }

            var ecKey = EthECKey.GenerateKey();
            var privateKeyBytes = ecKey.GetPrivateKeyAsBytes();
            var newAccount = new Account(privateKeyBytes);

            await db.TryInsertValut(new Vault
            {
                Name = accountName,
                Value = KeystoreService.EncryptAndGenerateKeyStore(Configuration.RealmSecret, privateKeyBytes, newAccount.Address)
            });

            return newAccount;
        }
    }
}