using Audit.EntityFramework;
using AutoMapper;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.Product;
using LendingPlatform.Utils.ApplicationClass.TaxForm;
using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
using SmartyStreets.USStreetApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SubLoanPurposeAC = LendingPlatform.Repository.ApplicationClass.Applications.SubLoanPurposeAC;
using SubLoanPurposeUtilsAC = LendingPlatform.Utils.ApplicationClass.SubLoanPurposeAC;
namespace LendingPlatform.Repository.AutoMapper
{
    public class MappingProfile : Profile
    {
        private readonly Regex isGuid =
                            new Regex(StringConstant.GuidValidationPatternRegex, RegexOptions.Compiled);
        public MappingProfile()
        {
            CreateMap<LoanApplication, ProductRuleAC>()
                .ForMember(dest => dest.SubLoanPurposeName,
                            opt => opt.MapFrom(src => src.LoanPurpose.Name))
                .ForMember(dest => dest.SubLoanPurposeName,
                            opt => opt.MapFrom(src => src.SubLoanPurpose.Name));
            CreateMap<ProductRangeTypeMapping, ProductRangeTypeMappingAC>()
                .ForMember(dest => dest.RangeTypeName,
                            opt => opt.MapFrom(src => src.LoanRangeType.Name));
            CreateMap<ProductSubPurposeMapping, ProductSubPurposeMappingAC>()
                .ForMember(dest => dest.SubPurposeName,
                            opt => opt.MapFrom(src => src.SubLoanPurpose.Name));

            CreateMap<TaxAC, EntityTaxForm>().ReverseMap();
            CreateMap<DescriptionPointSeedAC, DescriptionPoint>().ReverseMap();
            CreateMap<DocumentAC, Document>().ReverseMap();
            CreateMap<EntityTaxYearlyMapping, EntityTaxAccountAC>();
            CreateMap<TaxForm, TaxFormAC>();
            CreateMap<Product, RecommendedProductAC>();
            CreateMap<CompanyStructure, CompanyStructureAC>();
            CreateMap<AddressAC, Address>()
                .ForMember(dest =>
                dest.Response,
                opt => opt.MapFrom(src => src.AddressJson)).ReverseMap();
            CreateMap<UserAC, User>().ReverseMap();
            CreateMap<CompanyAC, Company>()
                .ForMember(dest =>
                dest.CompanySizeId,
                opt => opt.MapFrom(src => src.CompanySize.Id))
                .ForMember(dest =>
                dest.BusinessAgeId,
                opt => opt.MapFrom(src => src.BusinessAge.Id))
                .ForMember(dest =>
                dest.NAICSIndustryTypeId,
                opt => opt.MapFrom(src => src.IndustryType.Id))
                .ForMember(dest =>
                dest.IndustryExperienceId,
                opt => opt.MapFrom(src => src.IndustryExperience.Id))
                .ForMember(dest =>
                dest.CompanyStructureId,
                opt => opt.MapFrom(src => src.CompanyStructure.Id))
                .ForMember(dest =>
                dest.CompanyStructure,
                opt => opt.Ignore())
                .ForMember(dest =>
                dest.CompanySize,
                opt => opt.Ignore())
                .ForMember(dest =>
                dest.BusinessAge,
                opt => opt.Ignore())
                .ForMember(dest =>
                dest.IndustryExperience,
                opt => opt.Ignore())
                .ForMember(dest =>
                dest.NAICSIndustryType,
                opt => opt.Ignore())
                .ReverseMap();


            CreateMap<string, BusinessAge>()
                .ForMember(dest =>
                dest.Age,
                opt => opt.MapFrom(src => src));

            CreateMap<string, CompanySize>()
                .ForMember(dest =>
                dest.Size,
                opt => opt.MapFrom(src => src));

            CreateMap<string, Relationship>()
                .ForMember(dest =>
                dest.Relation,
                opt => opt.MapFrom(src => src));


            CreateMap<User, Entity>()
                .ForMember(dest =>
                dest.Type,
                opt => opt.MapFrom(src => EntityType.User));

            CreateMap<string, LoanPurpose>()
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src));

