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

- [CosmosHub](https://www.nuget.org/packages/Cosm.Net.CosmosHub)
- [Osmosis](https://www.nuget.org/packages/Cosm.Net)

#### Generators

- [CosmWasm](https://www.nuget.org/packages/Cosm.Net.Generators.CosmWasm)
- [Proto (Chain modules)](https://www.nuget.org/packages/Cosm.Net.Generators.Proto)

### Connecting to a chain

Cosm.NET provides the option to create a read-only client, as well as a fully featured client for sending transactions to the chain. You can construct both of them using the `CosmClientBuilder`.

#### Read-Only Client

If you do not intend to send any transactions the only required calls on the builder are

- `WithChannel`
  - Sets the underlying connection to use
- `Install[X]`
  - Sets the defaults for the set chain like the address prefix and installs adapters for common functionality like contracts & accounts.
  - Only one chain can be installed for each client. If you are intending to interact with multiple chains, create separate client instances.

```cs
//Create GrpcChannel for the connection
var channel = GrpcChannel.ForAddress(chain.GrpcUrl);

var readClient = new CosmClientBuilder()
    .WithChannel(channel)
    .InstallOsmosis() //You gotta install the package for your desired chain
    .BuildReadClient();

//Must be called before using the client, loads information about the connected chain
await readClient.InitializeAsync();

//See below on how to use this client
```

#### Tx Client

The tx client provides all the features of the read-only client, but also exposes methods to simulate and send transactions.

Besides the config required for the read-only client you'll also needs to call all of these:

- `WithSigner`
  - Attaches a wallet to the client to be used for signing transactions
- `WithTxScheduler`
  - A class managing the nonce and schedules tx submissions. The library includes a fire-and-forget scheduler `SequentialTxScheduler`. Use the code of that scheduler as a reference to building your own if needed.
- `WithGasFeeProvider`
  - This method is only available on the internal builder (`AsInternal`)
  - Chain packages should include an extension on the `CosmTxClientBuilder` class that implements the fee mechanism used on that chain.
  - The lib comes with a built-in helper `WithConstantGasPrice` that works well for most Cosmos chains.

```cs
//Define your wallet, fully matches Cosmos spec used in wallets like Keplr
var wallet = new CosmosWallet("totally my mnemonic phrase")

//Create tx client builder and attach it to the previously connected chain client
var txClient = new CosmTxClientBuilder()
    .WithChannel(channel)
    .InstallOsmosis() //You gotta install the package for your desired chain
    .WithSigner(signer)
    .WithTxScheduler<SequentialTxScheduler>() //Handler that manages tx submissions. Custom implementation could include retries, gas estimations, load balancing etc
    .WithConstantGasPrice("uosmo", 0.025m)
    .BuildTxClient()

//Must be called before using the client, loads chain info as well as account info for the given signer
await txClient.InitializeAsync();
```

### Adding Capability to interact with smart-contracts

```cs

var builder = ... //previous examples

builder.AddWasmd(wasm => wasm
  .RegisterContractSchema<IPyth>()
  .RegisterContractSchema<ILevanaFactory>()
  .RegisterContractSchema<ILevanaMarket>()
);

var txClient = builder.BuildTxClient(); //Works for both read and tx clients

```

### Querying the chain

You can query all the modules and contract registered in the chain client.
Retrieve instances of the contract / module from the client.

```cs
var bankModule = client.Module<IBankModule>();
var osmoBalance = await bankModule.BalanceAsync(wallet.GetAddress("osmo", "uosmo"));

var levanaFactory = client.Contract<ILevanaFactory>("osmo1ssw6x553kzqher0earlkwlxasfm2stnl3ms3ma2zz4tnajxyyaaqlucd45");
var markets = await levanaFactory.MarketsAsync();
```

### Executing transactions

Transactions can be made up of multiple messages. The messages itself can be gotten the same way that queries work.
Once you have your message you build them together to a transaction which can be submitted.

Note: Each CosmTxBuilder should only be used for a single transaction. You can attach as many messages to the CosmTxBuilder as fit into a block (many).

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
txBuilder.AddMessage(levanaMarket.Crank());

await txClient.SimulateAndPublishTxAsync(txBuilder.Build());
//Will hand over the the tx over to the configured TxScheduler
```

## Modules

### CosmosHub

- Basic support for the CosmosHub chain.
  - All chain module interfaces at `Cosm.Net.CosmosHub.Modules`
- No custom gas helper (constant price is fine)
- No supports for wasmd.

### Osmosis

- Module support for CosmosSDK and Osmosis
  - Interfaces located at `Cosm.Net.Modules`
- Gas helper function for EIP1159
  ```cs
  builder.WithEIP1159MempoolGasPrice("uosmo")
  ```
- Full Wasmd support

## Code Generation

### Proto

#### ToDo, till then check the existing modules

### CosmWasm

1. Create a new class library project (It will not work in the same project!)
2. Add the nuget packages `Cosm.Net.Generators.CosmWasm` and `Cosm.Net`
3. Get the schema files for the desired contracts (json schema files)
4. Create a schemas directory in your project and put the schemas into there
5. In your .csproj reference each schema file as AdditionalFile

```xml
	<ItemGroup>
		<AdditionalFiles Include="schemas/market.json" />
		<AdditionalFiles Include="schemas/factory.json" />
	</ItemGroup>
```

6. Create a partial interface for each contract you want to generate bindings for that inherits from `Cosm.Net.Models.IContract`
7. Add the `ContractSchemaFilePath` Attribute to the interface and specify the filename of your schema. (Just the file name, not the path!)

```cs
using Cosm.Net.Attributes;
using Cosm.Net.Models;

namespace LevanaContracts.Factory;
[ContractSchemaFilePath("factory.json")]
public partial interface ILevanaFactory : IContract
{
}

```

8. Build the class library to a nuget package. Do NOT use ProjectReference, it will compile but the IDE will not recognize any of the bindings. Till the tooling is better compiling to a package is the only good option.
9. Register the schema in your main project (See above for how to do that)
