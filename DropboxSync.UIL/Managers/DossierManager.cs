using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public class DossierManager : IDossierManager
    {
        public bool AddExpense(DossierExpensesAddedModel model)
        {
            throw new NotImplementedException();
        }

        public bool AddInvoice(DossierInvoiceAddedModel model)
        {
            throw new NotImplementedException();
        }

        public bool CloseDossier(DossierCloseModel model)
        {
            throw new NotImplementedException();
        }

        public bool Create<T>(T entity) where T : DossierCreateModel
        {
            throw new NotImplementedException();
        }

        public bool Delete<T>(T entity) where T : DossierDeleteModel
        {
            throw new NotImplementedException();
        }

        public bool Recall(DossierRecallForModificationModel model)
        {
            throw new NotImplementedException();
        }

        public bool RemoveExpense(DossierExpenseRemovedModel model)
        {
            throw new NotImplementedException();
        }

        public bool RemoveInvoice(DossierInvoiceRemovedModel model)
        {
            throw new NotImplementedException();
        }

        public bool Update<T>(T entity) where T : DossierUpdateModel
        {
            throw new NotImplementedException();
        }

        bool IEventManager.Create<T>(T model)
        {
            throw new NotImplementedException();
        }

        bool IEventManager.Delete<T>(T model)
        {
            throw new NotImplementedException();
        }

        bool IEventManager.Update<T>(T model)
        {
            throw new NotImplementedException();
        }
    }
}
