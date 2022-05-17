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
    }
}
