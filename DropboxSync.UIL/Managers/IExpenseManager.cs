using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public interface IExpenseManager : IEventManager
    {
        new bool Create<T>(T entity) where T : ExpenseReceivedModel;
        bool UpdateLabel(ExpenseLabelUpdatedModel model);
        bool UpdatePrice(ExpensePriceUpdatedModel model);
        new bool Delete<T>(T entity) where T : ExpenseRemovedModel;
        bool RemoveExpenseAttachment(ExpenseAttachmentRemovedModel model);
    }
}
