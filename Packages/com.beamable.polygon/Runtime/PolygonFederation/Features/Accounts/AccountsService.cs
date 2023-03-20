using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Microservices.PolygonFederation.Features.Accounts.Storage;
using Beamable.Microservices.PolygonFederation.Features.Accounts.Storage.Models;
using Nethereum.KeyStore;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;

namespace Beamable.Microservices.PolygonFederation.Features.Accounts
{
    internal static class AccountsService
    {
        private const string RealmAccountName = "default-account";

        private static Account _cachedRealmAccount;
        public static readonly KeyStoreScryptService KeystoreService = new();

        public static async Task<Account> GetOrCreateAccount(string accountName)
        {
            var account = await GetAccount(accountName);
            if (account is null)
            {
                account = await CreateAccount(accountName);
                if (account is null)
                {
                    BeamableLogger.LogWarning("Account already created, fetching again");
                    return await GetOrCreateAccount(accountName);
                }
                BeamableLogger.Log("Saved account {accountName} -> {accountAddress}", accountName, account.Address);
            }
            return account;
        }

        public static async ValueTask<Account> GetOrCreateRealmAccount()
        {
            var account = await GetAccount(RealmAccountName);
            if (account is null)
            {
                account = await CreateAccount(RealmAccountName);
                if (account is null)
                {
                    BeamableLogger.LogWarning("Account already created, fetching again");
                    return await GetOrCreateAccount(RealmAccountName);
                }
                BeamableLogger.Log("Saved account {accountName} -> {accountAddress}", RealmAccountName, account.Address);
                BeamableLogger.LogWarning("Please add some gas money to your account {accountAddress} to be able to pay for fees.", account.Address);
            }
            return account;
        }

        private static async Task<Account> GetAccount(string accountName)
        {
            var db = ServiceContext.Database;
            return (await db.GetValutByName(accountName))?.ToAccount();
        }

        private static async Task<Account> CreateAccount(string accountName)
        {
            var db = ServiceContext.Database;
            
            var ecKey = EthECKey.GenerateKey();
            var privateKeyBytes = ecKey.GetPrivateKeyAsBytes();
            var newAccount = new Account(privateKeyBytes);

            return (await db.TryInsertValut(new Vault
            {
                Name = accountName,
                Value = KeystoreService.EncryptAndGenerateKeyStore(Configuration.RealmSecret, privateKeyBytes, newAccount.Address)
            })) ? newAccount : null;
        }
    }
}