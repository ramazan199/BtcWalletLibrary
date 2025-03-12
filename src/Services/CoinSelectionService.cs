using BtcWalletLibrary.Interfaces;
using NBitcoin;
using System.Collections.Generic;
using System.Linq;

namespace BtcWalletLibrary.Services
{
    /// <summary>
    /// Service to select coins for transaction
    /// </summary>
    internal class CoinSelectionService : ICoinSelectionService
    {
        private readonly ITxFeeService _txFeeService;
        private readonly ISigningKeyService _signingKeyService;

        /// <summary>
        /// Constructor for CoinSelectionService
        /// </summary>
        /// <param name="txFeeService">Service to calculate transaction fees</param>
        /// <param name="signingKeyService">Service to manage signing keys</param>
        public CoinSelectionService(ITxFeeService txFeeService, ISigningKeyService signingKeyService)
        {
            _txFeeService = txFeeService;
            _signingKeyService = signingKeyService;
        }

        /// <summary>
        /// Automatically selects coins to spend when no fee is manually selected, ensuring enough funds for the amount and calculated fee.
        /// </summary>
        /// <param name="coinsToSpend">Output parameter, list of coins selected to spend</param>
        /// <param name="amount">The amount to send</param>
        /// <param name="fee">Output parameter, the calculated fee for the transaction</param>
        /// <param name="unspentCoins">List of available unspent coins</param>
        /// <param name="destinationAddress">The destination address for the transaction</param>
        /// <returns>True if coins are successfully selected and fee is calculated, false otherwise</returns>
        public bool AutoSelectCoinsWithNoFeeSelected(out List<Coin> coinsToSpend, Money amount, out Money fee,
            List<Coin> unspentCoins,
            IDestination destinationAddress)
        {
            coinsToSpend = new List<Coin>();
            fee = Money.Zero;
            foreach (var coin in unspentCoins.OrderByDescending(x => x.Amount))
            {
                coinsToSpend.Add(coin);
                // if it doesn't reach amount, continue adding next coin
                if (coinsToSpend.Sum(x => x.Amount) < amount + fee) continue;
                //if reaches calculate fee and check again
                var signingKeys = new List<Key>(_signingKeyService.PrepareSigningKeys(coinsToSpend));
                fee = _txFeeService.CalculateTransactionFee(coinsToSpend, signingKeys, destinationAddress, amount);
                if (coinsToSpend.Sum(x => x.Amount) < amount + fee) continue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Automatically selects coins to spend when a fee is manually selected, ensuring enough funds for the amount and the specified fee.
        /// </summary>
        /// <param name="coinsToSpend">Output parameter, list of coins selected to spend</param>
        /// <param name="amount">The amount to send</param>
        /// <param name="fee">The manually selected fee for the transaction</param>
        /// <param name="unspentCoins">List of available unspent coins</param>
        /// <returns>True if coins are successfully selected to cover the amount and fee, false otherwise</returns>
        public bool AutoSelectCoinsWithFeeSeleceted(out List<Coin> coinsToSpend, Money amount, Money fee,
            List<Coin> unspentCoins)
        {
            coinsToSpend = new List<Coin>();
            var haveEnough = false;
            foreach (var coin in unspentCoins.OrderByDescending(x => x.Amount))
            {
                coinsToSpend.Add(coin);
                // if it doesn't reach amount, continue adding next coin
                if (coinsToSpend.Sum(x => x.Amount) <= amount + fee) continue;
                haveEnough = true;
                break;
            }

            return haveEnough;
        }
    }
}