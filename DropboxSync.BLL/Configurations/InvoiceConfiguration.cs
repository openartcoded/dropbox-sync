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
    public class InvoiceConfiguration : IEntityTypeConfiguration<InvoiceEntity>
    {
        public void Configure(EntityTypeBuilder<InvoiceEntity> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.InvoiceNumber)
                .IsRequired(true)
                .HasMaxLength(12);

            builder.Property(d => d.ManualUpload)
                .IsRequired();

            builder.Property(d => d.SubTotal)
                .IsRequired();
        }
    }
}
