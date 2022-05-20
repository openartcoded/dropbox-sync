using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DocumentCreateUpdateModel : DocumentModelBase
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UploadId { get; set; } = string.Empty;
        public string? Tag { get; set; }
    }
}
