using BtcWalletLibrary.Exceptions;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BtcWalletLibrary.Services.Validators
{
    internal class TxValidator : ITxValidator
    {
        private readonly ICommonService _commonService;

        public TxValidator(ICommonService commonService)
        {
            _commonService = commonService;
        }

        public bool ValidateFundSufficiency(Money amount, Money fee, List<UnspentCoin> selectedUnspentCoins,
           out TransactionBuildErrorCode txBuildErrorCode)
        {
            var totalSatoshis = Money.Satoshis(selectedUnspentCoins.Sum(x => x.Amount * Money.COIN));
            if (fee + amount > totalSatoshis)
            {
                txBuildErrorCode = TransactionBuildErrorCode.InsufficientFunds;
                return false;
            }

            txBuildErrorCode = TransactionBuildErrorCode.None;
            return true;
        }

        public bool ValidateAddress(string destinationAddr, out TransactionBuildErrorCode txBuildErrorCode,
            out BitcoinAddress bitcoinDestinationAddr)
        {
            try
            {
                bitcoinDestinationAddr = BitcoinAddress.Create(destinationAddr, _commonService.BitcoinNetwork);
                txBuildErrorCode = TransactionBuildErrorCode.None;
                return true;
            }
            catch (Exception)
            {
                bitcoinDestinationAddr = null;
                txBuildErrorCode = TransactionBuildErrorCode.InvalidAddress;
                return false;
            }
        }

        public bool ValidateAddress(string destinationAddr, out TransactionBuildErrorCode txBuildErrorCode)
        {
            try
            {
                BitcoinAddress.Create(destinationAddr, _commonService.BitcoinNetwork);
                txBuildErrorCode = TransactionBuildErrorCode.None;
                return true;
            }
            catch (Exception)
            {
                txBuildErrorCode = TransactionBuildErrorCode.InvalidAddress;
                return false;
            }
        }

        public bool ValidateAmount(decimal amount, out TransactionBuildErrorCode txBuildError)
        {
            try
            {
                var money = new Money(amount, MoneyUnit.BTC);
                return ValidateAmount(money, out txBuildError);

            }
            catch (OverflowException)
            {
                txBuildError = TransactionBuildErrorCode.InvalidAmount;
                return false;
            }
        }

        public bool ValidateAmount(Money amount, out TransactionBuildErrorCode txBuildError)
        {
            if (amount <= Money.Zero)
            {
                txBuildError = TransactionBuildErrorCode.InvalidAmount;
                return false;
            }

            txBuildError = TransactionBuildErrorCode.None;
            return true;
        }

        public bool ValidateCustomFee(decimal customFee, out TransactionBuildErrorCode txBuildError)
        {
            try
            {
                var money = new Money(customFee, MoneyUnit.BTC);
                return ValidateCustomFee(money, out txBuildError);
            }
            catch (OverflowException)
            {
                txBuildError = TransactionBuildErrorCode.InvalidCustomFee;
                return false;
            }
        }

        public bool ValidateCustomFee(Money customFee, out TransactionBuildErrorCode txBuildError)
        {
            if (customFee <= Money.Zero)
            {
                txBuildError = TransactionBuildErrorCode.InvalidCustomFee;
                return false;
            }

            txBuildError = TransactionBuildErrorCode.None;
            return true;
        }

        public bool ValidateSelectedUnspentCoins(List<UnspentCoin> selectedUnspentCoins, decimal amount,
            out TransactionBuildErrorCode txBuildError)
        {
            return ValidateSelectedUnspentCoins(selectedUnspentCoins, new Money(amount, MoneyUnit.BTC),
                out txBuildError);
        }

        public bool ValidateSelectedUnspentCoins(List<UnspentCoin> selectedUnspentCoins, Money amount,
            out TransactionBuildErrorCode txBuildError)
        {
            if (selectedUnspentCoins == null || selectedUnspentCoins.Count == 0)
            {
                txBuildError = TransactionBuildErrorCode.NoUtxosSelected;
                return false;
            }

            var totalSatoshis = Money.Satoshis(selectedUnspentCoins.Sum(x => x.Amount * Money.COIN));
            if (amount > totalSatoshis)
            {
                txBuildError = TransactionBuildErrorCode.InsufficientFunds;
                return false;
            }

            txBuildError = TransactionBuildErrorCode.None;
            return true;
        }
    }
}
