using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseAttachmentRemoveModel
    {
        public string ExpenseId { get; set; } = string.Empty;
        public string UploadId { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }
}
