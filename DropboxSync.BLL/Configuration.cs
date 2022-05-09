using DropboxSync.BLL.IServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL
{
    public static class Configuration
    {
        public static IServiceCollection ConfigureBusinessLayer(this IServiceCollection services)
        {
            services.AddScoped<DropboxSyncContext>();

            services.AddScoped<IExpenseService, IExpenseService>();
            services.AddScoped<IInvoiceService, IInvoiceService>();
            services.AddScoped<IDossierService, IDossierService>();

            services.AddScoped<IDropboxService, IDropboxService>();
            // Configure injections of Automapper

            return services;
        }
    }
}
