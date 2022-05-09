using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    internal class DossierCloseModel : DossierModelBase
    {
        public string UploadId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
