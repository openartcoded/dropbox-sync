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
        /// <summary>
        /// Get all elements of type <see cref="TEntity"/> from the context
        /// </summary>
        /// <returns><see cref="IEnumerable{TEntity}"/></returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// Return an element of the context by its ID
        /// </summary>
        /// <param name="id">ID of type <see cref="TKey"/></param>
        /// <returns>An object of type <see cref="TEntity"/> if any exist. Otherwise return null</returns>
        TEntity GetById(TKey id);

        /// <summary>
        /// Attach a new element of type <see cref="TEntity"/> to the context
        /// </summary>
        /// <param name="entity">Entity of type <see cref="TEntity"/></param>
        void Create(TEntity entity);

        /// <summary>
        /// Add an existing modified element to the context
        /// </summary>
        /// <param name="entity">Entity of type <see cref="TEntity"/></param>
        void Update(TEntity entity);

        /// <summary>
        /// Set an entity from the context as deleted. After the <see cref="SaveChanges()"/> the given entity in parameter
        /// is deleted from the database
        /// </summary>
        /// <param name="entity">Entity of type <see cref="TEntity"/></param>
        void Delete(TEntity entity);

        /// <summary>
        /// Save all the changes in the context to the database
        /// </summary>
        /// <returns>true if more than 0 changes occured. false Otherwise</returns>
        bool SaveChanges();
    }
}
