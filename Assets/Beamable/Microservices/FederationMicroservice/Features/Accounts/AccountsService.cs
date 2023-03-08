using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.FederationMicroservice.Features.Accounts.Storage;
using Beamable.Microservices.FederationMicroservice.Features.Accounts.Storage.Models;
using Nethereum.KeyStore;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.FederationMicroservice.Features.Accounts
{
    internal static class AccountsService
    {
        private const string RealmAccountName = "default-account";

        private static Account _cachedRealmAccount;
        public static readonly KeyStoreScryptService KeystoreService = new();

        public static async Task<Account> GetOrCreateAccount(string accountName)
        {
            var db = ServiceContext.Database;
            var maybeExistingAccount = (await db.GetValutByName(accountName))?.ToAccount();
            if (maybeExistingAccount is not null) return maybeExistingAccount;

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
                BeamableLogger.LogWarning("Please add some gas money to your account {accountAddress} to be able to pay for fees.", newAccount.Address);
                return newAccount;
            }

            BeamableLogger.LogWarning("Account already created, fetching again");
            return await GetOrCreateAccount(accountName);
        }

        public static async ValueTask<Account> GetOrCreateRealmAccount()
        {
            if (_cachedRealmAccount == null)
            {
                _cachedRealmAccount = await GetOrCreateAccount(RealmAccountName);
            }

            return _cachedRealmAccount;
        }
    }
}