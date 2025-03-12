using NBitcoin;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BtcWalletLibrary.Utilities
{
    internal static class BitcoinHelper
    {
        /// <summary>
        /// reverts given script hash (ElectrumX server accepts reverted hashes)
        /// </summary>
        /// <param name="script">script hash</param>
        /// <returns></returns>
        public static string RevertScriptHash(string script)
        {
            var cArray = script.ToCharArray();
            var reverse = string.Empty;
            for (var i = cArray.Length - 1; i > -1; i -= 2)
            {
                reverse += cArray[i - 1];
                reverse += cArray[i];
            }
            return reverse;
        }

        /// <summary>
        /// Converts btc amount in string to Money format.
        /// </summary>
        /// <param name="value">amount</param>
        /// <returns></returns>
        public static Money ParseBtcString(string value)
        {
            if (!decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                Console.WriteLine("Wrong btc amount format.");
            return new Money(amount, MoneyUnit.BTC);
        }

        public static Dictionary<uint256, List<BalanceOperation>> GetOperationsPerTransactions(Dictionary<BitcoinAddress, List<BalanceOperation>> operationsPerAddresses)
        {
            // 1. Get all the unique operations
            var opSet = new HashSet<BalanceOperation>();
            foreach (var elem in operationsPerAddresses)
                foreach (var op in elem.Value)
                    opSet.Add(op);

            // 2. Get all operations, grouped by transactions
            var operationsPerTransactions = new Dictionary<uint256, List<BalanceOperation>>();
            foreach (var op in opSet)
            {
                var txId = op.TransactionId;
                if (operationsPerTransactions.TryGetValue(txId, out var ol))
                {
                    ol.Add(op);
                    operationsPerTransactions[txId] = ol;
                }
                else operationsPerTransactions.Add(txId, new List<BalanceOperation> { op });
            }
            return operationsPerTransactions;
        }

        public static void GetBalances(IEnumerable<AddressHistoryRecord> addressHistoryRecords, out Money confirmedBalance, out Money unconfirmedBalance)
        {
            confirmedBalance = Money.Zero;
            unconfirmedBalance = Money.Zero;
            foreach (var record in addressHistoryRecords)
                if (record.Confirmed) confirmedBalance += record.Amount;
                else unconfirmedBalance += record.Amount;
        }
    }

    internal class AddressHistoryRecord
    {
        public  BalanceOperation Operation { get; }
        public  BitcoinAddress Address { get; }
        public AddressHistoryRecord(BitcoinAddress address, BalanceOperation operation)
        {
            Address = address;
            Operation = operation;
        }

        public Money Amount
        {
            get
            {
                var amount = (from Coin coin in Operation.ReceivedCoins
                              let address = coin.GetScriptCode().GetDestinationAddress(Address.Network)
                              where address == Address
                              select coin.Amount).Sum();
                return (from Coin coin in Operation.SpentCoins
                        let address =
                            coin.GetScriptCode().GetDestinationAddress(Address.Network)
                        where address == Address
                        select coin)
                    .Aggregate(amount, (current, coin) => current - coin.Amount);
            }
        }
        public DateTimeOffset FirstSeen => Operation.FirstSeen;
        public bool Confirmed => Operation.Confirmations > 0;
        public uint256 TransactionId => Operation.TransactionId;
    }
}