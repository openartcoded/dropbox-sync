using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class InvoiceGeneratedModel : InvoiceModelBase
    {
        public bool ManualUpload { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public long DateOfInvoice { get; set; }
        public long DueDate { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is InvoiceGeneratedModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   InvoiceId == model.InvoiceId &&
                   UploadId == model.UploadId &&
                   ManualUpload == model.ManualUpload &&
                   SubTotal == model.SubTotal &&
                   Taxes == model.Taxes &&
                   InvoiceNumber == model.InvoiceNumber &&
                   DateOfInvoice == model.DateOfInvoice &&
                   DueDate == model.DueDate;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Timestamp);
            hash.Add(Version);
            hash.Add(EventName);
            hash.Add(InvoiceId);
            hash.Add(UploadId);
            hash.Add(ManualUpload);
            hash.Add(SubTotal);
            hash.Add(Taxes);
            hash.Add(InvoiceNumber);
            hash.Add(DateOfInvoice);
            hash.Add(DueDate);
            return hash.ToHashCode();
        }
    }
}
