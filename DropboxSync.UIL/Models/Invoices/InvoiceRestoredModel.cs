using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class InvoiceRestoredModel : InvoiceModelBase
    {
        public override bool Equals(object? obj)
        {
            return obj is InvoiceRestoredModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   InvoiceId == model.InvoiceId &&
                   UploadId == model.UploadId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, InvoiceId, UploadId);
        }
    }
}
