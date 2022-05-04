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
        /// Get all elements of type <typeparamref name="TEntity"/> from the context
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/> where T is of type <typeparamref name="TEntity"/></returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// Return an element of the context by its ID
        /// </summary>
        /// <param name="id">ID of type <typeparamref name="TKey"/></param>
        /// <returns>An object of type <typeparamref name="TEntity"/> if any exists. Otherwise return null</returns>
        TEntity GetById(TKey id);

        /// <summary>
        /// Attach a new element of type <typeparamref name="TEntity"/> to the context
        /// </summary>
        /// <param name="entity">Entity of type <typeparamref name="TEntity"/></param>
        void Create(TEntity entity);

        /// <summary>
        /// Add an existing modified element to the context
        /// </summary>
        /// <param name="entity">Entity of type <typeparamref name="TEntity"/></param>
        void Update(TEntity entity);

        /// <summary>
        /// Set an entity from the context as deleted. After the <see cref="SaveChanges()"/> the given entity in parameter
        /// is deleted from the database
        /// </summary>
        /// <param name="entity">Entity of type <typeparamref name="TEntity"/></param>
        void Delete(TEntity entity);

        /// <summary>
        /// Save all the changes in the context to the database
        /// </summary>
        /// <returns>true if more than 0 changes occured. false Otherwise</returns>
        bool SaveChanges();
    }
}
