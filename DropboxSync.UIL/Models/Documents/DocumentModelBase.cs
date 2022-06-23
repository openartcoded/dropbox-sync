using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DocumentModelBase : EventModel
    {
        public string DocumentId { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return obj is DocumentModelBase @base &&
                   base.Equals(obj) &&
                   Timestamp == @base.Timestamp &&
                   Version == @base.Version &&
                   EventName == @base.EventName &&
                   DocumentId == @base.DocumentId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, DocumentId);
        }
    }
}
