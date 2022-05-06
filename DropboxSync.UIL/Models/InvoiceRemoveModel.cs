using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    internal class InvoiceRemoveModel
    {
        public string InvoiceId { get; set; } = string.Empty;
        public string UploadId { get; set; } = string.Empty;
        public bool LogicalDelete { get; set; }
        public long Timestamp { get; set; }
    }
}
