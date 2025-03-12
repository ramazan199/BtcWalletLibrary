# Getting Started with BTCWalletLibrary

## Prerequisites
- [.NET Standard 2.0+ SDK](https://dotnet.microsoft.com/download)
- Basic knowledge of [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) in .NET

---

## Step 1: Install NuGet Package
Install the BTCWalletLibrary NuGet package in your .NET project:
```bash
dotnet add package BtcWalletLibrary
```

---

## Step 2: Configure Settings
Create an `appsettings.json` file in your project root, example file:
```json
{
  "CryptoWalletLibrary": {
    "NodeConfiguration": {
      "Url": "testnet.aranguren.org",
      "Port": 51001,
      "UseSSL": false,
      "Network": "TestNet",
      "MaxRangeEmptyAddr": 2
    },
    "BlockchainTxFeeApi": {
      "BaseUrl": "https://api.blockchain.info",
      "FeePath": "/mempool/fees"
    }
  }
}

```
**Note:** Set the file's **Copy to Output Directory** property to `Copy Always`.

---

## Step 3: Initialize the Library
Configure the library using Microsoft.Extensions.DependencyInjection:

```csharp

// Build configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Configure wallet library config
services.Configure<WalletConfig>(configuration.GetSection("CryptoWalletLibrary"));

// Setup dependency injection
var services = new ServiceCollection();

// Configure logging (Serilog shown here as example)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();
services.AddSingleton<ILoggingService, SerilogLoggingService>();


// Initialize with test mnemonic for demo purposes (replace with secure key management in production)
const string testMnemonic = 
    "asthma attend bus original science leaf deputy nuclear ticket valley vacuum tornado";
services.AddBtcWalletLibraryServices(testMnemonic);

// Build service provider
var serviceProvider = services.BuildServiceProvider();
```

---

## Demo App
Example usage in simple UI application:

link: https://github.com/ramazan199/BtcWalletUI

---

## Implementation Notes
- The example uses:
  - `Microsoft.Extensions.DependencyInjection` for DI
  - `Serilog` for logging (optional)
  - .NET's built-in configuration system
- Works in any .NET Standard 2.0+ compatible project:
  - Console apps
  - ASP.NET Core services
  - Desktop applications
  - Mobile apps (via .NET MAUI/Xamarin)

---





