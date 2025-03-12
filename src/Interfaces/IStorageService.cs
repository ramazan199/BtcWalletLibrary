using System.Collections.Generic;
using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Interfaces
{
    internal interface IStorageService
    {
        void ClearStorage();
        int GetLastChangeAddrIdxFromStorage();
        int GetLastMainAddrIdxFromStorage();
        List<Transaction> GetTransactionsFromStorage();
        void StoreLastChangeAddrIdx(uint idx);
        void StoreLastMainAddrIdx(uint idx);
        void StoreTransactions(List<Transaction> transactions);
    }
}