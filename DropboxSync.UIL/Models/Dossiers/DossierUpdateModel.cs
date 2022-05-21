﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class DossierUpdateModel : DossierModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? TvaDue { get; set; }
    }
}
