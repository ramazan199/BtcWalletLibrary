using System.Collections.Generic;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using BtcWalletLibrary.Services.Adapters;

namespace BtcWalletLibrary.Services
{
    internal class StorageService : IStorageService
    {
        private readonly ISecureStorageAdapter _secureStorage;

        public StorageService(ISecureStorageAdapter secureStorage)
        {
            _secureStorage = secureStorage;
        }

        public List<Transaction> GetTransactionsFromStorage()
        {
            var transactions = _secureStorage.ObjectStorage.
                LoadObject(typeof(List<Transaction>), "BitcoinTransactions") as List<Transaction>
                ?? new List<Transaction>();
            return transactions;
        }

        public int GetLastMainAddrIdxFromStorage()
        {
            return _secureStorage.Values.Get("lastNonEmptyMainAddrIdx", -1);
        }

        public int GetLastChangeAddrIdxFromStorage()
        {
            return _secureStorage.Values.Get("lastNonEmptyChangeAddrIdx", -1);
        }

        public void ClearStorage()
        {
            _secureStorage.ObjectStorage.DeleteObject(typeof(List<Transaction>), "BitcoinTransactions");
            _secureStorage.Values.Set("MyPassPhrase", "");
            _secureStorage.Values.Set("lastNonEmptyMainAddrIdx", -1);
            _secureStorage.Values.Set("lastNonEmptyChangeAddrIdx", -1);
        }

        public void StoreLastMainAddrIdx(uint idx)
        {
            _secureStorage.Values.Set("lastNonEmptyMainAddrIdx", idx);
        }

        public void StoreLastChangeAddrIdx(uint idx)
        {
            _secureStorage.Values.Set("lastNonEmptyChangeAddrIdx", idx);
        }

        public void StoreTransactions(List<Transaction> transactions)
        {
            _secureStorage.ObjectStorage.SaveObject(transactions, "BitcoinTransactions");
        }
    }
}
