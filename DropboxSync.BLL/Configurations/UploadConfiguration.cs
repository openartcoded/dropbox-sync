﻿using DropboxSync.BLL.Entities;
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

            builder.Property(d => d.OriginalFileName)
                .IsRequired();

            builder.Property(d => d.DropboxFileId)
                .IsRequired();

            builder.Property(d => d.ContentType)
                .IsRequired();

            builder.Property(d => d.FileExtention)
                .IsRequired();

            builder.Property(d => d.FileSize)
                .IsRequired();
        }
    }
}
