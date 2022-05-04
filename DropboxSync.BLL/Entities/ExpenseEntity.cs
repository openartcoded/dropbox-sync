using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Entities
{
    public class ExpenseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string Version { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public decimal Vat { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Relations
        public ICollection<UploadEntity>? Uploads { get; set; }
    }
}
