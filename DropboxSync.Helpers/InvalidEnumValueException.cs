using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DropboxSync.Helpers
{
    public class InvalidEnumValueException : Exception
    {
        public InvalidEnumValueException()
        {
        }

        public InvalidEnumValueException(string message) : base(message)
        {
        }

        public InvalidEnumValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidEnumValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
