using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.IServices
{
    public interface IServiceBase<TEntity, TKey>
        where TEntity : class
    {
        IEnumerable<TEntity> GetAll();
        TEntity GetById(TKey id);
        TEntity Create(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        bool SaveChanges();
    }
}
