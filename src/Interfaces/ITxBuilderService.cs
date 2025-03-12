using BtcWalletLibrary.Models;
using NBitcoin;
using System.Collections.Generic;
using Transaction = NBitcoin.Transaction;
using Coin = NBitcoin.Coin;
using BtcWalletLibrary.Exceptions;
namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
    /// Interface for services responsible for building Bitcoin transactions.
    /// This service handles the logic of constructing a transaction, including coin selection, fee calculation, and output creation.
    /// </summary>
    public interface ITxBuilderService
    {
        /// <summary>
        /// Attempts to build a Bitcoin transaction.
        /// </summary>
        /// <param name="amount">The amount to send in the transaction.</param>
        /// <param name="destinationAddr">The destination Bitcoin address for the transaction.</param>
        /// <param name="tx">When this method returns successfully, contains the built <see cref="Transaction"/>.</param>
        /// <param name="calculatedFee">When this method returns successfully, contains the calculated transaction fee.</param>
        /// <param name="autoSelectedCoins">When this method returns successfully, contains the list of coins that were automatically selected for the transaction if no specific coins were provided.</param>
        /// <param name="txBuildErrorCode">When this method returns false, contains the <see cref="TransactionBuildErrorCode"/> indicating the reason for transaction building failure.</param>
        /// <param name="selectedUnspentCoins">Optional. A list of specific <see cref="UnspentCoin"/> to use as inputs for the transaction. If null, coins will be automatically selected.</param>
        /// <param name="customFee">Optional. A custom fee to use for the transaction. If null, the fee will be automatically calculated.</param>
        /// <returns>True if the transaction was built successfully, false otherwise. Check <paramref name="txBuildErrorCode"/> for details on failure.</returns>
        bool TryBuildTx(Money amount,
     string destinationAddr,
     out Transaction tx,
     out Money calculatedFee,
     out List<Coin> autoSelectedCoins,
     out TransactionBuildErrorCode txBuildErrorCode,
     List<UnspentCoin> selectedUnspentCoins = null,
     Money customFee = null);
    }
}