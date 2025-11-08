using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace AzureKeyVaultReference.Benchmark;

public class BenchmarkConfig : ManualConfig
{
    private readonly IConfig _defaultConfig = DefaultConfig.Instance;

    public BenchmarkConfig()
    {
        AddColumnProvider(DefaultColumnProviders.Instance)
            .AddExporter(MarkdownExporter.GitHub)
            .AddLogger(ConsoleLogger.Default)
            .AddAnalyser(_defaultConfig.GetAnalysers().ToArray())
            .AddValidator(_defaultConfig.GetValidators().ToArray())
            .WithSummaryStyle(_defaultConfig.SummaryStyle)
            .WithOption(ConfigOptions.DisableLogFile, true)
            .WithArtifactsPath(Path.Combine(RuntimeContext.SolutionDirectory, "docs", "benchmarks"));

        var job = Job.MediumRun;

        AddJob(CustomJob(job, benchmarkFromNuget: true).AsBaseline());
        AddJob(CustomJob(job, benchmarkFromNuget: false));

        HideColumns(Column.Arguments, Column.NuGetReferences);
    }

    private static Job CustomJob(Job job, bool benchmarkFromNuget)
    {
        return job
            .WithId(benchmarkFromNuget ? "2.0.4" : "dev")
            .WithArguments(
            [
                new MsBuildArgument("/p:BenchmarkFromNuGet=" + (benchmarkFromNuget ? "true" : "false")),
                new MsBuildArgument("/p:SignAssembly=false"),
            ]);
    }
}
