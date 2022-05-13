using DropboxSync.BLL.IServices;
using DropboxSync.BLL.Services;
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

            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IDossierService, DossierService>();
            services.AddScoped<IUploadService, UploadService>();

            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IDropboxService, DropboxService>();
            // Configure injections of Automapper

            return services;
        }
    }
}
