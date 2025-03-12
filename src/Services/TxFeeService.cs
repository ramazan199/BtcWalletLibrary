using BtcWalletLibrary.Interfaces;
using Microsoft.Extensions.Options;
using NBitcoin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BtcWalletLibrary.Configuration;
using Coin = NBitcoin.Coin;
using BtcWalletLibrary.DTOs.Responses;
using BtcWalletLibrary.Exceptions;

namespace BtcWalletLibrary.Services
{
    internal class TxFeeService : ITxFeeService
    {
        private readonly IAddressService _btcAddressService;
        private readonly ICommonService _commonService;
        private readonly ILoggingService _loggingService;
        private readonly string _blockChainFeeApiPath;
        private readonly HttpClient _httpClient;
        public Money BitFeeRecommendedFastest { get; private set; }


        public TxFeeService(IOptionsMonitor<WalletConfig> options, IAddressService btcAddressService,
            ICommonService commonService, ILoggingService loggingService, IHttpClientFactory httpClientFactory)
        {
            _blockChainFeeApiPath = options.CurrentValue.BlockchainTxFeeApi.FeePath;
            _btcAddressService = btcAddressService;
            _commonService = commonService;
            _loggingService = loggingService;
            _httpClient = httpClientFactory.CreateClient("TxFeeApi");
        }


        public async Task<TxFeeResult> GetRecommendedBitFeeAsync()
        {
            const decimal defaultFeeSatPerByte = 100m; // Configurable default
            var result = new TxFeeResult();

            try
            {
                var response =
                    await _httpClient.GetAsync(_blockChainFeeApiPath, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode(); // Throws for non-2xx status

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var priorityFee = json.Value<decimal>("priority");
                result.Fee = new Money(priorityFee, MoneyUnit.Satoshi);
                result.IsSuccess = true;
                result.IsDefault = false;
            }
            catch (HttpRequestException ex)
            {
                // Network-level errors (DNS, connection timeout, etc.)
                result.Fee = new Money(defaultFeeSatPerByte, MoneyUnit.Satoshi);
                result.OperationError = new TransactionFeeError($"Network error: {ex.Message}");
                result.IsDefault = true;
                _loggingService.LogError(ex, "Network failure. Using default fee");
            }
            catch (Exception ex) when (ex is Newtonsoft.Json.JsonReaderException)
            {
                // Parsing errors (malformed JSON, missing "priority" field)
                result.Fee = new Money(defaultFeeSatPerByte, MoneyUnit.Satoshi);
                result.OperationError = new TransactionFeeError($"Data format error: {ex.Message}");
                result.IsDefault = true;
                _loggingService.LogError(ex, "API response parsing failed");
            }
            catch (Exception ex)
            {
                // Unknown errors
                result.Fee = new Money(defaultFeeSatPerByte, MoneyUnit.Satoshi);
                result.OperationError = new TransactionFeeError($"Unexpected error: {ex.Message}");
                result.IsDefault = true;
                _loggingService.LogError(ex, "Critical fee fetch failure");
            }
            BitFeeRecommendedFastest = result.Fee;
            return result;
        }

        public Money CalculateTransactionFee(List<Coin> selectedUnspentCoins, List<Key> signingKeys,
            IDestination destinationAddress, Money amount)
        {
            var builder = _commonService.BitcoinNetwork.CreateTransactionBuilder();
            var txForFeeCalculation = builder
                .AddCoins(selectedUnspentCoins)
                .AddKeys(signingKeys.ToArray())
                .Send(destinationAddress, amount)
                .SetChange(_btcAddressService.DeriveNewChangeAddr())
                .SendFees(0L)
                .BuildTransaction(true);

            var fee = BitFeeRecommendedFastest * txForFeeCalculation.GetVirtualSize();
            return fee;
        }
    }
}