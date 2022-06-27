using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DossierCloseModel : DossierModelBase
    {
        public string UploadId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return obj is DossierCloseModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   DossierId == model.DossierId &&
                   UploadId == model.UploadId &&
                   Name == model.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, DossierId, UploadId, Name);
        }
    }
}
