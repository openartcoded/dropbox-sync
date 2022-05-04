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
        public DbSet<DossierEntity> Dossiers { get; set; }
        public DbSet<ExpenseEntity> Expenses { get; set; }
        public DbSet<InvoiceEntity> Invoices { get; set; }
        public DbSet<UploadEntity> Uploads { get; set; }

        public DropboxSyncContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=DropboxSyncDatabase.db", options =>
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
