﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class InvoiceModelBase : EventModel
    {
        public string InvoiceId { get; set; } = string.Empty;
        public string UploadId { get; set; } = string.Empty;
    }
}
