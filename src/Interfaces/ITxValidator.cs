using BtcWalletLibrary.Exceptions;
using BtcWalletLibrary.Models;
using NBitcoin;
using System.Collections.Generic;

namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
    /// Interface for validating various aspects of Bitcoin transactions before they are built or broadcasted.
    /// This interface defines a set of methods to perform common validation checks on transaction-related data,
    /// such as recipient addresses, amounts, fees, and the sufficiency of funds.
    /// Implementations of this interface are responsible for enforcing transaction validity rules and providing
    /// specific error codes via the <see cref="TransactionBuildErrorCode"/> enum when validation fails.
    /// </summary>
    public interface ITxValidator
    {
        /// <summary>
        /// Validates a Bitcoin address string to ensure it is a valid and supported address format.
        /// </summary>
        /// <param name="destinationAddr">The Bitcoin address string to validate.</param>
        /// <param name="txBuildErrorCode">Output parameter that will be set to a <see cref="TransactionBuildErrorCode"/> value if validation fails.
        /// If validation is successful, this parameter may be set to <see cref="TransactionBuildErrorCode.None"/> or left unchanged.</param>
        /// <returns>True if the address is valid, false otherwise.</returns>
        bool ValidateAddress(string destinationAddr, out TransactionBuildErrorCode txBuildErrorCode);

        /// <summary>
        /// Validates a Bitcoin address string and attempts to parse it into a <see cref="BitcoinAddress"/> object.
        /// </summary>
        /// <param name="destinationAddr">The Bitcoin address string to validate.</param>
        /// <param name="txBuildErrorCode">Output parameter that will be set to a <see cref="TransactionBuildErrorCode"/> value if validation fails.
        /// If validation is successful, this parameter may be set to <see cref="TransactionBuildErrorCode.None"/> or left unchanged.</param>
        /// <param name="bitcoinDestinationAddr">Output parameter that will be populated with the parsed <see cref="BitcoinAddress"/> object if validation is successful.
        /// Will be null if validation fails.</param>
        /// <returns>True if the address is valid and parsed successfully, false otherwise.</returns>
        bool ValidateAddress(string destinationAddr, out TransactionBuildErrorCode txBuildErrorCode, out BitcoinAddress bitcoinDestinationAddr);

        /// <summary>
        /// Validates a decimal amount to ensure it is a valid Bitcoin amount.
        /// Checks for conditions such as negative amounts or amounts exceeding maximum limits.
        /// </summary>
        /// <param name="amount">The decimal amount to validate.</param>
        /// <param name="txBuildError">Output parameter that will be set to a <see cref="TransactionBuildErrorCode"/> value if validation fails.
        /// If validation is successful, this parameter may be set to <see cref="TransactionBuildErrorCode.None"/> or left unchanged.</param>
        /// <returns>True if the amount is valid, false otherwise.</returns>
        bool ValidateAmount(decimal amount, out TransactionBuildErrorCode txBuildError);

        /// <summary>
        /// Validates an amount represented as <see cref="Money"/> to ensure it is a valid Bitcoin amount.
        /// Checks for conditions such as negative amounts or amounts exceeding maximum limits.
        /// </summary>
        /// <param name="amount">The <see cref="Money"/> amount to validate.</param>
        /// <param name="txBuildError">Output parameter that will be set to a <see cref="TransactionBuildErrorCode"/> value if validation fails.
        /// If validation is successful, this parameter may be set to <see cref="TransactionBuildErrorCode.None"/> or left unchanged.</param>
        /// <returns>True if the amount is valid, false otherwise.</returns>
        bool ValidateAmount(Money amount, out TransactionBuildErrorCode txBuildError);

        /// <summary>
        /// Validates a custom fee amount provided as a decimal.
        /// Checks if the custom fee is within acceptable ranges (e.g., non-negative).
        /// </summary>
        /// <param name="customFee">The decimal custom fee amount to validate.</param>
        /// <param name="txBuildError">Output parameter that will be set to a <see cref="TransactionBuildErrorCode"/> value if validation fails.
        /// If validation is successful, this parameter may be set to <see cref="TransactionBuildErrorCode.None"/> or left unchanged.</param>
        /// <returns>True if the custom fee is valid, false otherwise.</returns>
        bool ValidateCustomFee(decimal customFee, out TransactionBuildErrorCode txBuildError);

        /// <summary>
        /// Validates a custom fee amount provided as <see cref="Money"/>.
        /// Checks if the custom fee is within acceptable ranges (e.g., non-negative).
        /// </summary>
        /// <param name="customFee">The <see cref="Money"/> custom fee amount to validate.</param>
        /// <param name="txBuildError">Output parameter that will be set to a <see cref="TransactionBuildErrorCode"/> value if validation fails.
        /// If validation is successful, this parameter may be set to <see cref="TransactionBuildErrorCode.None"/> or left unchanged.</param>
        /// <returns>True if the custom fee is valid, false otherwise.</returns>
        bool ValidateCustomFee(Money customFee, out TransactionBuildErrorCode txBuildError);

        /// <summary>
        /// Validates if the selected unspent coins (<see cref="UnspentCoin"/>) are sufficient to cover the transaction amount and the transaction fee.
        /// </summary>
        /// <param name="amount">The transaction amount (excluding fee) as <see cref="Money"/>.</param>
        /// <param name="fee">The transaction fee as <see cref="Money"/>.</param>
        /// <param name="selectedUnspentCoins">The list of <see cref="UnspentCoin"/> objects selected for the transaction inputs.</param>
        /// <param name="txBuildErrorCode">Output parameter that will be set to <see cref="TransactionBuildErrorCode.InsufficientFunds"/> if funds are insufficient, or another relevant error code if validation fails for other reasons.
        /// If validation is successful, this parameter may be set to <see cref="TransactionBuildErrorCode.None"/> or left unchanged.</param>
        /// <returns>True if the selected unspent coins are sufficient, false otherwise.</returns>
        bool ValidateFundSufficiency(Money amount, Money fee, List<UnspentCoin> selectedUnspentCoins, out TransactionBuildErrorCode txBuildErrorCode);

        /// <summary>
        /// Validates the selected unspent coins in relation to the specified transaction amount (decimal).
        /// This validation might include checks beyond fund sufficiency, such as ensuring that the selected coins are valid for the given amount in other transaction-specific ways if needed.
        /// </summary>
        /// <param name="selectedUnspentCoins">The list of <see cref="UnspentCoin"/> objects selected for the transaction inputs.</param>
        /// <param name="amount">The transaction amount (excluding fee) as a decimal.</param>
        /// <param name="txBuildError">Output parameter that will be set to a <see cref="TransactionBuildErrorCode"/> value if validation fails.
        /// If validation is successful, this parameter may be set to <see cref="TransactionBuildErrorCode.None"/> or left unchanged.</param>
        /// <returns>True if the selected unspent coins are valid, false otherwise.</returns>
        bool ValidateSelectedUnspentCoins(List<UnspentCoin> selectedUnspentCoins, decimal amount, out TransactionBuildErrorCode txBuildError);

        /// <summary>
        /// Validates the selected unspent coins in relation to the specified transaction amount (<see cref="Money"/>).
        /// This validation might include checks beyond fund sufficiency, such as ensuring that the selected coins are valid for the given amount in other transaction-specific ways if needed.
        /// </summary>
        /// <param name="selectedUnspentCoins">The list of <see cref="UnspentCoin"/> objects selected for the transaction inputs.</param>
        /// <param name="amount">The transaction amount (excluding fee) as <see cref="Money"/>.</param>
        /// <param name="txBuildError">Output parameter that will be set to a <see cref="TransactionBuildErrorCode"/> value if validation fails.
        /// If validation is successful, this parameter may be set to <see cref="TransactionBuildErrorCode.None"/> or left unchanged.</param>
        /// <returns>True if the selected unspent coins are valid, false otherwise.</returns>
        bool ValidateSelectedUnspentCoins(List<UnspentCoin> selectedUnspentCoins, Money amount, out TransactionBuildErrorCode txBuildError);
    }
}