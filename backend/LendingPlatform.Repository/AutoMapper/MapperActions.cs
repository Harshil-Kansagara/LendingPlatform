using AutoMapper;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.Repository.EntityInfo;
using System.Collections.Generic;

namespace LendingPlatform.Repository.AutoMapper
{
    public class MapperActions : ITypeConverter<List<EntityFinance>, List<CompanyFinanceAC>>, ITypeConverter<List<EntityTaxForm>, List<TaxAC>>
    {
        private readonly IEntityFinanceRepository _entityFinanceRepository;
        private readonly IEntityTaxReturnRepository _entityTaxReturnRepository;

        public MapperActions(IEntityFinanceRepository entityFinanceRepository, IEntityTaxReturnRepository entityTaxReturnRepository)
        {
            _entityFinanceRepository = entityFinanceRepository;
            _entityTaxReturnRepository = entityTaxReturnRepository;
        }

        /// <summary>
        /// Map entityfinance list to financeac list
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public List<CompanyFinanceAC> Convert(List<EntityFinance> source, List<CompanyFinanceAC> destination, ResolutionContext context)
        {
            destination = new List<CompanyFinanceAC>();
            foreach (var entityFinance in source)
                destination.Add(context.Mapper.Map<CompanyFinanceAC>(entityFinance));
            return _entityFinanceRepository.GetStandardAccountsList(destination, source);
        }

        /// <summary>
        /// Map entityTaxForm list to taxAC list
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public List<TaxAC> Convert(List<EntityTaxForm> source, List<TaxAC> destination, ResolutionContext context)
        {
            destination = new List<TaxAC>();
            return _entityTaxReturnRepository.GetTaxes(destination, source);
        }
    }
}
