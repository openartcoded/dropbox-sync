using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseReceivedModel
    {
        public string ExpenseId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string[] UploadIds { get; set; } = new string[0];
        public int Timestamp { get; set; } = 0;
    }
}
