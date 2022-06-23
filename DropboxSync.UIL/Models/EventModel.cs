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

        public override bool Equals(object? obj)
        {
            return obj is EventModel model &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Timestamp, Version, EventName);
        }
    }
}
