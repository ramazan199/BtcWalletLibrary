using BtcWalletLibrary.Configuration;
using ElectrumXClient;
using Microsoft.Extensions.Options;

namespace BtcWalletLibrary.Services.Factories
{
    internal interface IElectrumxClientFactory
    {
        IClient CreateClient();
    }

    internal class ElectrumxClientFactory : IElectrumxClientFactory
    {
        private readonly NodeConfigSection _config;

        public ElectrumxClientFactory(IOptionsMonitor<WalletConfig> options)
        {
            _config = options.CurrentValue.NodeConfiguration;
        }

        public IClient CreateClient()
        {
            return new Client(_config.Url, _config.Port, _config.UseSsl);
        }
    }
}