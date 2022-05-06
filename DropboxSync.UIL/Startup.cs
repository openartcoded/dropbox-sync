using DropboxSync.UIL.Managers;
using DropboxSync.UIL.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL
{
    public static class StartUp
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IExpenseManager, ExpenseManager>();

            services.AddSingleton<BrokerEventListener>();
        }

        public static void Build()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
