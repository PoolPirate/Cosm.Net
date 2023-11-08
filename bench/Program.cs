using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Cosm.Net.Bench;

BenchmarkRunner.Run<BIP32Bench>(DefaultConfig.Instance.WithOption(ConfigOptions.DisableOptimizationsValidator, true));
