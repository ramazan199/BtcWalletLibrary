using NBitcoin;

namespace BtcWalletLibrary.Models
{
    /// <summary>
    /// Represents a Bitcoin Coin (UTXO) within the wallet library, extending the base <see cref="NBitcoin.Coin"/> class with wallet-specific information.
    /// This class inherits from <see cref="NBitcoin.Coin"/> and adds a <see cref="Confirmed"/> property to track the confirmation status of the UTXO.
    /// It is used to represent spendable coins within the wallet, incorporating both the cryptographic details from NBitcoin and the wallet's internal tracking of confirmation status.
    /// </summary>
    public class Coin : NBitcoin.Coin
    {
        /// <summary>
        /// Gets a value indicating whether the coin (UTXO) is confirmed on the blockchain.
        /// True if the transaction that created this coin has reached a sufficient number of confirmations, false otherwise.
        /// This property is specific to the wallet's management of UTXOs and is not part of the base <see cref="NBitcoin.Coin"/> class.
        /// </summary>
        public bool Confirmed { get; }

        public Coin(NBitcoin.Transaction fromTx, uint fromOutputIndex, bool confirmed)
        : base(fromTx, fromOutputIndex)
        {
            Confirmed = confirmed;
        }
    }
}