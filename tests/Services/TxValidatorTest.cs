using BtcWalletLibrary.Exceptions;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using BtcWalletLibrary.Services.Validators;
using Moq;
using NBitcoin;

namespace BtcWalletLibrary.Tests.Services;

public class TxValidatorTest
{
    private readonly TxValidator _txValidator;

    public TxValidatorTest()
    {
        Mock<ICommonService> mockCommonService = new();
        mockCommonService.SetupGet<Network>(service => service.BitcoinNetwork).Returns(Network.TestNet);
        _txValidator = new TxValidator(mockCommonService.Object);
    }


    [Theory]
    [InlineData("tb1qqawdmracdp5jhvzm4e5s2z4pp4ty4ndq33d8pe")] //bech32 format
    [InlineData("mkHS9ne12qx9pS9VojpwU5xtRd4T7X7ZUt")] //legacy format
    public void ValidateAddress_WhenInputIsValidBtcAddr(string address)
    {
        //Act
        var isValid = _txValidator.ValidateAddress(address, out var txBuildErrorCode);

        //Assert
        Assert.True(isValid);
        Assert.True(txBuildErrorCode.Equals(TransactionBuildErrorCode.None));
    }


    [Fact]
    public void ValidateAddress_WithInvalidInput_ReturnsFalseAndErrorObj()
    {
        //Arrange
        var address = "wrongAddress";

        //Act
        var isValid = _txValidator.ValidateAddress(address, out var txBuildErrorCode);

        //Assert
        Assert.False(isValid);
        Assert.True(txBuildErrorCode.Equals(TransactionBuildErrorCode.InvalidAddress));
    }


    [Theory]
    [InlineData(10, 10, 30)]   // Total (30) > Amount (10) + Fee (10) = 20
    [InlineData(20, 5, 25)]    // Total (25) == Amount (20) + Fee (5) = 25
    [InlineData(0.5, 0.1, 1)] // Small values (0.6 needed)
    public void ValidateFundSufficiency_WithSufficientFunds_ReturnsTrue(
        decimal amountBtc,
        decimal feeBtc,
        decimal totalUnspentBtc)
    {
        // Arrange
        var amount = Money.Satoshis(amountBtc * Money.COIN);
        var fee = Money.Satoshis(feeBtc * Money.COIN);
        var selectedUnspentCoins = new List<UnspentCoin>
        {
            new() { Amount = totalUnspentBtc }
        };

        // Act
        var isValid = _txValidator.ValidateFundSufficiency(
            amount, fee, selectedUnspentCoins, out var errorCode);

        // Assert
        Assert.True(isValid);
        Assert.Equal(TransactionBuildErrorCode.None, errorCode);
    }


    [Theory]
    [InlineData(50, 10, 30)]  // Total (30) < Amount (50) + Fee (10) = 60
    [InlineData(25, 1, 25)]   // Total (25) < Amount (25) + Fee (1) = 26
    [InlineData(0, 0.1, 0.05)] // Zero amount, insufficient for fee
    public void ValidateFundSufficiency_WithInsufficientFunds_ReturnsFalse(
        decimal amountBtc,
        decimal feeBtc,
        decimal totalUnspentBtc)
    {
        // Arrange
        var amount = Money.Satoshis(amountBtc * Money.COIN);
        var fee = Money.Satoshis(feeBtc * Money.COIN);
        var selectedUnspentCoins = new List<UnspentCoin>
        {
            new() { Amount = totalUnspentBtc }
        };

        // Act
        var isValid = _txValidator.ValidateFundSufficiency(
            amount, fee, selectedUnspentCoins, out var errorCode);

        // Assert
        Assert.False(isValid);
        Assert.Equal(TransactionBuildErrorCode.InsufficientFunds, errorCode);
    }


    [Theory]
    [InlineData(1, true, TransactionBuildErrorCode.None)]         // Valid amount
    [InlineData(0, false, TransactionBuildErrorCode.InvalidAmount)] // Zero amount
    [InlineData(-1, false, TransactionBuildErrorCode.InvalidAmount)] // Negative amount
    public void ValidateAmount_WithMoneyInput_ReturnsExpectedResult(
    decimal amountBtc,
    bool expectedIsValid,
    TransactionBuildErrorCode expectedErrorCode)
    {
        // Arrange
        var amount = Money.Satoshis(amountBtc * Money.COIN);

        // Act
        var isValid = _txValidator.ValidateAmount(amount, out var errorCode);

        // Assert
        Assert.Equal(expectedIsValid, isValid);
        Assert.Equal(expectedErrorCode, errorCode);
    }


    [Theory]
    [InlineData(1)]     // Valid amount
    [InlineData(0.5)]   // Valid decimal
    public void ValidateAmount_WithValidDecimal_ReturnsTrue(decimal amount)
    {
        // Act
        var isValid = _txValidator.ValidateAmount(amount, out var errorCode);

        // Assert
        Assert.True(isValid);
        Assert.Equal(TransactionBuildErrorCode.None, errorCode);
    }


    // Cant create Btc Money Unit from decimal values bigger than 10^11
    [Fact]
    public void ValidateAmount_WithOverflowDecimal_ReturnsFalse()
    {
        // Arrange
        // Calculate the maximum BTC value that won't overflow
        decimal maxBtcBeforeOverflow = (decimal)long.MaxValue / Money.COIN;
        // Add 1 BTC to force overflow
        decimal overflowAmount = maxBtcBeforeOverflow + 1;

        // Act
        var isValid = _txValidator.ValidateAmount(overflowAmount, out var errorCode);

        // Assert
        Assert.False(isValid);
        Assert.Equal(TransactionBuildErrorCode.InvalidAmount, errorCode);
    }

    [Theory]
    [InlineData(0)]     // Zero
    [InlineData(-0.5)]  // Negative
    public void ValidateAmount_WithInvalidDecimal_ReturnsFalse(decimal amount)
    {
        // Act
        var isValid = _txValidator.ValidateAmount(amount, out var errorCode);

        // Assert
        Assert.False(isValid);
        Assert.Equal(TransactionBuildErrorCode.InvalidAmount, errorCode);
    }
}