using DropboxSync.BLL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<ExpenseEntity>
    {
        public void Configure(EntityTypeBuilder<ExpenseEntity> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.Label)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(d => d.Price)
                .IsRequired(false);

            builder.Property(d => d.Vat)
                .IsRequired(false);

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.UpdatedAt)
                .IsRequired();

            builder.HasMany(d => d.Uploads)
                .WithMany(d => d.Expenses)
                .UsingEntity(join => join.ToTable("ExpensesUploads"));
        }
    }
}
