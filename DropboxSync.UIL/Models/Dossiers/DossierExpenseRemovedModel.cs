using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    internal class DossierExpenseRemovedModel : DossierModelBase
    {
        public string ExpenseId { get; set; } = string.Empty;
    }
}
