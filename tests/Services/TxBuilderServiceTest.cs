using BtcWalletLibrary.Models;
using BtcWalletLibrary.Services;
using Moq;
using NBitcoin;
using Coin = NBitcoin.Coin;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Exceptions;

namespace BtcWalletLibrary.Tests.Services
{
    public class TxBuilderServiceTests
    {
        private readonly Mock<IAddressService> _addressServiceMock;
        private readonly Mock<ICoinMapper> _coinMapperMock;
        private readonly Mock<ISigningKeyService> _signingKeyServiceMock;
        private readonly Mock<ITxFeeService> _txFeeServiceMock;
        private readonly Mock<ICoinSelectionService> _coinSelectionServiceMock;
        private readonly Mock<IBalanceService> _balanceServiceMock;
        private readonly Mock<ITxValidator> _txValidatorMock;
        private readonly TxBuilderService _txBuilderService;
        private readonly Network _network;

        public TxBuilderServiceTests()
        {
            _network = Network.TestNet;
            _addressServiceMock = new Mock<IAddressService>();
            _coinMapperMock = new Mock<ICoinMapper>();
            _signingKeyServiceMock = new Mock<ISigningKeyService>();
            _txFeeServiceMock = new Mock<ITxFeeService>();
            _coinSelectionServiceMock = new Mock<ICoinSelectionService>();
            _balanceServiceMock = new Mock<IBalanceService>();
            _txValidatorMock = new Mock<ITxValidator>();
            Mock<ICommonService> commonServiceMock = new();
            Mock<ILoggingService> logServiceMock = new();

            commonServiceMock.Setup(c => c.BitcoinNetwork).Returns(_network);

            _txBuilderService = new TxBuilderService(
                _addressServiceMock.Object,
                _coinMapperMock.Object,
                _signingKeyServiceMock.Object,
                _txFeeServiceMock.Object,
                _coinSelectionServiceMock.Object,
                _balanceServiceMock.Object,
                _txValidatorMock.Object,
                commonServiceMock.Object,
                logServiceMock.Object);
        }

        private (Key privateKey, BitcoinAddress address, Coin coin) GenerateTestCoin()
        {
            var privateKey = new Key();
            var address = privateKey.PubKey.GetAddress(ScriptPubKeyType.Segwit, _network);
            var outPoint = new OutPoint(new uint256(1), 0);
            var txOut = new TxOut(Money.Coins(1), address.ScriptPubKey);
            var coin = new Coin(outPoint, txOut);
            return (privateKey, address, coin);
        }


        #region Tests
        #region Fee & Coins Selected
        [Fact]
        public void TryBuildTx_ValidSelectedCoinsAndFee_BuildsTransaction()
        {
            // Arrange
            var (privateKey, address, coin) = GenerateTestCoin();
            var amount = Money.Coins(0.5m);
            var fee = Money.Satoshis(1000);
            var selectedCoins = new List<UnspentCoin> { new() { Amount = 1 } };
            var nbitcoinCoins = new List<Coin> { coin };
            var changeAddr = BitcoinAddress.Create("tb1qm0f4nu37q8u82txpj0l0cp924836gs2q4m9rdf", _network);

            SetupAmountValidation(amount);
            SetupCustomFeeValidation(fee);
            SetupSelectedCoinsValidation(selectedCoins, amount);
            SetupFundSufficiency(amount, fee, selectedCoins);
            SetupAddressValidation(address.ToString(), address);
            SetupCoinMapping(selectedCoins, nbitcoinCoins);
            SetupSigningKeys(nbitcoinCoins, privateKey);
            SetupChangeAddress(changeAddr);

            // Act
            var result = _txBuilderService.TryBuildTx(
                amount,
                address.ToString(),
                out var tx,
                out _,
                out _,
                out var txBuildErrorCode,
                selectedCoins,
                fee);

            // Assert
            Assert.True(result);
            Assert.Equal(TransactionBuildErrorCode.None, txBuildErrorCode);
            Assert.NotNull(tx);
        }

