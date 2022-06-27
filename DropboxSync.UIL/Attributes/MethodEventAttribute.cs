using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Attributes
{
    public class MethodEventAttribute : Attribute
    {
        public Type EventType { get; private set; }
        public string EventName { get; private set; }

        public MethodEventAttribute(Type eventType, string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));

            EventType = eventType ??
                throw new ArgumentNullException(nameof(eventType));
            EventName = eventName;
        }
    }
}