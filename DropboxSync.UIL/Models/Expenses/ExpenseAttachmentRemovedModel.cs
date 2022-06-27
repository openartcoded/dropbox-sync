using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseAttachmentRemovedModel : ExpenseModelBase
    {
        public string UploadId { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return obj is ExpenseAttachmentRemovedModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   ExpenseId == model.ExpenseId &&
                   UploadId == model.UploadId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, ExpenseId, UploadId);
        }
    }
}
