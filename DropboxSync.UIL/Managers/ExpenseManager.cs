using DropboxSync.BLL.IServices;
using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public class ExpenseManager : IExpenseManager
    {
        private readonly IExpenseService _expenseService;

        public ExpenseManager(IExpenseService expenseService)
        {
            _expenseService = expenseService ??
                throw new ArgumentNullException(nameof(expenseService));
        }

        public bool Create(ExpenseModelBase model)
        {
            if (model.GetType() != typeof(ExpenseReceivedModel))
                throw new ArgumentException($"To create an expense, {nameof(model)}'s type must be of " +
                    $"type {typeof(ExpenseReceivedModel)}");

            return true;
        }

        public bool Delete(ExpenseModelBase model)
        {
            throw new NotImplementedException();
        }

        public bool Redirect(string eventJson)
        {
            throw new NotImplementedException();
        }

        public bool Update(ExpenseModelBase model)
        {
            throw new NotImplementedException();
        }
    }
}
