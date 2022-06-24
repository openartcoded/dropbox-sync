using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
    public interface IDossierManager : IEventManager
    {
        [MethodEvent(typeof(DossierCreateModel), nameof(BrokerEvent.DossierCreated))]

        new bool Create<T>(T entity) where T : DossierCreateModel;
        bool CloseDossier(DossierCloseModel model);
        new bool Delete<T>(T entity) where T : DossierDeleteModel;
        bool AddExpense(DossierExpensesAddedModel model);
        bool RemoveExpense(DossierExpenseRemovedModel model);

        [MethodEvent(typeof(DossierInvoiceAddedModel), nameof(BrokerEvent.InvoiceAddedToDossier))]
        bool AddInvoice(DossierInvoiceAddedModel model);
        bool RemoveInvoice(DossierInvoiceRemovedModel model);
        bool Recall(DossierRecallForModificationModel model);
        new bool Update<T>(T entity) where T : DossierUpdateModel;
    }
}
