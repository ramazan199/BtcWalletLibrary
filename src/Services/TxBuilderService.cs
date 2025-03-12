using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using NBitcoin;
using System;
using System.Collections.Generic;
using Transaction = NBitcoin.Transaction;
using Coin = NBitcoin.Coin;
using System.Linq;
using BtcWalletLibrary.Exceptions;
namespace BtcWalletLibrary.Services
{
    internal class TxBuilderService : ITxBuilderService
    {
        private readonly IAddressService _btcAddressService;
        private readonly IBalanceService _balanceService;
        private readonly ICoinMapper _coinMapper;
        private readonly ISigningKeyService _signingKeyService;
        private readonly ITxFeeService _txFeeService;
        private readonly ICoinSelectionService _coinSelectionService;
        private readonly ITxValidator _txValidator;
        private readonly ICommonService _commonService;
        private readonly ILoggingService _logService;

        public TxBuilderService(IAddressService btcAddressService, ICoinMapper coinMapper, ISigningKeyService signingKeyService, ITxFeeService txFeeService, ICoinSelectionService coinSelectionService, IBalanceService balanceService, ITxValidator txValidator, ICommonService commonService, ILoggingService logService)
        {
            _btcAddressService = btcAddressService;
            _coinMapper = coinMapper;
            _signingKeyService = signingKeyService;
            _txFeeService = txFeeService;
            _coinSelectionService = coinSelectionService;
            _balanceService = balanceService;
            _txValidator = txValidator;
            _commonService = commonService;
            _logService = logService;
        }

        public bool TryBuildTx(Money amount,
            string destinationAddr,
            out Transaction tx,
            out Money calculatedFee,
            out List<Coin> autoSelectedCoins,
            out TransactionBuildErrorCode txBuildErrorCode,
            List<UnspentCoin> selectedUnspentCoins = null,
            Money customFee = null)
        {
            // Initialize outputs
            tx = null;
            calculatedFee = null;
            autoSelectedCoins = null;
            try
            {
                // Determine which overload to call based on provided parameters
                if (customFee != null && selectedUnspentCoins != null)
                {
                    // User provided both custom fee and selected coins
                    return TryBuildTx(amount, destinationAddr, selectedUnspentCoins, customFee, out tx,
                        out txBuildErrorCode);
                }

                if (customFee != null)
                {
                    // User provided custom fee but no selected coins
                    return TryBuildTx(amount, destinationAddr, customFee, out tx, out autoSelectedCoins,
                        out txBuildErrorCode);
                }

                if (selectedUnspentCoins != null)
                {
                    // User provided selected coins but no custom fee
                    return TryBuildTx(amount, destinationAddr, selectedUnspentCoins, out tx, out calculatedFee,
                        out txBuildErrorCode);
                }
                else
                {
                    // User provided neither custom fee nor selected coins
                    return TryBuildTx(amount, destinationAddr, out tx, out autoSelectedCoins, out calculatedFee,
                        out txBuildErrorCode);
                }
            }
            catch (Exception ex)
            {
                txBuildErrorCode = TransactionBuildErrorCode.TransactionBuildFailed;
                _logService.LogError(ex, $"Error building transaction");
                return false;
            }
        }

        // coins and fee selected by user
        private bool TryBuildTx(Money amount, string destinationAddr, List<UnspentCoin> selectedUnspentCoins,
            Money customFee, out Transaction tx, out TransactionBuildErrorCode txBuildErrorCode)
        {
            tx = null;
            // Early validation checks
            if (!_txValidator.ValidateAmount(amount, out txBuildErrorCode)) return false;
            if (!_txValidator.ValidateCustomFee(customFee, out txBuildErrorCode)) return false;
            if (!_txValidator.ValidateSelectedUnspentCoins(selectedUnspentCoins, amount, out txBuildErrorCode)) return false;
            if (!_txValidator.ValidateFundSufficiency(amount, customFee, selectedUnspentCoins, out txBuildErrorCode)) return false;
            if (!_txValidator.ValidateAddress(destinationAddr, out txBuildErrorCode, out var bitcoinDestinationAddr))
                return false;

            var nBitcoinSelectedUnspentCoins = _coinMapper.UnspentCoinsToNbitcoinCoin(selectedUnspentCoins);
            var signingKeys = _signingKeyService.PrepareSigningKeys(nBitcoinSelectedUnspentCoins);

            tx = BuildTransaction(amount, nBitcoinSelectedUnspentCoins, signingKeys, customFee, bitcoinDestinationAddr,
                out var builder);
            return VerifyBuiltTransaction(ref tx, out txBuildErrorCode, builder);
        }

        // coins are selected, fee not selected
        private bool TryBuildTx(Money amount, string destinationAddr, List<UnspentCoin> selectedUnspentCoins,
            out Transaction tx, out Money calculatedFee, out TransactionBuildErrorCode txBuildErrorCode)
        {
            tx = null;
            calculatedFee = Money.Zero;
            // Early validation checks
            if (!_txValidator.ValidateAmount(amount, out txBuildErrorCode)) return false;
            if (!_txValidator.ValidateSelectedUnspentCoins(selectedUnspentCoins, amount, out txBuildErrorCode)) return false;
            if (!_txValidator.ValidateAddress(destinationAddr, out txBuildErrorCode, out var bitcoinDestinationAddr))
                return false;

            var nBitcoinSelectedUnspentCoins = _coinMapper.UnspentCoinsToNbitcoinCoin(selectedUnspentCoins);
            var signingKeys = _signingKeyService.PrepareSigningKeys(nBitcoinSelectedUnspentCoins);

            calculatedFee = _txFeeService.CalculateTransactionFee((List<Coin>)nBitcoinSelectedUnspentCoins, signingKeys, bitcoinDestinationAddr,
                amount);
            if (!_txValidator.ValidateFundSufficiency(amount, calculatedFee, selectedUnspentCoins, out txBuildErrorCode))
                return false;

            tx = BuildTransaction(amount, nBitcoinSelectedUnspentCoins, signingKeys, calculatedFee,
                bitcoinDestinationAddr, out var builder);
            return VerifyBuiltTransaction(ref tx, out txBuildErrorCode, builder);
        }