            CreateMap<Candidate, AddressAC>()
                .ForMember(dest =>
                    dest.PrimaryNumber,
                    opt => opt.MapFrom(src => src.Components.PrimaryNumber))
                .ForMember(dest =>
                    dest.StreetLine,
                    opt => opt.MapFrom(src => src.Components.StreetName))
                .ForMember(dest =>
                    dest.City,
                    opt => opt.MapFrom(src => src.Components.CityName))
                .ForMember(dest =>
                    dest.StateAbbreviation,
                    opt => opt.MapFrom(src => src.Components.State))
                .ForMember(dest =>
                    dest.StreetSuffix,
                    opt => opt.MapFrom(src => src.Components.StreetSuffix))
                .ForMember(dest =>
                    dest.SecondaryNumber,
                    opt => opt.MapFrom(src => src.Components.SecondaryNumber))
                .ForMember(dest =>
                    dest.SecondaryDesignator,
                    opt => opt.MapFrom(src => src.Components.SecondaryDesignator))
                .ForMember(dest =>
                    dest.ZipCode,
                    opt => opt.MapFrom(src => src.Components.ZipCode));

            CreateMap<Bank, BankAC>().ReverseMap();

            CreateMap<LoanApplication, ApplicationAC>();
            CreateMap<LoanApplication, LoanEntityBankDetailsAC>()
            .ForMember(dest =>
                dest.LoanApplicationId,
                opt => opt.MapFrom(src => src.Id))
            .ForPath(dest =>
                dest.EMIDeducteeBank.BankId,
                opt => opt.MapFrom(src => src.EMIDeducteeBank.Bank.Id))
            .ForPath(dest =>
                dest.EMIDeducteeBank.BankName,
                opt => opt.MapFrom(src => src.EMIDeducteeBank.Bank.Name))
            .ForPath(dest =>
                dest.EMIDeducteeBank.AccountNumber,
                opt => opt.MapFrom(src => src.EMIDeducteeBank.AccountNumber))
            .ForPath(dest =>
                dest.EMIDeducteeBank.SWIFTCode,
                opt => opt.MapFrom(src => src.EMIDeducteeBank.Bank.SWIFTCode))
            .ForPath(dest =>
                dest.LoanAmountDepositeeBank.BankId,
                opt => opt.MapFrom(src => src.LoanAmountDepositeeBank.Bank.Id))
            .ForPath(dest =>
                dest.LoanAmountDepositeeBank.BankName,
                opt => opt.MapFrom(src => src.LoanAmountDepositeeBank.Bank.Name))
            .ForPath(dest =>
                dest.LoanAmountDepositeeBank.AccountNumber,
                opt => opt.MapFrom(src => src.LoanAmountDepositeeBank.AccountNumber))
            .ForPath(dest =>
                dest.LoanAmountDepositeeBank.SWIFTCode,
                opt => opt.MapFrom(src => src.LoanAmountDepositeeBank.Bank.SWIFTCode));

            // New implementation
            CreateMap<ApplicationBasicDetailAC, LoanApplication>()
                .ForMember(dest =>
                    dest.CreatedOn,
                    opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest =>
                    dest.EntityId,
                    opt => opt.MapFrom(src => src.CreatedByUserId))
                .ForMember(dest =>
                    dest.CreatedOn,
                    opt => opt.MapFrom(src => DateTime.SpecifyKind(src.CreatedOn, DateTimeKind.Utc)))
                .ForMember(dest =>
                    dest.UpdatedOn,
                    opt => opt.MapFrom(src => src.UpdatedOn.HasValue ? DateTime.SpecifyKind(src.UpdatedOn.Value, DateTimeKind.Utc) : src.UpdatedOn));


            CreateMap<Consent, ConsentStatementAC>();

            CreateMap<Bank, BankAC>().ReverseMap();

