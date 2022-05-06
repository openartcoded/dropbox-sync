using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    internal class InvoiceAddToDossier
    {
        public string DossierId { get; set; } = string.Empty;
        public string InvoiceId { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }
}
