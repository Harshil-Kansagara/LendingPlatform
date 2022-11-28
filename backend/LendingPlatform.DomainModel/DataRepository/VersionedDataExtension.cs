using LendingPlatform.DomainModel.Models.EntityInfo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LendingPlatform.DomainModel.DataRepository
{
    public static class VersionedDataExtension
    {
        /// <summary>
        /// Helper that fetches all entries of latest version 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public static IQueryable<T> GetLatestVersionForLoan<T>(this IQueryable<T> queryable)
        {
            var orderedDataSet = queryable.AsEnumerable().OrderByDescending(x => x.GetType().GetProperty("SurrogateId").GetValue(x, null)).FirstOrDefault();
            if (orderedDataSet == null)
            {
                return queryable;
            }

            if (typeof(T).Name == nameof(EntityFinance))
            {
                var finances = ((IQueryable<EntityFinance>)queryable).Where(x => x.Version == (Guid)orderedDataSet.GetType().GetProperty("Version").GetValue(orderedDataSet));
                return (IQueryable<T>)finances;
            }
            else if (typeof(T).Name == nameof(EntityTaxForm))
            {
                var taxes = ((IQueryable<EntityTaxForm>)queryable).Where(x => x.Version == (Guid)orderedDataSet.GetType().GetProperty("Version").GetValue(orderedDataSet));
                return (IQueryable<T>)taxes;
            }
            else if (typeof(T).Name == nameof(CreditReport))
            {
                var creditReport = ((IQueryable<CreditReport>)queryable).Where(x => x.Version == (Guid)orderedDataSet.GetType().GetProperty("Version").GetValue(orderedDataSet));
                return (IQueryable<T>)creditReport;
            }

            return queryable;
        }

        /// <summary>
        /// Helper that versions an unversioned queryable and sets version, loanid and surrogateid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="loanId"></param>
        /// <returns></returns>
        public static IQueryable<T> VersionThisQueryable<T>(this IQueryable<T> queryable, Guid loanId)
        {
            Guid version = Guid.NewGuid();


            foreach (var query in queryable)
            {
                query.GetType().GetProperty("Version").SetValue(query, version);
                query.GetType().GetProperty("LoanApplicationId").SetValue(query, loanId);
            }
            return queryable;
        }

        /// <summary>
        /// Helper that sanitize identifiers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clonedObject"></param>
        /// <returns></returns>
        public static List<T> SanitizeIdentifiers<T>(this List<T> clonedObject)
        {
            foreach (var entity in clonedObject)
            {
                entity.GetType().GetProperty("Id").SetValue(entity, Guid.Empty);

                if (entity.GetType().GetProperty("Version") != null)
                    entity.GetType().GetProperty("Version").SetValue(entity, null);

                if (entity.GetType().GetProperty("LoanApplicationId") != null)
                    entity.GetType().GetProperty("LoanApplicationId").SetValue(entity, null);

                if (entity.GetType().GetProperty("CreatedOn") != null)
                    entity.GetType().GetProperty("CreatedOn").SetValue(entity, DateTime.UtcNow);
            }
            return clonedObject;
        }

    }
}
