using Raiqub.AzureKeyVaultReference.Configuration;
using SampleWorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .ConfigureAzureKeyVaultReference(
        options => options.GetDefaultVaultNameOrUri = () => Environment.GetEnvironmentVariable("KEYVAULTNAME"))
    .Build();

await host.RunAsync();
