using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class EventModel
    {
        public long Timestamp { get; set; }
        public string Version { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
    }
}
