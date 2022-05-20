using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly DropboxSyncContext _context;

        public DocumentService(DropboxSyncContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public void Create(DocumentEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Documents.Add(entity);
        }

        public void Delete(DocumentEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Documents.Remove(entity);
        }

        public IEnumerable<DocumentEntity> GetAll()
        {
            return _context.Documents.ToList();
        }

        public DocumentEntity? GetById(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));

            return _context.Documents.Find(id);
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public void Update(DocumentEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Documents.Update(entity);
        }
    }
}
