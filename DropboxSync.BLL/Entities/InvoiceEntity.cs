using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Entities
{
    public class InvoiceEntity
    {
        public Guid Id { get; set; }
        public bool ManualUpload { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public DateOnly DueDate { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Relations
        public Guid UploadId { get; set; }
        public UploadEntity? Upload { get; set; }
    }
}
