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
    public interface IExpenseManager : IEventManager
    {
        [MethodEvent(typeof(ExpenseReceivedModel), nameof(BrokerEvent.ExpenseReceived))]
        new bool Create<T>(T model) where T : ExpenseReceivedModel;

        [MethodEvent(typeof(ExpenseLabelUpdatedModel), nameof(BrokerEvent.ExpenseLabelUpdated))]
        bool UpdateLabel(ExpenseLabelUpdatedModel model);

        [MethodEvent(typeof(ExpensePriceUpdatedModel), nameof(BrokerEvent.ExpensePriceUpdated))]
        bool UpdatePrice(ExpensePriceUpdatedModel model);

        [MethodEvent(typeof(ExpenseRemovedModel), nameof(BrokerEvent.ExpenseRemoved))]
        new bool Delete<T>(T model) where T : ExpenseRemovedModel;

        [MethodEvent(typeof(ExpenseAttachmentRemovedModel), nameof(BrokerEvent.ExpenseAttachmentRemoved))]
        bool RemoveExpenseAttachment(ExpenseAttachmentRemovedModel model);
    }
}
