﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Entities
{
    public class UploadEntity
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string DropboxFileId { get; set; } = string.Empty;
        public string DropboxPath { get; set; } = string.Empty;
        public string LocalRelativePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string FileExtention { get; set; } = string.Empty;
        public float FileSize { get; set; }

        // Relations

        public ICollection<ExpenseEntity>? Expenses { get; set; }

    }
}
