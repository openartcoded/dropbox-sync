using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DossierExpenseRemovedModel : DossierModelBase
    {
        public string ExpenseId { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return obj is DossierExpenseRemovedModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   DossierId == model.DossierId &&
                   ExpenseId == model.ExpenseId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, DossierId, ExpenseId);
        }
    }
}
