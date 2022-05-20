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
        // Settings ApplicationData App's path

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appPath = Path.Join(appData, "OAC-DropboxSync");

        if (!Directory.Exists(appPath))
        {
            Directory.CreateDirectory(appPath);
        }

        Environment.SetEnvironmentVariable("DROPBOX_APPDATA_PATH", appPath);

        services.AddHostedService<ShutdownManager>();

        services.ConfigureBusinessLayer();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddTransient<IExpenseManager, ExpenseManager>();
        services.AddTransient<IInvoiceManager, InvoiceManager>();
        services.AddTransient<IDossierManager, DossierManager>();
        services.AddTransient<IDocumentManager, DocumentManager>();
        services.AddSingleton<BrokerEventListener>();
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();