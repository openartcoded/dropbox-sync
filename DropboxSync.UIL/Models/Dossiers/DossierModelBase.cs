using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DossierModelBase : EventModel
    {
        public string DossierId { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return obj is DossierModelBase @base &&
                   base.Equals(obj) &&
                   Timestamp == @base.Timestamp &&
                   Version == @base.Version &&
                   EventName == @base.EventName &&
                   DossierId == @base.DossierId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, DossierId);
        }
    }
}
