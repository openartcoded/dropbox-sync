﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class ExpenseModelBase : EventModel
    {
        public string ExpenseId { get; set; } = string.Empty;
    }
}
