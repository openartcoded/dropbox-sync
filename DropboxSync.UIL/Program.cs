using DropboxSync.BLL;
using DropboxSync.UIL;
using DropboxSync.UIL.Managers;
using Microsoft.EntityFrameworkCore;
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

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddTransient<IExpenseManager, ExpenseManager>();
        services.AddTransient<IInvoiceManager, InvoiceManager>();
        services.AddTransient<IDossierManager, DossierManager>();
        services.AddTransient<IDocumentManager, DocumentManager>();
        services.AddSingleton<BrokerEventListener>();
    })
    .UseConsoleLifetime()
    .Build();

var context = host.Services.GetRequiredService<DropboxSyncContext>();
context.Database.EnsureCreated();

// Uncomment this if you don't care of the WAL file
// For more information on WAL file follow the next link
// https://stackoverflow.com/a/58108435/12273615

//using (var connection = context.Database.GetDbConnection())
//{
//    connection.Open();

//    using (var command = connection.CreateCommand())
//    {
//        command.CommandText = "PRAGMA wal_autocheckpoint=1;";
//        command.ExecuteNonQuery();
//    }

//    connection.Close();
//}


await host.RunAsync();