using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class InvoiceModelBase : EventModel
    {
        public string InvoiceId { get; set; } = string.Empty;
        public string UploadId { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return obj is InvoiceModelBase @base &&
                   base.Equals(obj) &&
                   Timestamp == @base.Timestamp &&
                   Version == @base.Version &&
                   EventName == @base.EventName &&
                   InvoiceId == @base.InvoiceId &&
                   UploadId == @base.UploadId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, InvoiceId, UploadId);
        }
    }
}
