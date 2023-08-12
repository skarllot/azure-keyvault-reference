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
            .AddExporter(MarkdownExporter.GitHub)
            .AddLogger(ConsoleLogger.Default)
            .AddAnalyser(defaultConfig.GetAnalysers().ToArray())
            .AddValidator(defaultConfig.GetValidators().ToArray())
            .WithSummaryStyle(defaultConfig.SummaryStyle)
            .WithOption(ConfigOptions.DisableLogFile, true)
            .WithArtifactsPath(Path.Combine(RuntimeContext.SolutionDirectory, "docs", "benchmarks"));

        var job = Job.MediumRun;

        AddJob(CustomJob(job, "2.0.4").AsBaseline());
        AddJob(CustomJob(job, null));

        HideColumns(Column.Arguments, Column.NuGetReferences);
    }

    private static Job CustomJob(Job job, string? pkgVersion)
    {
        var result = job
            .WithId(pkgVersion ?? "dev")
            .WithArguments(
                new[]
                {
                    new MsBuildArgument("/p:BenchmarkFromNuGet=" + (pkgVersion is null ? "false" : "true")),
                    new MsBuildArgument("/p:SignAssembly=false"),
                });

        if (pkgVersion is not null)
        {
            result = result.WithNuGet("Raiqub.AzureKeyVaultReference", pkgVersion);
        }

        return result;
    }
}
