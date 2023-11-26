# Cosm.Net

Your all tools included Toolkit for interacting with blockchains built with the [Cosmos-Sdk](https://github.com/cosmos/cosmos-sdk/).
While building the lib I focused on 3 core aspects.
- Modularity
- Resiliency
- Efficiency

## Core Features
- Code generation for chain modules (includes packages for cosmos-sdk, osmosis, wasmd)
- Code generation for smart contracts from schemas (fully typed smart contract interactions)
- Fluent API for all chain interactions

## Getting Started
### Installation
#### Base package (required)
- [Cosm.Net](https://www.nuget.org/packages/Cosm.Net)

#### Modules
- [Cosmos-SDK](https://www.nuget.org/packages/Cosm.Net.CosmosSdk)
- [WasmD](https://www.nuget.org/packages/Cosm.Net.Wasm)
- [Osmosis](https://www.nuget.org/packages/Cosm.Net.Osmosis)

#### Generators
- [CosmWasm](https://www.nuget.org/packages/Cosm.Net.Generators.CosmWasm)
- [Proto (Chain modules)](https://www.nuget.org/packages/Cosm.Net.Generators.Proto)

### Connecting to a chain

```cs
//Create GrpcChannel for the connection
var channel = GrpcChannel.ForAddress(chain.GrpcUrl);

//Create client builder and install the modules & contracts you plan to interact with
var client = new CosmClientBuilder()
    .WithChannel(channel)
    .AddCosmosSdk()
    .AddWasmd()
    .RegisterContractSchema<ILevanaMarket>()
    .RegisterContractSchema<ILevanaFactory>()
    .Build();
```

### Adding ability to send transactions

```cs
//Define your wallet, fully matches Cosmos spec used in wallets like Keplr
var wallet = new Secp256k1Wallet("totally my mnemonic phrase")

//Create tx client builder and attach it to the previously connected chain client
var txClient = await new CosmTxClientBuilder()
    .WithCosmClient(cosmClient)
    .WithSigner(signer)
    .WithTxScheduler<SequentialTxScheduler>() //Handler that manages tx submissions. Custom implementation could include retries, gas estimations, load balancing etc
    .UseCosmosTxStructure() //Modular approach to transaction structures. Allows to integrate with heavy modified cosmos-sdk chains.
    .WithTxChainConfiguration(config =>
    {
        config.Bech32Prefix = "osmo";
        config.FeeDenom = "uosmo";
        config.GasPrice = 0.025m;
    })
    .BuildAsync()
```

### Querying the chain
You can query all the modules and contract registered in the chain client.
Retrieve instances of the contract / module from the client.

```cs
var bankModule = client.Module<IBankModule>();
var osmoBalance = await bankModule.BalanceAsync(wallet.GetAddress("osmo", "uosmo");

var levanaFactory = client.Contract<ILevanaFactory>("osmo1ssw6x553kzqher0earlkwlxasfm2stnl3ms3ma2zz4tnajxyyaaqlucd45");
var markets = await levanaFactory.MarketsAsync();
```

### Executing transactions
Transactions can be made up of multiple messages. The messages itself can be gotten the same way that queries work.
Once you have your message you build them together to a transaction which can be submitted.

```cs
var txBuilder = new CosmTxBuilder()
  .WithMemo("123"); //Configure memo, timeout height etc

var bankModule = client.Module<IBankModule>(); 
txBuilder.AddMessage(bankModule.Send(wallet.GetAddress("osmo"), "osmo1qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqmcn030",
  [new Coin()
  {
      Amount = "1000",
      Denom = "uosmo"
  }]));

var levanaMarket = client.Contract<ILevanaMarket>("osmo1nzddhaf086r0rv0gmrepn3ryxsu9qqrh7zmvcexqtfmxqgj0hhps4hruzu");
txBuilder.AddMessage(levanaFactory.Crank()); 

await txClient.SimulateAndPublishTxAsync(txBuilder.Build()); //Will hand over the scheduling of the tx over to the configured handler
```

