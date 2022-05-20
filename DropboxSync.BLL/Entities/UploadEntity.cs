using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Entities
{
    public class UploadEntity
    {
        public Guid Id { get; set; }
        public string UploadId { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string DropboxFileId { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }

        public ICollection<ExpenseEntity>? Expenses { get; set; }

        public UploadEntity()
        {

        }

        public UploadEntity(string uploadId, string originalFileName, string dropboxFileId, string contentType, long fileSize)
        {
            if (string.IsNullOrEmpty(uploadId)) throw new ArgumentNullException(nameof(uploadId));
            if (string.IsNullOrEmpty(originalFileName)) throw new ArgumentNullException(nameof(originalFileName));
            if (string.IsNullOrEmpty(dropboxFileId)) throw new ArgumentNullException(nameof(dropboxFileId));
            if (string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
            if (fileSize < 0) throw new ArgumentOutOfRangeException(nameof(fileSize));

            UploadId = uploadId;
            OriginalFileName = originalFileName;
            DropboxFileId = dropboxFileId;
            ContentType = contentType;
            FileSize = fileSize;
        }
    }
}
