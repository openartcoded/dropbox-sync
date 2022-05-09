using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseReceivedModel : ExpenseModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string[] UploadIds { get; set; } = new string[0];
    }
}
