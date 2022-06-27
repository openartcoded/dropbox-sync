using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DropboxSync.Helpers
{
    public class InvalidVariableTypeException : Exception
    {
        public InvalidVariableTypeException()
        {
        }

        public InvalidVariableTypeException(string message) : base(message)
        {
        }

        public InvalidVariableTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidVariableTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}