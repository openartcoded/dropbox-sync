using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    internal class InvoiceManager : IInvoiceManager
    {
        public bool Create(InvoiceModelBase model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(InvoiceModelBase model)
        {
            throw new NotImplementedException();
        }

        public bool Redirect(string eventJson)
        {
            return true;
        }

        public bool Update(InvoiceModelBase model)
        {
            throw new NotImplementedException();
        }
    }
}