            CreateMap<LoanPurpose, ApplicationClass.Others.LoanPurposeAC>();

            CreateMap<Entity, EntityAC>();

            CreateMap<Section, SectionAC>()
                .ReverseMap()
                .ForMember(dest =>
                dest.Id,
                opt => opt.Ignore());


            CreateMap<EntityLoanApplicationConsent, ConsentAC>()
                 .ForMember(dest =>
                    dest.ConsentText,
                    opt => opt.MapFrom(src => src.Consent.ConsentText))
                .ForMember(dest =>
                    dest.UserId,
                    opt => opt.MapFrom(src => src.Entity.Id))
                .ForMember(dest =>
                    dest.ConsentId,
                    opt => opt.MapFrom(src => src.ConsentId))
                .ForMember(dest =>
                    dest.IsConsentGiven,
                    opt => opt.MapFrom(src => src.IsConsentGiven));

            CreateMap<User, CurrentUserAC>();
            CreateMap<BankUser, CurrentUserAC>();



            CreateMap<IntegratedServiceConfiguration, IntegratedServiceConfigurationSeedAC>();

            CreateMap<IConfigurationSection, AppSettingAC>()
                .ForMember(dest =>
                    dest.FieldName,
                    opt => opt.MapFrom(src => src.Path))
                .ForMember(dest =>
                    dest.Value,
                    opt => opt.MapFrom(src => src.Value));




            CreateMap<CreditReport, CreditReportAC>()
                .ForMember(dest =>
                    dest.CreditReportJson,
                    opt => opt.MapFrom(src => src.Response));


            #region Finance Section
            CreateMap<EntityFinanceYearlyMapping, PeriodicFinancialAccountsAC>()
                .ForMember(dest =>
                dest.Period,
                opt => opt.MapFrom(src => src.Period.Substring(src.Period.Length - 4)))
                .ForMember(dest =>
                dest.FinancialAccountBalances,
                opt => opt.MapFrom(src => src.EntityFinanceStandardAccounts))
                .ForMember(dest =>
                dest.ReportName,
                opt => opt.MapFrom(src => src.EntityFinance.FinancialStatement.Name));

            CreateMap<EntityFinanceStandardAccount, FinancialAccountBalanceAC>()
                .ForMember(dest =>
                dest.Account,
                opt => opt.MapFrom(src => src.Name))
                .ForMember(dest =>
                dest.Id,
                opt => opt.Ignore())
                .ReverseMap();


            CreateMap<EntityFinance, CompanyFinanceAC>()
                .ForMember(dest =>
                    dest.FinancialJson,
                    opt => opt.MapFrom(src => src.FinancialInformationJson))
                .ForMember(dest =>
                    dest.FinancialStatement,
                    opt => opt.MapFrom(src => src.FinancialStatement == null ? null : src.FinancialStatement.Name))
                .ForMember(dest =>
                dest.ThirdPartyServiceName,
                opt => opt.MapFrom(src => src.IntegratedServiceConfiguration == null ? null : src.IntegratedServiceConfiguration.Name))
                .ForMember(dest =>
                dest.FinancialAccounts,
                opt => opt.MapFrom(src => src.EntityFinanceYearlyMappings))
                .ForMember(dest => dest.FinancialAccounts,
                opt => opt.AddTransform(x => x.OrderBy(y => y.Period).ToList()))
                .ReverseMap();


            CreateMap<List<EntityFinance>, List<CompanyFinanceAC>>()
                .ConvertUsing<MapperActions>();
            #endregion


            CreateMap<List<EntityTaxForm>, List<TaxAC>>().ConvertUsing<MapperActions>();

            CreateMap<BankUser, UserAC>()
                .ForMember(dest =>
                    dest.FirstName,
                    opt => opt.MapFrom(src => src.Name));

            CreateMap<LoanPurposeRangeTypeMapping, LoanPurposeRangeTypeMappingAC>()
                .ForMember(dest => dest.RangeTypeName,
                            opt => opt.MapFrom(src => src.LoanRangeType == null ? null : src.LoanRangeType.Name));

