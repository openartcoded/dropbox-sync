using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DropboxSync.Helpers
{
    public class InvalidFileNameException : Exception
    {
        public InvalidFileNameException()
        {
        }

        public InvalidFileNameException(string message) : base(message)
        {
        }

        public InvalidFileNameException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        protected InvalidFileNameException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
