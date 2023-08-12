using System.Reflection;
using AzureKeyVaultReference.Benchmark;
using BenchmarkDotNet.Running;

BenchmarkSwitcher
    .FromAssembly(Assembly.GetCallingAssembly())
    .Run(args, new BenchmarkConfig());