        [Fact]
        public void TryBuildTx_InsufficientFunds_ReturnsError()
        {
            // Arrange
            var amount = Money.Coins(1.5m);
            var fee = Money.Coins(0.5m);
            var selectedCoins = new List<UnspentCoin> { new() { Amount = 1 } };
            var address = BitcoinAddress.Create("tb1qm0f4nu37q8u82txpj0l0cp924836gs2q4m9rdf", Network.TestNet);

            SetupAmountValidation(amount);
            SetupCustomFeeValidation(fee);
            SetupSelectedCoinsValidation(selectedCoins, amount);
            SetupFundSufficiency(amount, fee, selectedCoins, isValid: false);
            SetupAddressValidation(address.ToString(), address);

            // Act
            var result = _txBuilderService.TryBuildTx(
                amount,
                address.ToString(),
                out _,
                out _,
                out _,
                out var txBuildErrorCode,
                selectedCoins,
                fee);

            // Assert
            Assert.False(result);
            Assert.Equal(TransactionBuildErrorCode.InsufficientFunds, txBuildErrorCode);
        }

        [Fact]
        public void TryBuildTx_InvalidAmount_ReturnsError()
        {
            // Arrange
            var amount = Money.Coins(1.5m);
            var fee = Money.Coins(0.5m);
            var selectedCoins = new List<UnspentCoin> { new() { Amount = 1 } };
            var address = BitcoinAddress.Create("tb1qm0f4nu37q8u82txpj0l0cp924836gs2q4m9rdf", Network.TestNet);

            SetupAmountValidation(amount, isValid: false);
            SetupCustomFeeValidation(fee);
            SetupSelectedCoinsValidation(selectedCoins, amount);
            SetupFundSufficiency(amount, fee, selectedCoins);
            SetupAddressValidation(address.ToString(), address);

            // Act
            var result = _txBuilderService.TryBuildTx(
                amount,
                address.ToString(),
                out _,
                out _,
                out _,
                out var txBuildErrorCode,
                selectedCoins,
                fee);

            // Assert
            Assert.False(result);
            Assert.Equal(TransactionBuildErrorCode.InvalidAmount, txBuildErrorCode);
        }

        [Fact]
        public void TryBuildTx_InvalidCustomFee_ReturnsError()
        {
            // Arrange
            var amount = Money.Coins(1.5m);
            var fee = Money.Coins(0.5m);
            var selectedCoins = new List<UnspentCoin> { new() { Amount = 1 } };
            var address = BitcoinAddress.Create("tb1qm0f4nu37q8u82txpj0l0cp924836gs2q4m9rdf", Network.TestNet);

            SetupAmountValidation(amount);
            SetupCustomFeeValidation(fee, isValid: false);
            SetupSelectedCoinsValidation(selectedCoins, amount);
            SetupFundSufficiency(amount, fee, selectedCoins);
            SetupAddressValidation(address.ToString(), address);

            // Act
            var result = _txBuilderService.TryBuildTx(
                amount,
                address.ToString(),
                out _,
                out _,
                out _,
                out var txBuildErrorCode,
                selectedCoins,
                fee);

            // Assert
            Assert.False(result);
            Assert.Equal(TransactionBuildErrorCode.InvalidCustomFee, txBuildErrorCode);
        }
        #endregion


        [Fact]
        public void TryBuildTx_CustomFeeNoCoins_BuildsTransaction()
        {
            // Arrange
            var (privateKey, address, coin) = GenerateTestCoin();
            var amount = Money.Coins(0.5m);
            var fee = Money.Satoshis(1000);
            var autoSelectedCoins = new List<Coin> { coin };
            var utxos = new List<UtxoDetailsElectrumx> { new() { Confirmed = false } };

            SetupAmountValidation(amount);
            SetupCustomFeeValidation(fee);
            SetupAddressValidation(address.ToString(), address);
            SetupBalanceServiceUtxos(utxos);

            _coinMapperMock.Setup(m => m.UtxoDetailsToNBitcoinCoin(It.IsAny<List<UtxoDetailsElectrumx>>()))
                .Returns([coin]);
            SetupCoinSelectionWithFee(autoSelectedCoins, amount, fee, [coin]);
            SetupSigningKeys(autoSelectedCoins, privateKey);
            SetupChangeAddress(address);

            // Act
            var result = _txBuilderService.TryBuildTx(
                amount,
                address.ToString(),
                out var tx,
                out _,
                out var selectedCoins,
                out var txBuildErrorCode,
                null,
                fee);

            // Assert
            Assert.True(result);
            Assert.Single(selectedCoins);
            Assert.Equal(TransactionBuildErrorCode.None, txBuildErrorCode);
            Assert.NotNull(tx);
        }

