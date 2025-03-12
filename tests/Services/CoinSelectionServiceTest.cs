using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Services;
using Moq;
using NBitcoin;


namespace BtcWalletLibrary.Tests.Services
{
    public class CoinSelectionServiceTests
    {
        private readonly Mock<ITxFeeService> _feeServiceMock;
        private readonly Mock<ISigningKeyService> _signingKeyServiceMock;
        private readonly CoinSelectionService _service;

        public CoinSelectionServiceTests()
        {
            _feeServiceMock = new Mock<ITxFeeService>();
            _signingKeyServiceMock = new Mock<ISigningKeyService>();
            _service = new CoinSelectionService(_feeServiceMock.Object, _signingKeyServiceMock.Object);
        }


        [Fact]
        public void AutoSelectCoinsWithNoFeeSelected_SelectsSingleCoinWhenSufficientAfterFee()
        {
            // Arrange
            var coinAmount = Money.Coins(6m);
            var coins = new List<Coin> { CreateCoin(coinAmount) };
            var amount = Money.Coins(5m);
            var expectedFee = Money.Coins(1m);

            _feeServiceMock.Setup(f => f.CalculateTransactionFee(
                    It.IsAny<List<Coin>>(),
                    It.IsAny<List<Key>>(),
                    It.IsAny<IDestination>(),
                    It.IsAny<Money>()))
                .Returns(expectedFee);

            _signingKeyServiceMock.Setup(s => s.PrepareSigningKeys(It.IsAny<List<Coin>>()))
                .Returns([new Key()]);

            // Act
            var result = _service.AutoSelectCoinsWithNoFeeSelected(
                out var coinsToSpend,
                amount,
                out var fee,
                coins,
                new Mock<IDestination>().Object);

            // Assert
            Assert.True(result);
            Assert.Single(coinsToSpend);
            Assert.Equal(coinAmount.Satoshi, coinsToSpend.Sum(c => c.Amount)); // Compare satoshis
            Assert.Equal(expectedFee, fee);
        }

        [Fact]
        public void AutoSelectCoinsWithNoFeeSelected_SelectsMultipleCoinsWhenInitialFeeInsufficient()
        {
            // Arrange
            var coins = new List<Coin>
            {
                CreateCoin(Money.Coins(6m)),
                CreateCoin(Money.Coins(2m))
            };
            var amount = Money.Coins(5m);

            _feeServiceMock.Setup(f => f.CalculateTransactionFee(
                    It.IsAny<List<Coin>>(),
                    It.IsAny<List<Key>>(),
                    It.IsAny<IDestination>(),
                    It.IsAny<Money>()))
                .Returns<List<Coin>, List<Key>, IDestination, Money>((selectedCoins, _, _, _) =>
                {
                    return selectedCoins.Count switch
                    {
                        1 => Money.Coins(2m),
                        2 => Money.Coins(1m),
                        _ => throw new ArgumentException("Unexpected number of coins")
                    };
                });

            _signingKeyServiceMock.Setup(s => s.PrepareSigningKeys(It.IsAny<List<Coin>>()))
                .Returns([new Key(), new Key()]);

            // Act
            var result = _service.AutoSelectCoinsWithNoFeeSelected(
                out var coinsToSpend,
                amount,
                out var fee,
                coins,
                new Mock<IDestination>().Object);

            // Assert
            Assert.True(result);
            Assert.Equal(2, coinsToSpend.Count);
            Assert.Equal(Money.Coins(8m).Satoshi, coinsToSpend.Sum(c => c.Amount)); // Compare satoshis
            Assert.Equal(Money.Coins(1m), fee);
        }

        [Fact]
        public void AutoSelectCoinsWithNoFeeSelected_ReturnsFalseWhenInsufficientFundsEvenAfterAddingAllCoins()
        {
            // Arrange
            var coins = new List<Coin>
            {
                CreateCoin(Money.Coins(3m)),
                CreateCoin(Money.Coins(2m))
            };
            var amount = Money.Coins(5m);

            _feeServiceMock.Setup(f => f.CalculateTransactionFee(
                    It.IsAny<List<Coin>>(),
                    It.IsAny<List<Key>>(),
                    It.IsAny<IDestination>(),
                    It.IsAny<Money>()))
                .Returns<List<Coin>, List<Key>, IDestination, Money>((selectedCoins, _, _, _) =>
                {
                    return selectedCoins.Count switch
                    {
                        1 => Money.Coins(1m),
                        2 => Money.Coins(2m),
                        _ => throw new ArgumentException("Unexpected number of coins")
                    };
                });

            _signingKeyServiceMock.Setup(s => s.PrepareSigningKeys(It.IsAny<List<Coin>>()))
                .Returns([new Key(), new Key()]);

            // Act
            var result = _service.AutoSelectCoinsWithNoFeeSelected(
                out var coinsToSpend,
                amount,
                out var fee,
                coins,
                new Mock<IDestination>().Object);

            // Assert
            Assert.False(result);
            Assert.Equal(2, coinsToSpend.Count);
            Assert.Equal(Money.Coins(5m).Satoshi, coinsToSpend.Sum(c => c.Amount)); // Compare satoshis
            Assert.Equal(Money.Coins(2m), fee);
        }

