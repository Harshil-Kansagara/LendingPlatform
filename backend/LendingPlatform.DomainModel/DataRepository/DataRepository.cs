using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Utils.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LendingPlatform.DomainModel.DataRepository
{
    public class DataRepository : IDataRepository
    {

        private readonly LendingPlatformContext _context;
        private readonly IDbConnection _dbConnection;

        public DataRepository(LendingPlatformContext context)
        {
            _context = context;
            _dbConnection = _context.Database.GetDbConnection();
        }

        /// <summary>
        /// Method fetches the IQueryable based on expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryable<T> Fetch<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return _context.Set<T>().Where<T>(predicate);
        }

        /// <summary>
        /// Method fetches the first or default item from the datacontext based on the the supplied function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Method fetches the first item from the datacontext based on the the supplied function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _context.Set<T>().FirstAsync(predicate);
        }

        /// <summary>
        /// Method fetches the single or default item from the datacontext based on the the supplied function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<T> SingleOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _context.Set<T>().SingleOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Method fetches the single item from the datacontext based on the the supplied function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<T> SingleAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _context.Set<T>().SingleAsync(predicate);
        }

        /// <summary>
        /// Method fetches the last or default item from the datacontext based on the the supplied function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<T> LastOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await _context.Set<T>().LastOrDefaultAsync(predicate);
        }


        /// <summary>
        /// DB Add record method, call SaveChangesAsync after this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task AddAsync<T>(T entity) where T : class
        {
            await _context.Set<T>().AddAsync(entity);
        }

        /// <summary>
        /// DB check if has any records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<bool> AnyAsync<T>() where T : class
        {
            return await _context.Set<T>().AnyAsync();
        }

        /// <summary>
        /// DB Add multiple records, call SaveChangesAsync after this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public async Task AddRangeAsync<T>(IEnumerable<T> entityList) where T : class
        {
            await _context.Set<T>().AddRangeAsync(entityList);
        }

        /// <summary>
        /// DB update records, call SaveChangesAsync after this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Update<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
        }

        /// <summary>
        /// DB Update multiple records, call SaveChangesAsync after this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityList"></param>
        public void UpdateRange<T>(IEnumerable<T> entityList) where T : class
        {
            _context.Set<T>().UpdateRange(entityList);
        }

        /// <summary>
        /// DB Delete record method, call SaveChangesAsync after this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Remove<T>(T entity) where T : class
        {
            _context.Set<T>().Remove(entity);
        }

        /// <summary>
        /// DB delete multiple record method, call SaveChangesAsync after this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void RemoveRange<T>(IEnumerable<T> entities) where T : class
        {
            _context.Set<T>().RemoveRange(entities);

        }

        /// <summary>
        /// Saves and add audit logs for all changes made in this context to the database.
        /// </summary>
        /// <param name="auditLog">Audit log object.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public async Task<int> SaveChangesAsync(AuditLog auditLog = null)
        {
            if (auditLog != null)
                _context.AddAuditCustomField(StringConstant.AuditLogCustomFieldName, auditLog);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Method that begin transaction, call CommitAsync to commit the changes
        /// </summary>
        /// <returns></returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Method to commit database transaction.
        /// </summary>
        public void CommitTransaction()
        {
            _context.Database.CommitTransaction();
        }

        /// <summary>
        /// Method fetches IQueryable with all table data. 
        /// Only use if there is requirement of all data, don't use when there happens to be any condition to filter the data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> GetAll<T>() where T : class
        {
            return _context.Set<T>();
        }

        /// <summary>
        /// Untrack queryables and reset properties like Id, Version and SurrogateId
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="sanitizeIdentifiers"></param>
        /// <returns></returns>
        public List<T> DetachEntities<T>(IQueryable<T> entities, bool sanitizeIdentifiers = true) where T : class
        {

            var clonedObject = entities.AsNoTracking().ToList();

            if (sanitizeIdentifiers)
            {
                clonedObject.SanitizeIdentifiers();
            }

            return clonedObject;
        }

        /// <summary>
        /// Enable auto detect changes
        /// </summary>
        public void EnableChangeTracking()
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        /// <summary>
        /// Disable auto detect changes
        /// </summary>
        public void DisableChangeTracking()
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        ~DataRepository()
        {
            _dbConnection.Close();
            _dbConnection.Dispose();
        }


    }

}
