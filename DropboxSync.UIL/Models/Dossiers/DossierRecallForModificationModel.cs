using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DossierRecallForModificationModel : DossierModelBase
    {
        public decimal TvaDue { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is DossierRecallForModificationModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   DossierId == model.DossierId &&
                   TvaDue == model.TvaDue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, DossierId, TvaDue);
        }
    }
}
