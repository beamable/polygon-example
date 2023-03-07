using System.Threading.Tasks;
using Beamable.Common;
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
        private const string RealmWalletName = "default-wallet";
        
        private static Account _cachedRealmAccount;
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

            var insertSuccessful = await db.TryInsertValut(new Vault
            {
                Name = accountName,
                Value = KeystoreService.EncryptAndGenerateKeyStore(Configuration.RealmSecret, privateKeyBytes, newAccount.Address)
            });

            if (insertSuccessful)
            {
                BeamableLogger.Log("Saved account {accountName} -> {accountAddress}", accountName, newAccount.Address);
                return newAccount;
            }

            BeamableLogger.LogWarning("Account already created, fetching again");
            return await GetOrCreateAccount(db, accountName);
        }

        public static async ValueTask<Account> GetOrCreateRealmAccount(IMongoDatabase db)
        {
            return _cachedRealmAccount ??= await GetOrCreateAccount(db, RealmWalletName);
        }
    }
}