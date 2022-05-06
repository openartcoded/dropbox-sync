using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseLabelUpdateModel
    {
        public string ExpenseId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal PriceHVat { get; set; }
        public decimal Vat { get; set; }
        public long Timestamp { get; set; }
    }
}