            CreateMap<AuditLog, AuditLogAC>()
                .ForMember(dest =>
                    dest.UserName,
                    opt => opt.MapFrom(src => GetUserName(src.CreatedByUser, src.CreatedByBankUser)))
                .ForMember(dest =>
                    dest.Email,
                    opt => opt.MapFrom(src => GetUserEmail(src.CreatedByUser, src.CreatedByBankUser)))
                .ForMember(dest =>
                    dest.CreatedOn,
                    opt => opt.MapFrom(src => DateTime.SpecifyKind(src.CreatedOn.AddMilliseconds(-src.CreatedOn.Millisecond), DateTimeKind.Utc)));

            CreateMap<EventEntryChange, AuditLogFieldAC>()
                .ForMember(dest => dest.IsGuid,
                            opt => opt.MapFrom(src => src.NewValue != null && isGuid.IsMatch(src.NewValue.ToString())));

            CreateMap<OCRExtractedValueAC, TaxFormValueLabelMappingAC>();
            CreateMap<TaxFormValueLabelMappingAC, TaxFormValueLabelMapping>();
            CreateMap<TaxFormValueLabelMapping, TaxFormValueLabelMappingAC>()
                .ForMember(dest => dest.Label,
                            opt => opt.MapFrom(src => src.TaxformLabelNameMapping.LabelFieldName));
            CreateMap<SubLoanPurposeUtilsAC, SubLoanPurpose>()
                .ForMember(dest => dest.Id,
                opt => opt.Ignore())
                .ForMember(dest => dest.LoanPurposeId,
                opt => opt.Ignore());
            CreateMap<SubLoanPurpose, SubLoanPurposeAC>();

            //Personal Finances
            CreateMap<PersonalFinanceAccount, PersonalFinanceAccountAC>()
                .ForMember(dest =>
                    dest.Categories,
                    opt => opt.Ignore());

            CreateMap<PersonalFinanceCategory, PersonalFinanceCategoryAC>()
                .ForMember(dest =>
                    dest.ChildCategories,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.ParentAttribute,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.Attributes,
                    opt => opt.Ignore());

            CreateMap<PersonalFinanceAttribute, PersonalFinanceAttributeAC>();

            CreateMap<PersonalFinanceConstant, PersonalFinanceConstantAC>()
                .ForMember(dest =>
                    dest.Options,
                    opt => opt.Ignore());

            CreateMap<PersonalFinanceConstantOptionSeedDataAC, PersonalFinanceConstantOptionAC>();

            CreateMap<AdditionalDocumentType, AdditionalDocumentTypeAC>()
                .ReverseMap();

            CreateMap<EntityAdditionalDocument, AdditionalDocumentAC>()
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.DocumentType,
                    opt => opt.MapFrom(src => src.AdditionalDocumentType))
                .ForMember(dest =>
                    dest.Document,
                    opt => opt.MapFrom(src => src.Document))
                .ReverseMap();
        }

        /// <summary>
        /// Get the username from the user object.
        /// </summary>
        /// <param name="createdByUser">User object</param>
        /// <param name="createdByBankUser">Bank user object.</param>
        /// <returns></returns>
        private string GetUserName(User createdByUser, BankUser createdByBankUser)
        {
            if (createdByUser != null)
            {
                return $"{createdByUser.FirstName} {createdByUser.LastName}";
            }
            else
            {
                return createdByBankUser != null ? createdByBankUser.Name : string.Empty;
            }
        }
        /// <summary>
        /// Get the Email from the user object.
        /// </summary>
        /// <param name="createdByUser">User object</param>
        /// <param name="createdByBankUser">Bank user object.</param>
        /// <returns></returns>
        private string GetUserEmail(User createdByUser, BankUser createdByBankUser)
        {
            if (createdByUser != null)
            {
                return createdByUser.Email;
            }
            else
            {
                return createdByBankUser != null ? createdByBankUser.Email : string.Empty;
            }
        }

    }
}
