using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseReceivedModel : ExpenseModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string[] UploadIds { get; set; } = new string[0];

        public override bool Equals(object? obj)
        {
            return obj is ExpenseReceivedModel model &&
                   base.Equals(obj) &&
                   Timestamp == model.Timestamp &&
                   Version == model.Version &&
                   EventName == model.EventName &&
                   ExpenseId == model.ExpenseId &&
                   Name == model.Name &&
                   EqualityComparer<string[]>.Default.Equals(UploadIds, model.UploadIds);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, ExpenseId, Name, UploadIds);
        }
    }
}
