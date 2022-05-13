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
        public bool Create<T>(T entity) where T : InvoiceGeneratedModel
        {
            throw new NotImplementedException();
        }

        public bool Delete<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        public bool Update<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        bool IEventManager.Create<T>(T model)
        {
            throw new NotImplementedException();
        }
    }
}
