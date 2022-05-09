using DropboxSync.BLL;
using DropboxSync.UIL;
using DropboxSync.UIL.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<ShutdownManager>();

        services.ConfigureBusinessLayer();

        services.AddTransient<IExpenseManager, ExpenseManager>();
        services.AddTransient<IInvoiceManager, InvoiceManager>();
        services.AddTransient<IDossierManager, DossierManager>();
        services.AddSingleton<BrokerEventListener>();
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();