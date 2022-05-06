﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    internal class ExpenseRemoveFromDossier
    {
        public string DossierId { get; set; } = string.Empty;
        public string ExpenseId { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }
}