        // coins are not selected, fee selected
        private bool TryBuildTx(Money amount, string destinationAddr, Money customFee, out Transaction tx,
            out List<Coin> autoSelectedUnspentCoins, out TransactionBuildErrorCode txBuildErrorCode)
        {
            tx = null;
            autoSelectedUnspentCoins = null;

            // Early validation checks
            if (!_txValidator.ValidateAmount(amount, out txBuildErrorCode)) return false;
            if (!_txValidator.ValidateCustomFee(customFee, out txBuildErrorCode)) return false;
            if (!_txValidator.ValidateAddress(destinationAddr, out txBuildErrorCode, out var bitcoinDestinationAddr))
                return false;

            PrepareCoinsForSelection(out var unspentConfirmedCoins, out var unspentUnconfirmedCoins);
            bool haveEnough = _coinSelectionService.AutoSelectCoinsWithFeeSeleceted(out autoSelectedUnspentCoins, amount, customFee, unspentConfirmedCoins);
            if (!haveEnough)
            {
                // Then try selecting coins with unconfirmed coins
                haveEnough = _coinSelectionService.AutoSelectCoinsWithFeeSeleceted(out autoSelectedUnspentCoins, amount, customFee, unspentUnconfirmedCoins);
            }

            if (!haveEnough)
            {
                txBuildErrorCode = TransactionBuildErrorCode.InsufficientFunds;
                return false;
            }

            var signingKeys = _signingKeyService.PrepareSigningKeys(autoSelectedUnspentCoins);

            tx = BuildTransaction(amount, autoSelectedUnspentCoins, signingKeys, customFee, bitcoinDestinationAddr,
                out var builder);
            return VerifyBuiltTransaction(ref tx, out txBuildErrorCode, builder);
        }

        // no coins no fee selected
        private bool TryBuildTx(Money amount, string destinationAddr, out Transaction tx,
            out List<Coin> autoSelectedUnspentCoins, out Money calculatedFee, out TransactionBuildErrorCode txBuildErrorCode)
        {
            tx = null;
            autoSelectedUnspentCoins = null;
            calculatedFee = Money.Zero;

            // Early validation checks
            if (!_txValidator.ValidateAmount(amount, out txBuildErrorCode)) return false;
            if (!_txValidator.ValidateAddress(destinationAddr, out txBuildErrorCode, out var bitcoinDestinationAddr))
                return false;

            PrepareCoinsForSelection(out var unspentConfirmedCoins, out var unspentUnconfirmedCoins);
            var haveEnough = _coinSelectionService.AutoSelectCoinsWithNoFeeSelected(out autoSelectedUnspentCoins, amount, out calculatedFee, unspentConfirmedCoins,
                bitcoinDestinationAddr);
            if (!haveEnough)
            {
                // Then try selecting coins with unconfirmed coins
                haveEnough = _coinSelectionService.AutoSelectCoinsWithNoFeeSelected(out autoSelectedUnspentCoins, amount, out calculatedFee,
                    unspentUnconfirmedCoins,
                    bitcoinDestinationAddr);
            }

            if (!haveEnough)
            {
                txBuildErrorCode = TransactionBuildErrorCode.InsufficientFunds;
                return false;
            }

            var signingKeys = _signingKeyService.PrepareSigningKeys(autoSelectedUnspentCoins);
            tx = BuildTransaction(amount, autoSelectedUnspentCoins, signingKeys, calculatedFee, bitcoinDestinationAddr,
                out var builder);
            return VerifyBuiltTransaction(ref tx, out txBuildErrorCode, builder);
        }

        private Transaction BuildTransaction(Money amount, IEnumerable<Coin> selectedUnspentCoins, List<Key> signingKeys,
          Money customFee, BitcoinAddress bitcoinDestinationAddr, out TransactionBuilder builder)
        {
            builder = _commonService.BitcoinNetwork.CreateTransactionBuilder();
            return builder
                .AddCoins(selectedUnspentCoins)
                .AddKeys(signingKeys.ToArray())
                .Send(bitcoinDestinationAddr, amount)
                .SetChange(_btcAddressService.DeriveNewChangeAddr())
                .SendFees(customFee)
                .BuildTransaction(true);
        }

        private bool VerifyBuiltTransaction(ref Transaction tx, out TransactionBuildErrorCode txBuildErrorCode,
            TransactionBuilder builder)
        {
            if (!builder.Verify(tx))
            {
                tx = null;
                txBuildErrorCode = TransactionBuildErrorCode.TransactionBuildFailed;
                return false;
            }
            else
            {
                txBuildErrorCode = TransactionBuildErrorCode.None;
                return true;
            }
        }

        private void PrepareCoinsForSelection(out List<Coin> unspentConfirmedCoins, out List<Coin> unspentUnconfirmedCoins)
        {
            unspentConfirmedCoins = _coinMapper.UtxoDetailsToNBitcoinCoin(_balanceService.Utxos.Where(utxo => utxo.Confirmed).ToList());
            unspentUnconfirmedCoins = _coinMapper.UtxoDetailsToNBitcoinCoin(_balanceService.Utxos.Where(utxo => !utxo.Confirmed).ToList());
        }
    }
}
