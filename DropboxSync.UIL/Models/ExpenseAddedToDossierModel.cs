using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseAddedToDossierModel
    {
        public string DossierId { get; set; } = string.Empty;
        public string[] ExpenseIds { get; set; } = new string[0];
        public long Timestamp { get; set; }
    }
}
