using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpensePriceUpdateModel
    {
        public string ExpenseId { get; set; } = string.Empty;
        public decimal PriceHvat { get; set; }
        public decimal Vat { get; set; }
        public long Timestamp { get; set; }
    }
}