        [Fact]
        public void TryBuildTx_SelectedCoinsNoFee_BuildsTransaction()
        {
            // Arrange
            var (privateKey, address, coin) = GenerateTestCoin();
            var amount = Money.Coins(0.5m);
            var selectedCoins = new List<UnspentCoin> { new() { Amount = 1 } };
            var nbitcoinCoins = new List<Coin> { coin };
            var calculatedFee = Money.Satoshis(1000);

            SetupAmountValidation(amount);
            SetupSelectedCoinsValidation(selectedCoins, amount);
            SetupAddressValidation(address.ToString(), address);
            SetupFundSufficiency(amount, calculatedFee, selectedCoins);

            _txFeeServiceMock.Setup(f => f.CalculateTransactionFee(nbitcoinCoins, It.IsAny<List<Key>>(), address, amount))
                .Returns(calculatedFee);
            SetupCoinMapping(selectedCoins, nbitcoinCoins);
            SetupSigningKeys(nbitcoinCoins, privateKey);
            SetupChangeAddress(address);

            // Act
            var result = _txBuilderService.TryBuildTx(
                amount,
                address.ToString(),
                out var tx,
                out var fee,
                out _,
                out var txBuildErrorCode,
                selectedCoins);

            // Assert
            Assert.True(result);
            Assert.Equal(calculatedFee, fee);
            Assert.Equal(TransactionBuildErrorCode.None, txBuildErrorCode);
            Assert.NotNull(tx);
        }

        [Fact]
        public void TryBuildTx_NoCoinsNoFee_BuildsTransaction()
        {
            // Arrange
            var (privateKey, address, coin) = GenerateTestCoin();
            var amount = Money.Coins(0.5m);
            var autoSelectedCoins = new List<Coin> { coin };
            var utxos = new List<UtxoDetailsElectrumx> { new() { Confirmed = true } };

            SetupAmountValidation(amount);
            SetupAddressValidation(address.ToString(), address);
            SetupBalanceServiceUtxos(utxos);

            _coinMapperMock.Setup(m => m.UtxoDetailsToNBitcoinCoin(It.IsAny<List<UtxoDetailsElectrumx>>()))
                .Returns(new List<Coin> { coin });
            SetupCoinSelectionWithoutFee(autoSelectedCoins, amount, out var calculatedFee, [coin], address);
            SetupSigningKeys(autoSelectedCoins, privateKey);
            SetupChangeAddress(address);

            // Act
            var result = _txBuilderService.TryBuildTx(
                amount,
                address.ToString(),
                out var tx,
                out var actualFee,
                out var selectedCoins,
                out var txBuildErrorCode);

            // Assert
            Assert.True(result);
            Assert.Equal(calculatedFee, actualFee);
            Assert.Single(selectedCoins);
            Assert.Equal(TransactionBuildErrorCode.None, txBuildErrorCode);
            Assert.NotNull(tx);
        }
        #endregion


        #region Helper Methods
        private void SetupAmountValidation(Money amount, bool isValid = true)
        {
            var mockRes = _txValidatorMock.Setup(v => v.ValidateAmount(amount, out It.Ref<TransactionBuildErrorCode>.IsAny))
                .Returns(isValid);
            if (!isValid) mockRes.Callback(ValidateAmountCallback);
        }

        private void ValidateAmountCallback(Money _, out TransactionBuildErrorCode errorCode)
        {
            errorCode = TransactionBuildErrorCode.InvalidAmount;
        }

        private void SetupCustomFeeValidation(Money fee, bool isValid = true)
        {
            var mockRes = _txValidatorMock.Setup(v => v.ValidateCustomFee(fee, out It.Ref<TransactionBuildErrorCode>.IsAny))
                .Returns(isValid);
            if(!isValid) mockRes.Callback(ValidateCustomFeeCallback);
        }

