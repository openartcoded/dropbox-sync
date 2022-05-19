using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Services
{
    public class UploadService : IUploadService
    {
        private readonly DropboxSyncContext _context;

        public UploadService(DropboxSyncContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public void Create(UploadEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Uploads.Add(entity);
        }

        public void Delete(UploadEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Uploads.Remove(entity);
        }

        public IEnumerable<UploadEntity> GetAll()
        {
            return _context.Uploads.ToList();
        }

        public UploadEntity? GetById(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException($"{nameof(id)} cannot be empty guid!");

            return _context.Uploads.Find(id);
        }

        public UploadEntity? GetByUploadId(string uploadId)
        {
            if (string.IsNullOrEmpty(uploadId)) throw new ArgumentNullException(nameof(uploadId));

            return _context.Uploads.SingleOrDefault(u => u.UploadId.Equals(uploadId));
        }

        public UploadEntity? GetInvoiceRelatedUpload(Guid invoiceId)
        {
            InvoiceEntity? invoiceFromRepo = _context.Invoices.Find(invoiceId);

            if (invoiceFromRepo is null) return null;

            UploadEntity? uploadFromRepo = _context.Uploads.SingleOrDefault(u => u.Id == invoiceFromRepo.UploadId);

            return uploadFromRepo;
        }

        public IEnumerable<UploadEntity>? GetExpenseRelatedUploads(Guid expenseId)
        {
            ExpenseEntity? expenseFromRepo = _context.Expenses
                .Include(e => e.Uploads)
                .SingleOrDefault(e => e.Id == expenseId);

            if (expenseFromRepo is null) return null;

            return expenseFromRepo.Uploads;
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public void Update(UploadEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Uploads.Update(entity);
        }
    }
}
