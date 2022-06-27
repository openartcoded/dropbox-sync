using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseLabelUpdatedModel : ExpenseModelBase
    {
        public string Label { get; set; } = string.Empty;
        public decimal PriceHVat { get; set; }
        public decimal Vat { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ExpenseLabelUpdatedModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   ExpenseId == model.ExpenseId &&
                   Label == model.Label &&
                   PriceHVat == model.PriceHVat &&
                   Vat == model.Vat;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, ExpenseId, Label, PriceHVat, Vat);
        }
    }
}
