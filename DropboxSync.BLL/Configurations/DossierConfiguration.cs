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
    public class DossierConfiguration : IEntityTypeConfiguration<DossierEntity>
    {
        public void Configure(EntityTypeBuilder<DossierEntity> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(d => d.DueVat)
                .IsRequired(false)
                .HasColumnType("decimal(18,4)");

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.UpdatedAt)
                .IsRequired();

            builder.HasOne(d => d.Upload);

            builder.Property(d => d.Upload)
                .IsRequired(false);

            builder.Property(d => d.UploadId)
                .IsRequired(false);
        }
    }
}
