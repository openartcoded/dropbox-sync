using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly DropboxSyncContext _context;

        public InvoiceService(DropboxSyncContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public void Create(InvoiceEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Invoices.Add(entity);
        }

        public void Delete(InvoiceEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Invoices.Remove(entity);
        }

        public IEnumerable<InvoiceEntity> GetAll()
        {
            return _context.Invoices.ToList();
        }

        public InvoiceEntity? GetById(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException($"{nameof(id)} cannot be empty guid!");

            return _context.Invoices.Find(id);
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public void Update(InvoiceEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Invoices.Update(entity);
        }
    }
}
