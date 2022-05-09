using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    internal class ExpenseAttachmentRemovedModel : ExpenseModelBase
    {
        public string UploadId { get; set; } = string.Empty;
    }
}
