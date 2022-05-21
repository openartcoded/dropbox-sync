using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DocumentModelBase : EventModel
    {
        public string DocumentId { get; set; } = string.Empty;
    }
}
