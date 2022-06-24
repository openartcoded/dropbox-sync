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

        [MethodEvent(typeof(DossierCloseModel), nameof(BrokerEvent.DossierClosed))]
        bool CloseDossier(DossierCloseModel model);

        [MethodEvent(typeof(DossierDeleteModel), nameof(BrokerEvent.DossierDeleted))]
        new bool Delete<T>(T entity) where T : DossierDeleteModel;

        [MethodEvent(typeof(DossierExpensesAddedModel), nameof(BrokerEvent.ExpensesAddedToDossier))]
        bool AddExpense(DossierExpensesAddedModel model);

        [MethodEvent(typeof(DossierExpenseRemovedModel), nameof(BrokerEvent.ExpenseRemovedFromDossier))]
        bool RemoveExpense(DossierExpenseRemovedModel model);

        [MethodEvent(typeof(DossierInvoiceAddedModel), nameof(BrokerEvent.InvoiceAddedToDossier))]
        bool AddInvoice(DossierInvoiceAddedModel model);

        [MethodEvent(typeof(DossierInvoiceRemovedModel), nameof(BrokerEvent.InvoiceRemovedFromDossier))]
        bool RemoveInvoice(DossierInvoiceRemovedModel model);

        [MethodEvent(typeof(DossierRecallForModificationModel), nameof(BrokerEvent.DossierRecallForModification))]
        bool Recall(DossierRecallForModificationModel model);

        [MethodEvent(typeof(DossierUpdateModel), nameof(BrokerEvent.DossierUpdated))]
        new bool Update<T>(T entity) where T : DossierUpdateModel;
    }
}
