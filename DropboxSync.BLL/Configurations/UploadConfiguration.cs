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
    public class UploadConfiguration : IEntityTypeConfiguration<UploadEntity>
    {
        public void Configure(EntityTypeBuilder<UploadEntity> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.DropboxPath)
                .IsRequired();

            builder.Property(d => d.BackUpPath)
                .IsRequired();
        }
    }
}
