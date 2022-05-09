using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public interface IExpenseManager
    {
        void RedirectExpenses(string expenseEvent);
        void CreateExpense(ExpenseReceivedModel expenseReceived);
    }
}