        private void ValidateCustomFeeCallback(Money _, out TransactionBuildErrorCode txBuildErrorCode)
        {
            txBuildErrorCode = TransactionBuildErrorCode.InvalidCustomFee;
        }

        private void SetupSelectedCoinsValidation(List<UnspentCoin> selectedCoins, Money amount, bool isValid = true)
        {
            if (selectedCoins != null)
            {
                _txValidatorMock.Setup(v => v.ValidateSelectedUnspentCoins(selectedCoins, amount, out It.Ref<TransactionBuildErrorCode>.IsAny))
                    .Returns(isValid);
            }
        }

        private void SetupFundSufficiency(Money amount, Money fee, List<UnspentCoin> selectedCoins, bool isValid = true)
        {

            var mockResult = _txValidatorMock.Setup(v => v.ValidateFundSufficiency(amount, fee, selectedCoins, out It.Ref<TransactionBuildErrorCode>.IsAny))
                .Returns(isValid);
            if (!isValid) mockResult.Callback(ValidateFundSufficiencyCallback);
        }
        private void ValidateFundSufficiencyCallback(Money amount, Money fee, List<UnspentCoin> coins, out TransactionBuildErrorCode errorCode)
        {
            errorCode = TransactionBuildErrorCode.InsufficientFunds;
        }

        private void SetupAddressValidation(string destinationAddr, BitcoinAddress btcAddress)
        {
            _txValidatorMock.Setup(v => v.ValidateAddress(destinationAddr, out It.Ref<TransactionBuildErrorCode>.IsAny, out It.Ref<BitcoinAddress>.IsAny))
                .Callback((string _, out TransactionBuildErrorCode error, out BitcoinAddress addrOut) =>
                {
                    addrOut = btcAddress;
                    error = TransactionBuildErrorCode.None;
                })
                .Returns(true);
        }

        private void SetupCoinMapping(List<UnspentCoin> unspentCoins, List<Coin> nbitcoinCoins)
        {
            _coinMapperMock.Setup(m => m.UnspentCoinsToNbitcoinCoin(unspentCoins))
                .Returns(nbitcoinCoins);
        }

        private void SetupSigningKeys(List<Coin> coins, Key privateKey)
        {
            _signingKeyServiceMock.Setup(s => s.PrepareSigningKeys(coins))
                .Returns([privateKey]);
        }

        private void SetupChangeAddress(BitcoinAddress changeAddress)
        {
            _addressServiceMock.Setup(a => a.DeriveNewChangeAddr())
                .Returns(changeAddress);
        }

        private void SetupBalanceServiceUtxos(List<UtxoDetailsElectrumx> utxos)
        {
            _balanceServiceMock.Setup(b => b.Utxos).Returns(utxos);
        }

        private void SetupCoinSelectionWithFee(List<Coin> selectedCoins, Money amount, Money fee, List<Coin> availableCoins)
        {
            _coinSelectionServiceMock.Setup(c => c.AutoSelectCoinsWithFeeSeleceted(out selectedCoins, amount, fee, availableCoins))
                .Callback((out List<Coin> selected, Money _, Money _, List<Coin> _) =>
                {
                    selected = selectedCoins;
                })
                .Returns(true);
        }

        private void SetupCoinSelectionWithoutFee(List<Coin> selectedCoins, Money amount, out Money calculatedFee, List<Coin> availableCoins, IDestination changeAddr)
        {
            // Create local copy to capture the value
            Money fee = Money.Satoshis(1000);
            calculatedFee = fee;

            _coinSelectionServiceMock.Setup(c => c.AutoSelectCoinsWithNoFeeSelected(
                out It.Ref<List<Coin>>.IsAny,
                amount,
                out It.Ref<Money>.IsAny,
                availableCoins,
                changeAddr
            ))
            .Callback(new AutoSelectCallback((out List<Coin> actualSelected, Money _, out Money actualFee, List<Coin> _, IDestination _) =>
            {
                actualSelected = selectedCoins;
                actualFee = fee;
            }))
            .Returns(true);
        }

        // Required delegate to handle out parameters
        private delegate void AutoSelectCallback(
            out List<Coin> selected,
            Money amount,
            out Money fee,
            List<Coin> availableCoins,
            IDestination changeAddr
        );
        #endregion

    }
}