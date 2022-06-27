using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class FailedEventModel
    {
        public int Attempt { get; set; }
        public string MessageJson { get; set; } = string.Empty;

        public FailedEventModel(int attempt, string messageJson)
        {
            Attempt = attempt;
            MessageJson = messageJson;
        }
    }
}