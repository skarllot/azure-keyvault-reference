using Raiqub.AzureKeyVaultReference.Configuration;
using SampleWorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .ConfigureAzureKeyVaultReference()
    .Build();

await host.RunAsync();
