using LendingPlatform.DomainModel.Models.EntityInfo;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LendingPlatform.DomainModel.DataRepository
{
    public interface IDataRepository
    {

        /// <summary>
        /// Method fetches the IQueryable based on expression.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IQueryable<T> Fetch<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// Method fetches the first or default item from the datacontext based on the the supplied function.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// Method fetches the single or default item from the datacontext based on the the supplied function.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<T> SingleOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// Method fetches the single item from the datacontext based on the the supplied function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<T> SingleAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// Method fetches the last or default item from the datacontext based on the the supplied function.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<T> LastOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class;


        /// <summary>
        /// DB Add record method, call SaveChangesAsync after this
        /// </summary>
        /// <param name="entity"></param>
        Task AddAsync<T>(T entity) where T : class;

        /// <summary>
        /// DB check if has any records
        /// </summary>
        Task<bool> AnyAsync<T>() where T : class;

        /// <summary>
        /// DB Add multiple records, call SaveChangesAsync after this
        /// </summary>
        /// <param name="entityList"></param>
        Task AddRangeAsync<T>(IEnumerable<T> entityList) where T : class;

        /// <summary>
        /// DB Update record method, call SaveChangesAsync after this
        /// </summary>
        /// <param name="entity"></param>
        void Update<T>(T entity) where T : class;


        /// <summary>
        /// DB Update multiple records, call SaveChangesAsync after this
        /// </summary>
        /// <param name="entityList"></param>
        void UpdateRange<T>(IEnumerable<T> entityList) where T : class;

        /// <summary>
        /// DB Delete record method, call SaveChangesAsync after this
        /// </summary>
        /// <param name="entity"></param>
        void Remove<T>(T entity) where T : class;

        /// <summary>
        /// Method deletes a range of entities from the datacontext
        /// </summary>
        /// <param name="entities"></param>
        void RemoveRange<T>(IEnumerable<T> entities) where T : class;


        /// <summary>
        /// Saves and add audit logs for all changes made in this context to the database.
        /// </summary>
        /// <param name="auditLog">Audit log object.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(AuditLog auditLog = null);
        /// <summary>
        /// Method that begin transaction, call CommitAsync to commit the changes
        /// </summary>
        /// <returns></returns>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Method that commits database transaction
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Method fetches IQueryable with all table data
        /// Only use if there is requirement of all data, don't use when there happens to be any condition to filter the data
        /// </summary>
        /// <returns></returns>
        IQueryable<T> GetAll<T>() where T : class;

        /// <summary>
        /// Untrack queryables and reset properties like Id, Version and SurrogateId
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="sanitizeIdentifiers"></param>
        /// <returns></returns>
        List<T> DetachEntities<T>(IQueryable<T> entities, bool sanitizeIdentifiers = true) where T : class;

        /// <summary>
        /// Enable auto detect changes
        /// </summary>
        void EnableChangeTracking();

        /// <summary>
        /// Disable auto detect changes
        /// </summary>
        void DisableChangeTracking();

    }
}
