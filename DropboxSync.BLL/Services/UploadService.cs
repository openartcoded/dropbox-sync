using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
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
