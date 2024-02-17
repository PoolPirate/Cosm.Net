using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Cosm.Net.Bench;

BenchmarkRunner.Run<SecretEncryptionBench>(DefaultConfig.Instance.WithOption(ConfigOptions.DisableOptimizationsValidator, true));
