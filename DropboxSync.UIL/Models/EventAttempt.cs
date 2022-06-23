using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class EventAttempt
    {
        public int Attempt { get; private set; }
        public string EventJson { get; set; }

        public EventAttempt(string eventJson)
        {
            if (string.IsNullOrEmpty(eventJson)) throw new ArgumentNullException(nameof(eventJson));

            Attempt = 0;
            EventJson = eventJson;
        }

    }
}