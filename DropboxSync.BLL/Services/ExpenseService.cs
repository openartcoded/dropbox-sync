using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly DropboxSyncContext _context;

        public ExpenseService(DropboxSyncContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public void Create(ExpenseEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Expenses.Add(entity);
        }

        public void Delete(ExpenseEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Expenses.Remove(entity);
        }

        public IEnumerable<ExpenseEntity> GetAll()
        {
            return _context.Expenses.ToList();
        }

        public ExpenseEntity? GetById(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException($"{nameof(id)} cannot be an empty guid!");

            return _context.Expenses.Find(id);
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public void Update(ExpenseEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Expenses.Update(entity);
        }
    }
}
