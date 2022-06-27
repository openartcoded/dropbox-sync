using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class InvoiceRemovedModel : InvoiceModelBase
    {
        public bool LogicalDelete { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is InvoiceRemovedModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   InvoiceId == model.InvoiceId &&
                   UploadId == model.UploadId &&
                   LogicalDelete == model.LogicalDelete;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, InvoiceId, UploadId, LogicalDelete);
        }
    }
}
