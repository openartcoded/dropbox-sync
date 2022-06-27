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

        [MethodEvent(typeof(InvoiceRemovedModel), nameof(BrokerEvent.InvoiceRemoved))]
        new bool Delete<T>(T entity) where T : InvoiceRemovedModel;

        [MethodEvent(typeof(InvoiceRestoredModel), nameof(BrokerEvent.InvoiceRestored))]
        bool Restore(InvoiceRestoredModel model);
    }
}
