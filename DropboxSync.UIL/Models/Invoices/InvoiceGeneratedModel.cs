using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    internal class InvoiceGeneratedModel : InvoiceModelBase
    {
        public bool ManualUpload { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public long DateOfInvoice { get; set; }
        public long DueDate { get; set; }
    }
}
