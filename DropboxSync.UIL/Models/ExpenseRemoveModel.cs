using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseRemoveModel
    {
        public string ExpenseId { get; set; }
        public string[] UploadIds { get; set; } = new string[0];
        public long Timestamp { get; set; }
    }
}
