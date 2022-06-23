using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DossierExpensesAddedModel : DossierModelBase
    {
        public string[] ExpenseIds { get; set; } = new string[0];

        public override bool Equals(object? obj)
        {
            return obj is DossierExpensesAddedModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   DossierId == model.DossierId &&
                   EqualityComparer<string[]>.Default.Equals(ExpenseIds, model.ExpenseIds);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, DossierId, ExpenseIds);
        }
    }
}
