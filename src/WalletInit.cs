using System;
using BtcWalletLibrary.Configuration;
using BtcWalletLibrary.Events;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Services;
using BtcWalletLibrary.Services.Adapters;
using BtcWalletLibrary.Services.Factories;
using BtcWalletLibrary.Services.Mappers;
using BtcWalletLibrary.Services.Strategies;
using BtcWalletLibrary.Services.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace BtcWalletLibrary
{
    /// <summary>
    /// Sets up the Bitcoin wallet library services for dependency injection.
    /// </summary>
    public static class WalletInit
    {
        /// <summary>
        /// Adds Bitcoin services to service collection.
        /// </summary>
        /// <param name="mnemonicWords">passphrase for wallet address generation</param>
        public static void AddBtcWalletLibraryServices(this IServiceCollection services, string mnemonicWords)
        {
            AddServices(services, mnemonicWords);
        }
        
        private static void AddServices(IServiceCollection services, string mnemonicWords)
        {
            RegisterHttpClient(services);

            services.AddSingleton<IElectrumxClientFactory, ElectrumxClientFactory>();
            services.AddSingleton(sp =>
            {
                var factory = sp.GetRequiredService<IElectrumxClientFactory>();
                return factory.CreateClient();
            });

            services.AddSingleton<IEventDispatcher, EventDispatcher>();
            services.AddSingleton<ICommonService>(sp => new CommonService(mnemonicWords, sp.GetRequiredService<IOptionsMonitor<WalletConfig>>()));

            services.AddSingleton<ISecureStorageAdapter, SecureStorageAdapter>();

            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<ITxInputDetailsService, TxInputDetailsService>();
            services.AddSingleton<IAddressService, AddressService>();
            services.AddSingleton<IAddressDerivationStrategy, MainAddressDerivationStrategy>();
            services.AddSingleton<IAddressDerivationStrategy, ChangeAddressDerivationStrategy>();
            services.AddSingleton<ITxHistoryService, TxHistoryService>();
            //firs transactions need to be restored from storage;
            services.AddSingleton<IBalanceService, BalanceService>();
            services.AddSingleton<ITxMapper, TxMapper>();
            services.AddSingleton<ITxValidator, TxValidator>();
            services.AddSingleton<ITxFeeService, TxFeeService>();
            services.AddSingleton<ISigningKeyService, SigningKeyService>();
            services.AddSingleton<ICoinMapper, CoinMapper>();
            services.AddSingleton<ICoinSelectionService, CoinSelectionService>();
            services.AddSingleton<ITxBuilderService, TxBuilderService>();
            services.AddSingleton<ITransferService, TransferService>();
        }

        private static void RegisterHttpClient(IServiceCollection services)
        {
            // Configure the named client using WalletConfig
            services.AddTransient<IConfigureOptions<HttpClientFactoryOptions>>(sp =>
            {
                var walletConfig = sp.GetRequiredService<IOptionsMonitor<WalletConfig>>().CurrentValue;

                return new ConfigureNamedOptions<HttpClientFactoryOptions>("TxFeeApi", options =>
                {
                    options.HttpClientActions.Add(client =>
                    {
                        client.BaseAddress = new Uri(walletConfig.BlockchainTxFeeApi.BaseUrl); // Set base URI from WalletConfig
                    });
                });
            });

            // Register the named client
            services.AddHttpClient("TxFeeApi");
        }
    }
}
