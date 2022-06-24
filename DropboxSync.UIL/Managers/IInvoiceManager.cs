using DropboxSync.UIL.Attributes;
using DropboxSync.UIL.Enums;
using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public interface IInvoiceManager : IEventManager
    {
        [MethodEvent(typeof(InvoiceGeneratedModel), nameof(BrokerEvent.InvoiceGenerated))]

        new bool Create<T>(T entity) where T : InvoiceGeneratedModel;
        new bool Delete<T>(T entity) where T : InvoiceRemovedModel;
        bool Restore(InvoiceRestoredModel model);
    }
}
