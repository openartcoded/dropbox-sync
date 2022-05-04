using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Entities
{
    public class UploadEntity
    {
        public Guid Id { get; set; }
        public string DropboxPath { get; set; } = string.Empty;
        public string BackUpPath { get; set; } = string.Empty;

        // Relations

        public ICollection<ExpenseEntity>? Expenses { get; set; }

    }
}
