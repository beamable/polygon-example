using System.Threading.Tasks;
using Beamable.Microservices.PolygonFederation.Features.Transactions.Exceptions;
using Beamable.Microservices.PolygonFederation.Features.Transactions.Storage;

namespace Beamable.Microservices.PolygonFederation.Features.Transactions
{
    internal static class TransactionManager
    {
        public static async Task SaveTransaction(string transactionId)
        {
            var isSuccess = await ServiceContext.Database.TryInsertTransaction(transactionId);
            if (!isSuccess)
            {
                throw new TransactionException($"Transaction {transactionId} already processed or in-progress");
            }
        }

        public static async Task ClearTransaction(string transactionId)
        {
            await ServiceContext.Database.DeleteTransaction(transactionId);
        }
    }
}