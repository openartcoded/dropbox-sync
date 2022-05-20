using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DropboxSync.Helpers
{
    public class DropboxRootFolderMissingException : Exception
    {
        public DropboxRootFolderMissingException()
        {
        }

        public DropboxRootFolderMissingException(string message) : base(message)
        {
        }

        public DropboxRootFolderMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DropboxRootFolderMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
