using DropboxSync.BLL;
using DropboxSync.UIL;
using DropboxSync.UIL.Locators;
using DropboxSync.UIL.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class Program
{
    public static IHost? Host { get; private set; }

    public static async Task Main(string[] args)
    {
        Host = new HostBuilder()
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
            services.AddScoped<EventManagerLocator>();
        })
        .UseConsoleLifetime()
        .Build();

        var context = Host.Services.GetRequiredService<DropboxSyncContext>();
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


        await Host.RunAsync();
    }
}