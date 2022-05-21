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
    public class DocumentConfiguration : IEntityTypeConfiguration<DocumentEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentEntity> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Title)
                .IsRequired(true);

            builder.Property(d => d.Description)
                .IsRequired(true);

            builder.HasOne(d => d.Upload)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        }
    }
}
