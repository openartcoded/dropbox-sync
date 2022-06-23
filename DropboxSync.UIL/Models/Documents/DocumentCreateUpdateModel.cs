using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DocumentCreateUpdateModel : DocumentModelBase
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UploadId { get; set; } = string.Empty;
        public string? Tag { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is DocumentCreateUpdateModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   DocumentId == model.DocumentId &&
                   Title == model.Title &&
                   Description == model.Description &&
                   UploadId == model.UploadId &&
                   Tag == model.Tag;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Timestamp);
            hash.Add(Version);
            hash.Add(EventName);
            hash.Add(DocumentId);
            hash.Add(Title);
            hash.Add(Description);
            hash.Add(UploadId);
            hash.Add(Tag);
            return hash.ToHashCode();
        }
    }
}
