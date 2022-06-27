using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DossierInvoiceRemovedModel : DossierModelBase
    {
        public string InvoiceId { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return obj is DossierInvoiceRemovedModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   DossierId == model.DossierId &&
                   InvoiceId == model.InvoiceId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, DossierId, InvoiceId);
        }
    }
}
