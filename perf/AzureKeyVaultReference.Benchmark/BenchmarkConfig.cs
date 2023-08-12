using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace AzureKeyVaultReference.Benchmark;

public class BenchmarkConfig : ManualConfig
{
    private readonly IConfig defaultConfig = DefaultConfig.Instance;

    public BenchmarkConfig()
    {
        AddColumnProvider(DefaultColumnProviders.Instance)
            .AddExporter(AsciiDocExporter.Default)
            .AddLogger(ConsoleLogger.Default)
            .AddAnalyser(defaultConfig.GetAnalysers().ToArray())
            .AddValidator(defaultConfig.GetValidators().ToArray())
            .WithSummaryStyle(defaultConfig.SummaryStyle)
            .WithOption(ConfigOptions.DisableLogFile, true)
            .WithArtifactsPath(Path.Combine(RuntimeContext.SolutionDirectory, "docs", "benchmarks"));

        var job = Job.MediumRun;

        AddJob(CustomJob(job, useNuGet: true).AsBaseline());
        AddJob(CustomJob(job, useNuGet: false));
    }

    private static Job CustomJob(Job job, bool useNuGet)
    {
        var result = job
            .WithId("AzureKeyVaultReference" + (useNuGet ? string.Empty : "-dev"))
            .WithArguments(
                new[]
                {
                    new MsBuildArgument("/p:BenchmarkFromNuGet=" + (useNuGet ? "true" : "false")),
                    new MsBuildArgument("/p:SignAssembly=false"),
                });

        if (useNuGet)
        {
            result = result.WithNuGet("Raiqub.AzureKeyVaultReference", "2.0.4");
        }

        return result;
    }
}
