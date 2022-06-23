using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseModelBase : EventModel
    {
        public string ExpenseId { get; set; } = string.Empty;

        public override bool Equals(object? obj)
        {
            return obj is ExpenseModelBase @base &&
                   base.Equals(obj) &&
                   Timestamp == @base.Timestamp &&
                   Version == @base.Version &&
                   EventName == @base.EventName &&
                   ExpenseId == @base.ExpenseId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Timestamp, Version, EventName, ExpenseId);
        }
    }
}
