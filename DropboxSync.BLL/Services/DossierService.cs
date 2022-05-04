using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Services
{
    public class DossierService : IDossierService
    {
        private readonly DropboxSyncContext _context;

        public DossierService(DropboxSyncContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public void Create(DossierEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Dossiers.Add(entity);
        }

        public void Delete(DossierEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Dossiers.Remove(entity);
        }

        public IEnumerable<DossierEntity> GetAll()
        {
            return _context.Dossiers.ToList();
        }

        public DossierEntity GetById(Guid id)
        {
            DossierEntity? entity = _context.Dossiers.Find(id);

            return entity;
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public void Update(DossierEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            _context.Dossiers.Update(entity);
        }
    }
}
