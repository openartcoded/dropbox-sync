using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    internal class ExpensePriceUpdatedModel : ExpenseModelBase
    {
        public decimal PriceHvat { get; set; }
        public decimal Vat { get; set; }
    }
}
