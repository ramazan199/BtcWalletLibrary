using System.Collections.Generic;
using System.Threading.Tasks;
using BtcWalletLibrary.Models;
using Transaction = NBitcoin.Transaction;

namespace BtcWalletLibrary.Interfaces
{
    internal interface ITxInputDetailsService
    {
        Task<List<TransactionInput>> GetTransactionInputDetails(Transaction transaction);
        List<TransactionOutput> GetTransactionOutputDetails(Transaction transaction);
    }
}