        [Fact]
        public void AutoSelectCoinsWithFeeSelected_SelectsSingleCoinWhenSufficient()
        {
            // Arrange
            var coins = new List<Coin> { CreateCoin(Money.Coins(5m)) };
            var amount = Money.Coins(3m);
            var fee = Money.Coins(1m);

            // Act
            var result = _service.AutoSelectCoinsWithFeeSeleceted(
                out var coinsToSpend,
                amount,
                fee,
                coins);

            // Assert
            Assert.True(result);
            Assert.Single(coinsToSpend);
            Assert.Equal(Money.Coins(5m).Satoshi, coinsToSpend.Sum(c => c.Amount)); // Compare satoshis
        }

        [Fact]
        public void AutoSelectCoinsWithFeeSelected_SelectsMultipleCoinsWhenNeeded()
        {
            // Arrange
            var coins = new List<Coin>
            {
                CreateCoin(Money.Coins(4m)),
                CreateCoin(Money.Coins(2m))
            };
            var amount = Money.Coins(4m);
            var fee = Money.Coins(1m);

            // Act
            var result = _service.AutoSelectCoinsWithFeeSeleceted(
                out var coinsToSpend,
                amount,
                fee,
                coins);

            // Assert
            Assert.True(result);
            Assert.Equal(2, coinsToSpend.Count);
            Assert.Equal(Money.Coins(6m).Satoshi, coinsToSpend.Sum(c => c.Amount)); // Compare satoshis
        }

        [Fact]
        public void AutoSelectCoinsWithFeeSelected_ReturnsFalseWhenTotalIsInsufficient()
        {
            // Arrange
            var coins = new List<Coin> { CreateCoin(Money.Coins(3m)) };
            var amount = Money.Coins(3m);
            var fee = Money.Coins(1m);

            // Act
            var result = _service.AutoSelectCoinsWithFeeSeleceted(
                out var coinsToSpend,
                amount,
                fee,
                coins);

            // Assert
            Assert.False(result);
            Assert.Single(coinsToSpend);
            Assert.Equal(Money.Coins(3m).Satoshi, coinsToSpend.Sum(c => c.Amount)); // Compare satoshis
        }

        [Fact]
        public void AutoSelectCoinsWithFeeSelected_ReturnsFalseWhenSumEqualsAmountPlusFee()
        {
            // Arrange
            var coins = new List<Coin> { CreateCoin(Money.Coins(5m)) };
            var amount = Money.Coins(4m);
            var fee = Money.Coins(1m);

            // Act
            var result = _service.AutoSelectCoinsWithFeeSeleceted(
                out var coinsToSpend,
                amount,
                fee,
                coins);

            // Assert
            Assert.False(result);
            Assert.Single(coinsToSpend);
            Assert.Equal(Money.Coins(5m).Satoshi, coinsToSpend.Sum(c => c.Amount)); // Compare satoshis
        }

        [Fact]
        public void AutoSelectCoinsWithNoFeeSelected_ReturnsFalseWhenNoCoins()
        {
            // Arrange
            var coins = new List<Coin>();
            var amount = Money.Coins(1m);

            // Act
            var result = _service.AutoSelectCoinsWithNoFeeSelected(
                out var coinsToSpend,
                amount,
                out var fee,
                coins,
                new Mock<IDestination>().Object);

            // Assert
            Assert.False(result);
            Assert.Empty(coinsToSpend);
            Assert.Equal(Money.Zero, fee);
        }

        #region Helper Methods

        private Coin CreateCoin(Money amount)
        {
            return new Coin(
                new OutPoint(new uint256(1), 0),
                new TxOut(amount, Script.Empty)
            );
        }

        #endregion
    }
}