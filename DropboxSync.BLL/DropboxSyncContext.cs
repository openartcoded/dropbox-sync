using DropboxSync.BLL.Configurations;
using DropboxSync.BLL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL
{
    public class DropboxSyncContext : DbContext
    {
        public DbSet<DossierEntity> Dossiers => Set<DossierEntity>();
        public DbSet<ExpenseEntity> Expenses => Set<ExpenseEntity>();
        public DbSet<InvoiceEntity> Invoices => Set<InvoiceEntity>();
        public DbSet<UploadEntity> Uploads => Set<UploadEntity>();

        public DropboxSyncContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbName = Environment.GetEnvironmentVariable("DROPBOX_DATABASE_NAME") ??
                "DropboxSyncDatabase";

            string appPath = Environment.GetEnvironmentVariable("DROPBOX_APPDATA_PATH") ??
                throw new NullReferenceException($"Environnement variable DROPBOX_APPDATA_PATH couldn't be retrieved!");

            string dbPath = Path.Join(appPath, dbName);

            optionsBuilder.UseSqlite($"Data Source={dbPath}", options =>
            {
                // Add options
            });

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DossierConfiguration());
            modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
            modelBuilder.ApplyConfiguration(new UploadConfiguration());
        }
    }
}
