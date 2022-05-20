using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DropboxSync.Helpers
{
    public class NullValueException : Exception
    {
        public NullValueException()
        {
        }

        public NullValueException(string message) : base(message)
        {
        }

        public NullValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NullValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
