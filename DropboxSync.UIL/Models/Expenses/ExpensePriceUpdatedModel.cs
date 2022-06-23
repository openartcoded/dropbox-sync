using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpensePriceUpdatedModel : ExpenseModelBase
    {
        public decimal PriceHvat { get; set; }
        public decimal Vat { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ExpensePriceUpdatedModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   ExpenseId == model.ExpenseId &&
                   PriceHvat == model.PriceHvat &&
                   Vat == model.Vat;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, ExpenseId, PriceHvat, Vat);
        }
    }
}
