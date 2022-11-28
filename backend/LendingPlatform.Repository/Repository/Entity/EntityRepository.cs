using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using LendingPlatform.Utils.Utils.Transunion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.EntityInfo
{
    public class EntityRepository : IEntityRepository
    {

        #region Private variables
        private readonly IDataRepository _dataRepository;
        private readonly IConfiguration _configuration;
        private readonly IGlobalRepository _globalRepository;
        private readonly IMapper _mapper;
        private readonly ISmartyStreetsUtility _smartyStreetsUtility;
        private readonly IFileOperationsUtility _fileOperationsUtility;
        private readonly IExperianUtility _experianUtility;
        private readonly ITransunionUtility _transunionUtility;
        private readonly IEquifaxUtility _equifaxUtility;
        private readonly IAmazonServicesUtility _amazonS3Utility;
        #endregion

        #region Constructor
        public EntityRepository(IDataRepository dataRepository, IConfiguration configuration,
            IGlobalRepository globalRepository, IMapper mapper,
            ISmartyStreetsUtility smartyStreetsUtility,
            IFileOperationsUtility fileOperationsUtility,
            IExperianUtility experianUtility,
            ITransunionUtility transunionUtility,
            IEquifaxUtility equifaxUtility,
            IAmazonServicesUtility amazonS3Utility)
        {
            _dataRepository = dataRepository;
            _configuration = configuration;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _smartyStreetsUtility = smartyStreetsUtility;
            _mapper = mapper;
            _fileOperationsUtility = fileOperationsUtility;
            _experianUtility = experianUtility;
            _transunionUtility = transunionUtility;
            _equifaxUtility = equifaxUtility;
            _amazonS3Utility = amazonS3Utility;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get valid address.
        /// </summary>
        /// <param name="address">address Object</param>
        /// <returns>returns smarty streets validated address</returns>
        private async Task<SmartyStreets.USStreetApi.Candidate> GetValidatedAddressAsync(AddressAC address)
        {
            IntegratedServiceConfiguration smartyStreetsService = await _dataRepository.Fetch<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.SmartyStreets)).SingleAsync();
            var validAddress = _smartyStreetsUtility.GetValidatedAddress(address, smartyStreetsService.ConfigurationJson);
            if (validAddress == null)
            {
                throw new ValidationException(StringConstant.InvalidAddress);
            }
            return validAddress;
        }

        /// <summary>
        /// Add entity relation mapping of sole proprietor.
        /// </summary>
        /// <param name="primaryEntityId">Primary enitity Id</param>
        /// <param name="relativeEntityId">Relative entity Id</param>
        /// <returns></returns>
        private async Task AddEntityRelationMapping(Guid primaryEntityId, Guid relativeEntityId)
        {
            Guid proprietorRelationId = await _dataRepository.Fetch<Relationship>(x => x.Relation.Equals(StringConstant.Proprietor)).Select(x => x.Id).SingleAsync();

            //Add Single linked entity as sole proprietor
            EntityRelationshipMapping entityRelationshipMapping = new EntityRelationshipMapping
            {
                PrimaryEntityId = primaryEntityId,
                RelativeEntityId = relativeEntityId,
                RelationshipId = proprietorRelationId,
                SharePercentage = 100
            };

            await _dataRepository.AddAsync<EntityRelationshipMapping>(entityRelationshipMapping);
        }

        /// <summary>
        /// Check the user self declared credit profile information.
        /// </summary>
        /// <param name="user">User Object</param>
        /// <returns>return true or false</returns>
        private bool CheckUserCreditProfileInformation(User user)
        {
            bool isAllowAnyJudgements = _configuration.GetValue<bool>("UserSelfDeclarationExpectedResponse:IsAllowAnyJudgements");
            bool isAllowBankruptcy = _configuration.GetValue<bool>("UserSelfDeclarationExpectedResponse:IsAllowBankruptcy");
            int minCreditScore = _configuration.GetValue<int>("UserSelfDeclarationExpectedResponse:MinCreditScore");

            // Check Credit score if fail then return false.
            if (!string.IsNullOrEmpty(user.SelfDeclaredCreditScore))
            {
                if (user.SelfDeclaredCreditScore.Contains("<=") && minCreditScore >= int.Parse(user.SelfDeclaredCreditScore.Split('=')[1]))
                {
                    return false;
                }
                else if (user.SelfDeclaredCreditScore.Contains("-") && minCreditScore > int.Parse(user.SelfDeclaredCreditScore.Split('-')[1]))
                {
                    return false;
                }

                // Check jugements and bankruptcy and return value based on criteria.
                return (!user.HasAnyJudgementsSelfDeclared || user.HasAnyJudgementsSelfDeclared == isAllowAnyJudgements) &&
                       (!user.HasBankruptcySelfDeclared || user.HasBankruptcySelfDeclared == isAllowBankruptcy);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method fetches loan applications with basic details for given entity id.
        /// </summary>
        /// <param name="entityId">Entity id</param>
        /// <param name="currentUser">Current user</param>
        /// <param name="isOnlyUserInitiatedLoansRequired">Required all loans or only created by user</param>
        /// <returns>List of type LoanApplicationBasicDetailAC</returns>
        private async Task<List<ApplicationBasicDetailAC>> GetLoanApplicationsWithBasicDetailsAsync(Guid entityId, CurrentUserAC currentUser, bool isOnlyUserInitiatedLoansRequired = false)
        {
            List<EntityLoanApplicationMapping> loanApplicationList;
            if (entityId == currentUser.Id)
            {
                return await GetCurrentUserLoanApplicationsWithBasicDetailsAsync(entityId, isOnlyUserInitiatedLoansRequired);
            }
            else
            {
                if (!await _globalRepository.CheckEntityRelationshipMappingAsync(entityId, currentUser.Id))
                {
                    throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
                }
                loanApplicationList = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => x.EntityId == entityId)
                    .Include(i => i.LoanApplication.UserLoanSectionMappings).ThenInclude(y => y.Section).Include(i => i.LoanApplication.EMIDeducteeBank.Bank).Include(i => i.LoanApplication.LoanAmountDepositeeBank.Bank).ToListAsync();

                // If no any loan application is linked with entity then return empty list.
                if (loanApplicationList.Any())
                {
                    return loanApplicationList.Select(m => new ApplicationBasicDetailAC
                    {
                        Id = m.LoanApplicationId,
                        LoanApplicationNumber = m.LoanApplication.LoanApplicationNumber,
                        Status = m.LoanApplication.Status,
                        IsReadOnlyMode = _globalRepository.IsLoanReadOnlyAsync(m.LoanApplication).Result,
                        SectionName = m.LoanApplication.UserLoanSectionMappings.Any(x => x.UserId.Equals(currentUser.Id)) ? m.LoanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(currentUser.Id)).Section.Name : m.LoanApplication.UserLoanSectionMappings.Single(x => x.UserId.Equals(m.LoanApplication.CreatedByUserId)).Section.Name,
                        LastUpdatedOn = m.LoanApplication.UpdatedOn ?? m.LoanApplication.CreatedOn,
                        CreatedByUserId = m.LoanApplication.CreatedByUserId,
                        // Add bank details in the response to show on customer frontend
                        EntityBankDetails = m.LoanApplication.EMIDeducteeBankId != null ? new LoanEntityBankDetailsAC
                        {
                            LoanAmountDepositeeBank = new EntityBankDetailsAC
                            {
                                AccountNumber = m.LoanApplication.LoanAmountDepositeeBank.AccountNumber,
                                BankId = m.LoanApplication.LoanAmountDepositeeBank.BankId,
                                BankName = m.LoanApplication.LoanAmountDepositeeBank.Bank.Name,
                                SWIFTCode = m.LoanApplication.LoanAmountDepositeeBank.Bank.SWIFTCode
                            },
                            EMIDeducteeBank = new EntityBankDetailsAC
                            {
                                AccountNumber = m.LoanApplication.EMIDeducteeBank.AccountNumber,
                                BankId = m.LoanApplication.EMIDeducteeBank.BankId,
                                BankName = m.LoanApplication.EMIDeducteeBank.Bank.Name,
                                SWIFTCode = m.LoanApplication.EMIDeducteeBank.Bank.SWIFTCode
                            }
                        } : new LoanEntityBankDetailsAC()
                    }).ToList();
                }
                else
                {
                    return new List<ApplicationBasicDetailAC>() { };
                }
            }
        }

        /// <summary>
        /// Method fetches loan applications with basic details for given current user (entity).
        /// </summary>
        /// <param name="entityId">Entity id</param>
        /// <param name="isOnlyUserInitiatedLoansRequired">Required all loans or only created by user</param>
        /// <returns>List of type LoanApplicationBasicDetailAC</returns>
        private async Task<List<ApplicationBasicDetailAC>> GetCurrentUserLoanApplicationsWithBasicDetailsAsync(Guid entityId, bool isOnlyUserInitiatedLoansRequired)
        {
            List<LoanApplication> loanApplications = await _dataRepository.Fetch<LoanApplication>(x => x.CreatedByUserId == entityId)
                    .Include(i => i.EntityLoanApplicationMappings).Include(i => i.UserLoanSectionMappings).ThenInclude(y => y.Section).Include(i => i.EMIDeducteeBank.Bank).Include(i => i.LoanAmountDepositeeBank.Bank)
                    .ToListAsync();

            if (!isOnlyUserInitiatedLoansRequired)
            {
                var companyIds = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.RelativeEntityId == entityId).Select(x => x.PrimaryEntityId).ToListAsync();
                var entityLoanMappingIds = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => companyIds.Contains(x.EntityId)).Select(x => x.LoanApplicationId).Distinct().ToListAsync();
                loanApplications.AddRange(await _dataRepository.Fetch<LoanApplication>(x => entityLoanMappingIds.Contains(x.Id))
                .Include(i => i.EntityLoanApplicationMappings).Include(i => i.UserLoanSectionMappings).ThenInclude(y => y.Section).Include(i => i.EMIDeducteeBank.Bank).Include(i => i.LoanAmountDepositeeBank.Bank)
                .ToListAsync());
            }

            var loanApplicationBasicDetailACs = new List<ApplicationBasicDetailAC>();
            if (loanApplications.Any())
            {
                foreach (var loanApplication in loanApplications)
                {
                    if (loanApplication.UserLoanSectionMappings.Any(x => x.UserId == entityId))
                    {
                        var loanApplicationBasicDetailAC = _mapper.Map<ApplicationBasicDetailAC>(loanApplication);
                        loanApplicationBasicDetailAC.SectionName = loanApplication.UserLoanSectionMappings.Single(s => s.UserId.Equals(entityId)).Section.Name;
                        loanApplicationBasicDetailAC.MappedEntityId = loanApplication.EntityLoanApplicationMappings.Any() ? loanApplication.EntityLoanApplicationMappings.First().EntityId : Guid.Empty;
                        loanApplicationBasicDetailAC.IsReadOnlyMode = _globalRepository.IsLoanReadOnlyAsync(loanApplication).Result;
                        loanApplicationBasicDetailACs.Add(loanApplicationBasicDetailAC);
                    }
                }
            }
            return loanApplicationBasicDetailACs;
        }

        /// <summary>
        /// Prepare company entityAc object by entity model.
        /// </summary>
        /// <param name="entityUser">entity object.</param>
        /// <returns>Company EntityAC object.</returns>
        private EntityAC PrepareUserEntityACByEntity(Entity entityUser)
        {
            EntityAC userEntity = new EntityAC
            {
                Id = entityUser.Id,
                User = _mapper.Map<User, UserAC>(entityUser.User),
                Address = _mapper.Map<Address, AddressAC>(entityUser.Address),
                Type = EntityType.User
            };

            if (entityUser.Address != null)
            {
                // Concatinating primary number and street line to show them combinely on UI.
                userEntity.Address.StreetLine = string.Format("{0} {1} {2} {3} {4}", entityUser.Address.PrimaryNumber, entityUser.Address.StreetLine, entityUser.Address.StreetSuffix, entityUser.Address.SecondaryNumber, entityUser.Address.SecondaryDesignator).Trim();
            }
            return userEntity;
        }
        /// <summary>
        /// Prepare user entityAc object by entity model.
        /// </summary>
        /// <param name="entity">Entity object.</param>
        /// <returns>User EntityAC object.</returns>
        private EntityAC PrepareCompanyEntityACByEntity(Entity entity)
        {
            EntityAC companyEntity = new EntityAC
            {
                Id = entity.Id,
                Company = new CompanyAC
                {
                    Name = entity.Company.Name,
                    BusinessAge = new BusinessAgeAC
                    {
                        Id = entity.Company.BusinessAge.Id,
                        Age = entity.Company.BusinessAge.Age,
                        Order = entity.Company.BusinessAge.Order
                    },
                    CIN = entity.Company.CIN,
                    CompanyFiscalYearStartMonth = entity.Company.CompanyFiscalYearStartMonth,
                    CompanyRegisteredState = entity.Company.CompanyRegisteredState,
                    IndustryType = new IndustryTypeAC
                    {
                        Id = entity.Company.NAICSIndustryTypeId,
                        IndustryType = entity.Company.NAICSIndustryType.IndustryType,
                        IndustryCode = entity.Company.NAICSIndustryType.IndustryCode
                    },
                    CompanySize = new CompanySizeAC
                    {
                        Id = entity.Company.CompanySize.Id,
                        Size = entity.Company.CompanySize.Size,
                        Order = entity.Company.CompanySize.Order
                    },
                    CompanyStructure = new CompanyStructureAC
                    {
                        Id = entity.Company.CompanyStructure.Id,
                        Structure = entity.Company.CompanyStructure.Structure,
                        Order = entity.Company.CompanyStructure.Order
                    },
                    IndustryExperience = new IndustryExperienceAC
                    {
                        Id = entity.Company.IndustryExperience.Id,
                        Experience = entity.Company.IndustryExperience.Experience,
                        Order = entity.Company.IndustryExperience.Order
                    },
                    CreatedByUserId = entity.Company.CreatedByUserId
                },
                Address = _mapper.Map<Address, AddressAC>(entity.Address),
                Type = EntityType.Company
            };
            // Concatinating primary number and street line to show them combinely on UI.
            companyEntity.Address.StreetLine = string.Format("{0} {1} {2} {3} {4}", entity.Address.PrimaryNumber, entity.Address.StreetLine, entity.Address.StreetSuffix, entity.Address.SecondaryNumber, entity.Address.SecondaryDesignator).Trim();
            return companyEntity;
        }
        /// <summary>
        /// Apply filter on the list of entity and return filter result.
        /// </summary>
        /// <param name="filterModel">Filter model object.</param>
        /// <param name="entityList">Company or user entity list details.</param>
        private List<EntityAC> GetFilterEntityList(FilterModelAC filterModel, List<EntityAC> entityList)
        {
            #region Filter model is null.
            if (filterModel == null)
            {
                var entity = entityList.SingleOrDefault(x => x.Type.Equals(null));
                if (entity != null)
                {
                    entityList.Remove(entity);
                }
                return entityList;
            }
            #endregion

            #region Filter model is not null.

            #region Filter
            if (filterModel.Filters != null)
            {
                foreach (var filter in filterModel.Filters)
                {
                    if (filter.Field.Trim().ToLowerInvariant() == StringConstant.FilterType && filter.Operator.Trim().ToLowerInvariant() == StringConstant.FilterEqualOperator)
                    {
                        if (filter.Value.Trim().ToLowerInvariant() == StringConstant.CompanyEntityType.ToLowerInvariant().Trim())
                        {
                            entityList = entityList.Where(x => x.Type == EntityType.Company).ToList();
                        }
                        else if (filter.Value.Trim().ToLowerInvariant() == StringConstant.PeopleEntityType.ToLowerInvariant().Trim())
                        {
                            entityList = entityList.Where(x => x.Type == EntityType.User).ToList();
                        }
                        else if (filter.Value.Trim().ToLowerInvariant().Equals(StringConstant.BankerEntityType.ToLowerInvariant().Trim()))
                        {
                            entityList = entityList.Where(x => x.Type.Equals(null)).ToList();
                        }
                    }
                    else if (filter.Field.Trim().ToLowerInvariant() == StringConstant.FilterId.ToLowerInvariant().Trim() && filter.Operator.Trim().ToLowerInvariant() == StringConstant.FilterEqualOperator.ToLowerInvariant().Trim())
                    {
                        entityList = entityList.Where(x => x.Id.Equals(Guid.Parse(filter.Value.Trim()))).ToList();
                    }
                }
            }
            #endregion

            #region Pagination
            if (filterModel.PageNo != null && filterModel.PageRecordCount != null)
            {
                int recordToSkip = (Math.Abs(filterModel.PageNo.Value) - 1) * Math.Abs(filterModel.PageRecordCount.Value);
                entityList = entityList.Skip(recordToSkip).Take(Math.Abs(filterModel.PageRecordCount.Value)).ToList();
            }
            #endregion
            #endregion
            return entityList;
        }

        /// <summary>
        /// Update User entity
        /// </summary>
        /// <param name="checkUser"></param>
        /// <param name="entity"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        private async Task UpdateUserAsync(User checkUser, EntityAC entity, CurrentUserAC loggedInUser)
        {
            // Unique SSN validation.
            if (entity.User.SSN != null)
            {
                User userWithSameSSN = await _dataRepository.SingleOrDefaultAsync<User>(x => x.SSN == entity.User.SSN && x.Email != checkUser.Email);
                if (userWithSameSSN != null)
                {
                    throw new ValidationException(String.Format(StringConstant.NotUniqueSSN, entity.User.SSN));
                }
            }

            // Unique phone number validation.
            if (entity.User.Phone != null)
            {
                User userWithSamePhone = await _dataRepository.SingleOrDefaultAsync<User>(x => x.Phone == entity.User.Phone && x.Email != checkUser.Email);
                if (userWithSamePhone != null)
                {
                    throw new ValidationException(String.Format(StringConstant.UniquePhoneErrorMessage, entity.User.Phone));
                }
            }

            if (entity.Address != null)
            {
                entity = await UpdateAddressAsync(entity, loggedInUser);
            }


            checkUser.UpdatedByUserId = loggedInUser.Id;
            checkUser.UpdatedOn = DateTime.UtcNow;
            checkUser.FirstName = entity.User.FirstName;
            checkUser.LastName = entity.User.LastName;
            checkUser.MiddleName = entity.User.MiddleName;
            checkUser.Phone = entity.User.Phone;
            checkUser.DOB = entity.User.DOB;
            if (!checkUser.IsRegistered)
            {
                checkUser.Email = entity.User.Email;
            }
            checkUser.ResidencyStatus = entity.User.ResidencyStatus;
            if (checkUser.SSN != entity.User.SSN)
            {
                //Linked proprietor companies of user.
                var userLinkedProprietorCompanies = await _dataRepository.Fetch<Company>(x => x.CompanyStructure.Structure.Equals(StringConstant.Proprietorship) && x.Entity.PrimaryEntityRelationships.Any(x => x.RelativeEntityId.Equals(checkUser.Id))).ToListAsync();

                if (userLinkedProprietorCompanies.Any())
                {
                    //Update CIN with new SSN of each proprietor company linked with user.
                    foreach (var userLinkedProprietorCompany in userLinkedProprietorCompanies)
                    {
                        userLinkedProprietorCompany.CIN = entity.User.SSN;
                    }
                    _dataRepository.UpdateRange(userLinkedProprietorCompanies);
                }
            }
            checkUser.SSN = entity.User.SSN;
            _dataRepository.Update(checkUser);
            var companyId = entity.RelationMapping != null ? entity.RelationMapping.PrimaryEntityId : entity.Id;
            // Prepare the Auditlog object to save the custom fields in the dbcontext.
            AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(loggedInUser, ResourceType.Company, companyId);
            await _dataRepository.SaveChangesAsync(auditLog);
        }

        /// <summary>
        /// Update address of entity
        /// </summary>
        /// <param name="entity">entity object</param>
        /// <param name="loggedInUser">Current logged in user</param>
        /// <returns></returns>
        private async Task<EntityAC> UpdateAddressAsync(EntityAC entity, CurrentUserAC loggedInUser)
        {

            if (entity.Address.Id == null)
            {
                Address address = await AddAddressAsync(entity, loggedInUser);
                Entity entityUser = await _dataRepository.Fetch<Entity>(x => x.Id.Equals(entity.Id)).SingleAsync();
                if (entityUser.AddressId != null)
                {
                    throw new InvalidParameterException(StringConstant.AddressAlreadyLinked);
                }
                entityUser.AddressId = address.Id;
                entity.Address.Id = address.Id;
            }
            else
            {
                Entity entityAddress = await _dataRepository.Fetch<Entity>(x => x.AddressId.Equals(entity.Address.Id) && x.Id.Equals(entity.Id)).Include(x => x.Address).SingleOrDefaultAsync();

                if (entityAddress == null)
                {
                    throw new InvalidParameterException(StringConstant.NoAddressForEntityId);
                }

                if (entityAddress.Address.City != entity.Address.City || entityAddress.Address.StateAbbreviation != entity.Address.StateAbbreviation
                    || string.Format("{0}{1}{2}{3}{4}", entityAddress.Address.PrimaryNumber, entityAddress.Address.StreetLine, entityAddress.Address.StreetSuffix, entityAddress.Address.SecondaryNumber, entityAddress.Address.SecondaryDesignator).Replace(" ", "")
                    != entity.Address.StreetLine.Replace(" ", ""))
                {
                    var validAddress = await GetValidatedAddressAsync(entity.Address);
                    AddressAC addressAC = _mapper.Map<SmartyStreets.USStreetApi.Candidate, AddressAC>(validAddress, entity.Address);
                    addressAC.IntegratedServiceConfigurationId = await _dataRepository.Fetch<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.SmartyStreets) && x.IsServiceEnabled).Select(x => x.Id).SingleAsync();
                    addressAC.AddressJson = System.Text.Json.JsonSerializer.Serialize(validAddress.Components);
                    entityAddress.Address = _mapper.Map<AddressAC, Address>(addressAC, entityAddress.Address);
                    entityAddress.Address.UpdatedByUserId = loggedInUser.Id;
                    entityAddress.Address.UpdatedOn = DateTime.UtcNow;
                }

                _dataRepository.Update<Address>(entityAddress.Address);
            }
            return entity;
        }

        /// <summary>
        /// Add new Address
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        private async Task<Address> AddAddressAsync(EntityAC entity, CurrentUserAC loggedInUser)
        {

            //Validate Address
            var validAddress = await GetValidatedAddressAsync(entity.Address);
            entity.Address = _mapper.Map<SmartyStreets.USStreetApi.Candidate, AddressAC>(validAddress);
            entity.Address.AddressJson = System.Text.Json.JsonSerializer.Serialize(validAddress.Components);
            entity.Address.IntegratedServiceConfigurationId = await _dataRepository.Fetch<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.SmartyStreets) && x.IsServiceEnabled).Select(x => x.Id).SingleAsync();

            Address address = _mapper.Map<AddressAC, Address>(entity.Address);

            // Add CreatedBy user id and CreatedOn date.
            address.CreatedByUserId = loggedInUser.Id;
            address.CreatedOn = DateTime.UtcNow;

            await _dataRepository.AddAsync<Address>(address);
            return address;
        }

        /// <summary>
        /// Method to fetch experian credit report of user and save it in CreditReport table
        /// </summary>
        /// <param name="entityId">Entity id</param>
        /// <param name="applicationId">Loan application id</param>
        /// <param name="currentUser">LoggedIn user object</param>
        /// <returns></returns>
        private async Task UserCreditReportAPIAsync(Guid entityId, Guid applicationId, CurrentUserAC currentUser)
        {
            string bankPreferenceCustomerAPIBureau = _configuration.GetValue<string>("BankPreference:ConsumerAPIBureau");
            var integratedServiceId = (await _dataRepository.SingleAsync<IntegratedServiceConfiguration>(x => x.Name.Equals(bankPreferenceCustomerAPIBureau))).Id;

            CreditReport creditReport;
            bool isCreditReportAdded = false;

            //Fetch credit report for bank prefered bureau
            var existingCreditReportsForTheEntity = _dataRepository.Fetch<CreditReport>(c => c.EntityId.Equals(entityId) && c.IntegratedServiceConfigurationId.Equals(integratedServiceId));
            creditReport = await existingCreditReportsForTheEntity.GetLatestVersionForLoan().SingleOrDefaultAsync();

            //if credit report not exists or older than 1 year.
            if (creditReport == null || (DateTime.UtcNow - creditReport.CreatedOn).TotalDays > 365)
            {
                Entity entity = await _dataRepository.Fetch<Entity>(e => e.Id.Equals(entityId)).Include(i => i.User).Include(i => i.Address).SingleAsync();

                UserInfoAC userInfo = new UserInfoAC
                {
                    FirstName = entity.User.FirstName,
                    LastName = entity.User.LastName,
                    MiddleName = entity.User.MiddleName ?? "",
                    Address = _mapper.Map<Address, AddressAC>(entity.Address)
                };

                userInfo.Address.StreetLine = string.Format("{0} {1} {2} {3} {4}", entity.Address.PrimaryNumber, entity.Address.StreetLine, entity.Address.StreetSuffix, entity.Address.SecondaryNumber, entity.Address.SecondaryDesignator).Trim();

                userInfo.DOB = entity.User.DOB.Value;

                userInfo.SSN = entity.User.SSN;

                userInfo.SSN = userInfo.SSN.Insert(3, "-");

                userInfo.SSN = userInfo.SSN.Insert(6, "-");

                userInfo.PhoneNumber = entity.User.Phone;

                //if selected bureau(from appsettings) is equifax then call equifax utility function for user credit report.
                if (bankPreferenceCustomerAPIBureau == StringConstant.EquifaxAPI)
                {
                    IntegratedServiceConfiguration equifaxService = await _dataRepository.Fetch<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.EquifaxAPI)).SingleAsync();
                    userInfo = await _equifaxUtility.FetchUserCreditScoreEquifaxAsync(userInfo, equifaxService.ConfigurationJson);
                }
                //if selected bureau(from appsettings) is experian then call experian utility function.
                else if (bankPreferenceCustomerAPIBureau == StringConstant.ExperianAPI)
                {
                    IntegratedServiceConfiguration experianService = await _dataRepository.Fetch<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.ExperianAPI)).SingleAsync();
                    userInfo = await _experianUtility.FetchUserCreditScoreExperianAsync(userInfo, experianService.ConfigurationJson);
                }
                //else if BankPreference is transunion
                else if (bankPreferenceCustomerAPIBureau == StringConstant.TransunionAPI)
                {
                    userInfo.SSN = userInfo.SSN.Replace(@"-", "");
                    IntegratedServiceConfiguration transunionService = await _dataRepository.Fetch<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.TransunionAPI)).SingleAsync();
                    //call transunion untility method to fetch user credit report.
                    userInfo = await _transunionUtility.FetchConsumerCreditReportAsync(userInfo, transunionService.ConfigurationJson);
                }
                else
                {
                    throw new InvalidParameterException(StringConstant.InvalidSelectedBureau);
                }

                if (userInfo.ConsumerCreditReportResponse != null)
                {
                    creditReport = new CreditReport()
                    {
                        EntityId = entityId,
                        Response = userInfo.ConsumerCreditReportResponse,
                        IntegratedServiceConfigurationId = integratedServiceId,
                        CreatedOn = DateTime.UtcNow,
                        FsrScore = userInfo.Score,
                        IsBankrupted = userInfo.Bankruptcy,
                        LoanApplicationId = applicationId,
                        Version = Guid.NewGuid()
                    };
                    await _dataRepository.AddAsync(creditReport);
                    isCreditReportAdded = true;
                }
            }
            else
            {
                //Fetch the latest credit report and save a copy of it.
                creditReport = _dataRepository.DetachEntities(existingCreditReportsForTheEntity.GetLatestVersionForLoan()).AsQueryable().VersionThisQueryable(applicationId).SingleOrDefault();
                await _dataRepository.AddAsync(creditReport);
                isCreditReportAdded = true;
            }

            //If an add operation has been performed then only version that record and save the audit logs.
            if (isCreditReportAdded)
            {
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Company, entityId);
                await _dataRepository.SaveChangesAsync(auditLog);
            }
        }

        /// <summary>
        /// Method to fetch experian credit report of company and save it in Credit Report table
        /// </summary>
        /// <param name="entityId">entity Id</param>
        /// <param name="applicationId">Loan application id</param>
        /// <param name="currentUser">LoggedIn user object.</param>
        /// <returns></returns>
        private async Task CompanyCreditReportExperianAPIAsync(Guid entityId, Guid applicationId, CurrentUserAC currentUser)
        {
            var integratedServiceId = (await _dataRepository.SingleAsync<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.ExperianAPI))).Id;
            CreditReport creditReport;
            bool isCreditReportAdded = false;

            //Fetch the latest version of a credit report of the given entity.
            var existingCreditReportsForTheEntity = _dataRepository.Fetch<CreditReport>(c => c.EntityId.Equals(entityId));
            creditReport = await existingCreditReportsForTheEntity.GetLatestVersionForLoan().SingleOrDefaultAsync();

            //if credit report not exists or older than 1 year then fetch new and save it else copy the latest one and save it.
            if (creditReport == null || ((DateTime.UtcNow - creditReport.CreatedOn).TotalDays > 365 && creditReport.IntegratedServiceConfigurationId == integratedServiceId))
            {
                Company company = await _dataRepository.Fetch<Company>(x => x.Id.Equals(entityId)).Include(i => i.CompanyStructure).SingleAsync();

                if (company.CompanyStructure.Structure != StringConstant.Proprietorship)
                {
                    IntegratedServiceConfiguration experianService = await _dataRepository.Fetch<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.ExperianAPI)).SingleAsync();
                    PremierProfilesResponseAC response = await _experianUtility.FetchCompanyCreditScoreExperianAsync(company.CIN, experianService.ConfigurationJson);

                    if (response.Results != null)
                    {
                        creditReport = new CreditReport()
                        {
                            IsBankrupted = response.Results.ExpandedCreditSummary.BankruptcyIndicator,
                            HasPendingJudgment = response.Results.ExpandedCreditSummary.JudgmentIndicator,
                            HasPendingLien = response.Results.ExpandedCreditSummary.TaxLienIndicator,
                            FsrScore = response.Results.ScoreInformation.FsrScore.Score,
                            CommercialScore = response.Results.ScoreInformation.CommercialScore.Score,
                            EntityId = entityId,
                            Response = response.JsonResponse,
                            IntegratedServiceConfigurationId = (await _dataRepository.SingleAsync<IntegratedServiceConfiguration>(x => x.Name.Equals(StringConstant.ExperianAPI))).Id,
                            CreatedOn = DateTime.UtcNow,
                            LoanApplicationId = applicationId,
                            Version = Guid.NewGuid()
                        };
                        await _dataRepository.AddAsync(creditReport);
                        isCreditReportAdded = true;
                    }
                }
            }
            else
            {
                //Fetch the latest credit report and save a copy of it.
                creditReport = _dataRepository.DetachEntities(existingCreditReportsForTheEntity.GetLatestVersionForLoan()).AsQueryable().VersionThisQueryable(applicationId).SingleOrDefault();
                await _dataRepository.AddAsync(creditReport);
                isCreditReportAdded = true;
            }

            //If an add operation has been performed then only save the audit logs.
            if (isCreditReportAdded)
            {
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Company, entityId);
                await _dataRepository.SaveChangesAsync(auditLog);
            }
        }

        /// <summary>
        /// Check if Company structure is valid 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="companyStructuresId"></param>
        /// <returns></returns>
        private EntityAC CheckValidCompanyStructureData(EntityAC entity, CompanyStructuresIdAC companyStructuresId)
        {
            if (entity.Company.CompanyStructure.Id == companyStructuresId.ProprietorshipId)
            {
                entity.Company.CompanyRegisteredState = null;
                entity.Company.CompanyFiscalYearStartMonth = null;
            }
            else if (entity.Company.CompanyStructure.Id == companyStructuresId.LimitedLiabilityCompanyId || entity.Company.CompanyStructure.Id == companyStructuresId.PartnershipId || entity.Company.CompanyStructure.Id == companyStructuresId.SCorporationId || entity.Company.CompanyStructure.Id == companyStructuresId.CCorporationId)
            {
                if (entity.Company.CompanyStructure.Id != companyStructuresId.CCorporationId)
                {
                    entity.Company.CompanyFiscalYearStartMonth = null;
                }
                else if ((entity.Company.CompanyFiscalYearStartMonth > 12 || entity.Company.CompanyFiscalYearStartMonth < 0) && entity.Company.CompanyFiscalYearStartMonth != null)
                {
                    throw new InvalidParameterException(StringConstant.InvalidCompanyFiscalYearStartMonth);
                }

                //Read JSON file of US State list
                string json = _fileOperationsUtility.ReadFileContent("Dataset_USStateList.json");
                var stateList = JsonConvert.DeserializeObject<List<StateAC>>(json);

                //Check if state is not valid
                if (!stateList.Any(x => x.Name.Equals(entity.Company.CompanyRegisteredState)))
                {
                    throw new InvalidParameterException(StringConstant.InvalidCompanyRegisteredState);
                }
            }
            else
            {
                throw new InvalidParameterException(StringConstant.InvalidCompanyStructure);
            }
            return entity;
        }

        /// <summary>
        /// Add new company
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="companyStructuresId"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        private async Task<EntityAC> AddCompanyAsync(EntityAC entity, CompanyStructuresIdAC companyStructuresId, CurrentUserAC loggedInUser)
        {
            //Check uniqueness of company CIN
            if (entity.Company.CompanyStructure.Id != companyStructuresId.ProprietorshipId && !await _globalRepository.IsUniqueEINAsync(entity.Company.CIN))
            {
                throw new ValidationException(String.Format(StringConstant.NotUniqueEIN, entity.Company.CIN));
            }

            Address address = await AddAddressAsync(entity, loggedInUser);

            entity.Address.Id = address.Id;

            Company company = _mapper.Map<CompanyAC, Company>(entity.Company);

            Entity entityCompany = new Entity
            {
                Type = EntityType.Company,
                AddressId = address.Id
            };

            await _dataRepository.AddAsync(entityCompany);

            company.Id = entityCompany.Id;
            entity.Id = company.Id;

            // Add CreatedBy user id and CreatedOn date.
            company.CreatedByUserId = loggedInUser.Id;
            company.CreatedOn = DateTime.UtcNow;

            if (entity.Company.CompanyStructure.Id == companyStructuresId.ProprietorshipId)
            {
                await AddEntityRelationMapping(company.Id, loggedInUser.Id);

                // Unique SSN validation
                User userWithSameSSN = await _dataRepository.SingleOrDefaultAsync<User>(x => x.SSN == entity.Company.CIN && x.Id != loggedInUser.Id);
                if (userWithSameSSN != null)
                {
                    throw new ValidationException(String.Format(StringConstant.NotUniqueSSN, entity.Company.CIN));
                }
                User user = await _dataRepository.Fetch<User>(x => x.Id.Equals(loggedInUser.Id)).SingleAsync();
                user.SSN = entity.Company.CIN;
                _dataRepository.Update(user);
            }
            await _dataRepository.AddAsync<Company>(company);
            return entity;
        }


        /// <summary>
        /// Update existing company
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="companyStructuresId"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        private async Task<EntityAC> UpdateCompanyAsync(EntityAC entity, CompanyStructuresIdAC companyStructuresId, CurrentUserAC loggedInUser)
        {
            //Check if user has access to company
            Company company = await _dataRepository.Fetch<Company>(x => x.Id.Equals(entity.Id) && loggedInUser.Id.Equals(x.CreatedByUserId)).SingleOrDefaultAsync();

            if (company != null)
            {
                Company checkCIN = await _dataRepository.SingleOrDefaultAsync<Company>(u => u.CIN.Equals(entity.Company.CIN) && !u.Id.Equals(entity.Id));

                if (checkCIN != null && company.CompanyStructureId != companyStructuresId.ProprietorshipId)
                {
                    throw new ValidationException(String.Format(StringConstant.NotUniqueEIN, entity.Company.CIN));
                }

                company.Name = entity.Company.Name;
                company.CompanyRegisteredState = entity.Company.CompanyRegisteredState;
                company.CompanyFiscalYearStartMonth = entity.Company.CompanyFiscalYearStartMonth;
                company.IndustryExperienceId = entity.Company.IndustryExperience.Id;
                company.NAICSIndustryTypeId = entity.Company.IndustryType.Id;
                company.BusinessAgeId = entity.Company.BusinessAge.Id;
                company.CIN = entity.Company.CIN;
                company.CompanyStructureId = entity.Company.CompanyStructure.Id;
                company.CompanySizeId = entity.Company.CompanySize.Id;

                // Update UpdatedBy user id and UpdatedOn date.
                company.UpdatedByUserId = loggedInUser.Id;
                company.UpdatedOn = DateTime.UtcNow;
                entity = await UpdateAddressAsync(entity, loggedInUser);

                if (entity.Company.CompanyStructure.Id == companyStructuresId.ProprietorshipId)
                {
                    // Unique SSN validation.
                    User userWithSameSSN = await _dataRepository.SingleOrDefaultAsync<User>(x => x.SSN == entity.Company.CIN && x.Id != loggedInUser.Id);
                    if (userWithSameSSN != null)
                    {
                        throw new ValidationException(String.Format(StringConstant.NotUniqueSSN, entity.Company.CIN));
                    }

                    //Update User SSN
                    User user = await _dataRepository.Fetch<User>(x => x.Id.Equals(loggedInUser.Id)).SingleAsync();
                    user.SSN = entity.Company.CIN;
                    _dataRepository.Update(user);

                    //Remove all existing linked entity with primary entity
                    List<EntityRelationshipMapping> removeEntityRelationshipMapping = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(entity.Id)).ToListAsync();
                    _dataRepository.RemoveRange(removeEntityRelationshipMapping);

                    //Add Single linked entity as sole proprietor
                    await AddEntityRelationMapping(company.Id, loggedInUser.Id);
                }
                else
                {
                    Guid relationId = Guid.NewGuid();
                    if (entity.Company.CompanyStructure.Id == companyStructuresId.PartnershipId)
                    {
                        relationId = await _dataRepository.Fetch<Relationship>(x => x.Relation.Equals(StringConstant.Partner)).Select(x => x.Id).SingleAsync();
                    }
                    else if (entity.Company.CompanyStructure.Id == companyStructuresId.LimitedLiabilityCompanyId || entity.Company.CompanyStructure.Id == companyStructuresId.CCorporationId || entity.Company.CompanyStructure.Id == companyStructuresId.SCorporationId)
                    {
                        relationId = await _dataRepository.Fetch<Relationship>(x => x.Relation.Equals(StringConstant.Shareholder)).Select(x => x.Id).SingleAsync();
                    }

                    List<EntityRelationshipMapping> entityRelationshipMappings = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(entity.Id)).ToListAsync();
                    foreach (var entityRelationshipMapping in entityRelationshipMappings)
                    {
                        entityRelationshipMapping.RelationshipId = relationId;
                    }
                    _dataRepository.UpdateRange<EntityRelationshipMapping>(entityRelationshipMappings);
                }

                _dataRepository.Update<Company>(company);
            }
            else
            {
                throw new InvalidResourceAccessException(StringConstant.NoUpdateAccess);
            }
            return entity;
        }

        /// <summary>
        /// Add or Update entity company
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        private async Task<EntityAC> AddOrUpdateCompanyAsync(EntityAC entity, CurrentUserAC loggedInUser)
        {
            //Check company CIN is in valid format
            if (!_globalRepository.IsValidCIN(entity.Company.CIN))
            {
                throw new ValidationException(String.Format(StringConstant.InvalidCIN, entity.Company.CIN, entity.Company.Name));
            }

            //Check if relation exists
            if (entity.RelationMapping == null)
            {

                var industryType = await _dataRepository.SingleOrDefaultAsync<NAICSIndustryType>(s => s.Id.Equals(entity.Company.IndustryType.Id));
                //Check if provided industry type is valid or not
                if (industryType == null)
                {
                    throw new InvalidParameterException(StringConstant.InvalidSICIndustryCode);
                }

                entity.Company.IndustryType.Id = industryType.Id;

                //Fetch all company structure Id's
                CompanyStructuresIdAC companyStructuresId = await GetCompanyStructuresIdAsync();

                //Check if Company structure is valid 
                entity = CheckValidCompanyStructureData(entity, companyStructuresId);

                //Check if existing is updating or new is added
                if (entity.Id == null)
                {
                    entity = await AddCompanyAsync(entity, companyStructuresId, loggedInUser);
                }
                else
                {
                    entity = await UpdateCompanyAsync(entity, companyStructuresId, loggedInUser);
                }
            }
            return entity;
        }

        /// <summary>
        /// Add new user
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        private async Task<EntityAC> AddUserAsync(EntityAC entity, CurrentUserAC loggedInUser)
        {
            if (entity.User.Phone != null)
            {
                // Unique phone number validation.
                User userWithSamePhone = await _dataRepository.SingleOrDefaultAsync<User>(x => x.Phone == entity.User.Phone);
                if (userWithSamePhone != null)
                {
                    throw new ValidationException(String.Format(StringConstant.UniquePhoneErrorMessage, entity.User.Phone));
                }
            }


            // Unique SSN validation.
            if (entity.User.SSN != null)
            {
                User userWithSameSSN = await _dataRepository.SingleOrDefaultAsync<User>(x => x.SSN == entity.User.SSN);
                if (userWithSameSSN != null)
                {
                    throw new ValidationException(String.Format(StringConstant.NotUniqueSSN, entity.User.SSN));
                }
            }
            Entity userEntity = new Entity()
            {
                Type = EntityType.User
            };
            if (entity.Address != null)
            {
                Address address = await AddAddressAsync(entity, loggedInUser);

                entity.Address.Id = address.Id;

                userEntity.AddressId = address.Id;
            }


            await _dataRepository.AddAsync<Entity>(userEntity);
            User user = _mapper.Map<UserAC, User>(entity.User);
            user.CreatedByUserId = loggedInUser.Id;
            user.CreatedOn = DateTime.UtcNow;
            user.Id = userEntity.Id;
            await _dataRepository.AddAsync<User>(user);
            entity.Id = user.Id;
            // Prepare the Auditlog object to save the custom fields in the dbcontext.
            AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(loggedInUser, ResourceType.Company, userEntity.Id);
            await _dataRepository.SaveChangesAsync(auditLog);
            return entity;
        }

        /// <summary>
        /// Add user entity relation mapping
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="companyStructuresId"></param>
        /// <param name="primaryEntity"></param>
        /// <returns></returns>
        private async Task<EntityAC> AddUserEntityRelationMappingAsync(EntityAC entity, CompanyStructuresIdAC companyStructuresId, Company primaryEntity)
        {
            List<EntityRelationshipMapping> entityRelationshipMappingList = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(entity.RelationMapping.PrimaryEntityId)).ToListAsync();

            decimal TotalSharePercentage = entityRelationshipMappingList.Select(s => s.SharePercentage).Sum().Value;

            if (_configuration.GetValue<bool>("Entity:Relatives") && entityRelationshipMappingList.Any())
            {
                if ((primaryEntity.CompanyStructureId == companyStructuresId.LimitedLiabilityCompanyId || primaryEntity.CompanyStructureId == companyStructuresId.CCorporationId || primaryEntity.CompanyStructureId == companyStructuresId.SCorporationId) && entityRelationshipMappingList.Count == 1 && TotalSharePercentage >= _configuration.GetValue<decimal>("Entity:MajoritySharePercentage"))
                {
                    throw new InvalidParameterException(StringConstant.MajorityShareholderAlreadyExist);
                }

                if (entityRelationshipMappingList.Any(x => x.RelativeEntityId.Equals(entity.Id)))
                {
                    throw new InvalidParameterException(StringConstant.ShareholderAlreadyExist);
                }
            }

            EntityRelationshipMapping entityRelationshipMapping = new EntityRelationshipMapping()
            {
                PrimaryEntityId = entity.RelationMapping.PrimaryEntityId,
                RelativeEntityId = entity.Id.Value,
                RelationshipId = entity.RelationMapping.Relation.Id,
                SharePercentage = entity.RelationMapping.SharePercentage
            };

            await _dataRepository.AddAsync<EntityRelationshipMapping>(entityRelationshipMapping);
            entity.RelationMapping.Id = entityRelationshipMapping.Id;
            return entity;
        }

        /// <summary>
        /// Update user entity relation mapping
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task<EntityAC> UpdateUserEntityRelationMappingAsync(EntityAC entity)
        {
            EntityRelationshipMapping entityRelationshipMapping = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.Id.Equals(entity.RelationMapping.Id)).SingleOrDefaultAsync();
            if (entityRelationshipMapping == null)
            {
                throw new InvalidParameterException(StringConstant.InvalidEntityRelationshipMappingId);
            }
            entityRelationshipMapping.PrimaryEntityId = entity.RelationMapping.PrimaryEntityId;
            entityRelationshipMapping.RelativeEntityId = entity.Id.Value;
            entityRelationshipMapping.RelationshipId = entity.RelationMapping.Relation.Id;
            entityRelationshipMapping.SharePercentage = entity.RelationMapping.SharePercentage;

            _dataRepository.Update<EntityRelationshipMapping>(entityRelationshipMapping);
            return entity;
        }

        /// <summary>
        /// Set relation Id for adding entity(user) relation mapping
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="primaryEntity"></param>
        /// <param name="companyStructuresId"></param>
        /// <returns></returns>
        private async Task<EntityAC> SetRelationIdAsync(EntityAC entity, Company primaryEntity, CompanyStructuresIdAC companyStructuresId)
        {
            entity.RelationMapping.Relation = new RelationshipAC();
            if (primaryEntity.CompanyStructureId == companyStructuresId.PartnershipId)
            {
                entity.RelationMapping.Relation.Id = await _dataRepository.Fetch<Relationship>(x => x.Relation.Equals(StringConstant.Partner)).Select(x => x.Id).SingleAsync();
            }
            else if (primaryEntity.CompanyStructureId == companyStructuresId.LimitedLiabilityCompanyId || primaryEntity.CompanyStructureId == companyStructuresId.CCorporationId || primaryEntity.CompanyStructureId == companyStructuresId.SCorporationId)
            {
                entity.RelationMapping.Relation.Id = await _dataRepository.Fetch<Relationship>(x => x.Relation.Equals(StringConstant.Shareholder)).Select(x => x.Id).SingleAsync();
            }
            else if (primaryEntity.CompanyStructureId == companyStructuresId.ProprietorshipId)
            {
                //As Linked Entity(User) was already added with company at the time of creation of proprietorship company
                //So Cannot add more linked entity(user)
                throw new InvalidParameterException(StringConstant.ProprietorOnlyOneUser);
            }
            else
            {
                throw new InvalidParameterException(StringConstant.InvalidCompanyStructure);
            }
            return entity;
        }

        /// <summary>
        /// Add or Update entity(user) relation
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        private async Task<EntityAC> AddOrUpdateUserEntityRelationMappingAsync(EntityAC entity, CurrentUserAC loggedInUser)
        {
            if (_configuration.GetValue<bool>("Entity:Relatives"))
            {
                if (!(entity.RelationMapping.SharePercentage <= 100 && entity.RelationMapping.SharePercentage > 0))
                {
                    throw new ValidationException(String.Format(StringConstant.InvalidSharePercentageRange));
                }
            }
            else
            {
                entity.RelationMapping.SharePercentage = null;
            }
            //Check if user is shareholder
            var isShareHolder = await _dataRepository.Fetch<EntityRelationshipMapping>(x => (x.PrimaryEntityId.Equals(entity.RelationMapping.PrimaryEntityId) && x.RelativeEntityId.Equals(loggedInUser.Id))).Include(x => x.PrimaryEntity.Company).AnyAsync();

            //Check Access to company(Logged In user need to be company creator)
            var primaryEntity = await _dataRepository.Fetch<Company>(x => x.Id.Equals(entity.RelationMapping.PrimaryEntityId) && x.CreatedByUserId.Equals(loggedInUser.Id)).SingleOrDefaultAsync();

            if (isShareHolder || primaryEntity != null)
            {
                CompanyStructuresIdAC companyStructuresId = await GetCompanyStructuresIdAsync();

                entity = await SetRelationIdAsync(entity, primaryEntity, companyStructuresId);

                if (entity.RelationMapping.Id == null)
                {
                    entity = await AddUserEntityRelationMappingAsync(entity, companyStructuresId, primaryEntity);
                }
                else
                {
                    entity = await UpdateUserEntityRelationMappingAsync(entity);
                }
            }
            else
            {
                throw new InvalidResourceAccessException(StringConstant.NoUpdateAccess);
            }
            return entity;
        }

        /// <summary>
        /// Add or Update entity user
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        private async Task<EntityAC> AddOrUpdateUserAsync(EntityAC entity, CurrentUserAC loggedInUser)
        {
            entity.User.FirstName = entity.User.FirstName?.Trim();
            entity.User.MiddleName = entity.User.MiddleName?.Trim();
            entity.User.LastName = entity.User.LastName?.Trim();
            entity.User.Phone = entity.User.Phone?.Trim();
            entity.User.Email = entity.User.Email.Trim().ToLowerInvariant();

            if (entity.Id == null)
            {
                User checkUser = await _dataRepository.SingleOrDefaultAsync<User>(x => x.Email.Equals(entity.User.Email));

                if (checkUser == null)
                {
                    entity = await AddUserAsync(entity, loggedInUser);
                }
                else if (checkUser.IsRegistered)
                {
                    entity.Id = checkUser.Id;
                }
                else
                {
                    await UpdateUserAsync(checkUser, entity, loggedInUser);
                    entity.Id = checkUser.Id;
                }

            }
            else
            {
                User checkUser = await _dataRepository.SingleOrDefaultAsync<User>(x => x.Id.Equals(entity.Id));

                if (!checkUser.IsRegistered || checkUser.Id == loggedInUser.Id)
                {
                    await UpdateUserAsync(checkUser, entity, loggedInUser);
                }
            }

            //Add or Update entity(user) Relation
            if (entity.RelationMapping != null)
            {
                entity = await AddOrUpdateUserEntityRelationMappingAsync(entity, loggedInUser);
            }
            return entity;
        }

        /// <summary>
        /// Get All Company structures Id
        /// </summary>
        /// <returns></returns>
        private async Task<CompanyStructuresIdAC> GetCompanyStructuresIdAsync()
        {
            List<CompanyStructure> companyStructureList = await _dataRepository.GetAll<CompanyStructure>().ToListAsync();
            CompanyStructuresIdAC companyStructuresId = new CompanyStructuresIdAC
            {
                ProprietorshipId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.Proprietorship)).Select(s => s.Id).Single(),
                PartnershipId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.Partnership)).Select(s => s.Id).Single(),
                LimitedLiabilityCompanyId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.LimitedLiabilityCompany)).Select(s => s.Id).Single(),
                CCorporationId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.CCorporation)).Select(s => s.Id).Single(),
                SCorporationId = companyStructureList.Where(x => x.Structure.Equals(StringConstant.SCorporation)).Select(s => s.Id).Single()
            };
            return companyStructuresId;
        }

        /// <summary>
        /// Method to prepare additional document list to add and copy the documents on S3.
        /// </summary>
        /// <param name="entityId">Entity id</param>
        /// <param name="additionalDocuments">List of additional documents to add</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>List of EntityAdditionalDocument objects</returns>
        private List<EntityAdditionalDocument> PrepareAddditionalDocumentsListAndCopyDocumentsOnS3(Guid entityId, List<AdditionalDocumentAC> additionalDocuments, CurrentUserAC currentUser)
        {
            var additionalDocumentsToAdd = new List<EntityAdditionalDocument>();
            foreach (var additionalDocument in additionalDocuments)
            {
                //Copy files on S3
                var documentToAdd = _mapper.Map<Document>(additionalDocument.Document);
                if (documentToAdd.Path.StartsWith(StringConstant.FileTemp))
                {
                    var destinationObjectKey = _globalRepository.GetPathForKeyNameBucketForAdditionalDocument(entityId, additionalDocument);

                    _amazonS3Utility.CopyObject(documentToAdd.Path, destinationObjectKey);
                    documentToAdd.Path = destinationObjectKey;
                }

                //Add documents in DB
                var additionalDocumentToAdd = new EntityAdditionalDocument
                {
                    EntityId = entityId,
                    Document = documentToAdd,
                    AdditionalDocumentTypeId = additionalDocument.DocumentType.Id,
                    CreatedOn = DateTime.UtcNow,
                    CreatedByUserId = currentUser.Id
                };
                additionalDocumentsToAdd.Add(additionalDocumentToAdd);
            }
            return additionalDocumentsToAdd;
        }

        /// <summary>
        /// Method to get additional documents for the resource type company
        /// </summary>
        /// <param name="id">Resource id</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>List of EntityAdditionalDocument</returns>
        private async Task<List<EntityAdditionalDocument>> GetAdditionalDocumentsForResourceTypeCompanyAsync(Guid id, CurrentUserAC currentUser)
        {
            List<EntityAdditionalDocument> additionalDocuments = new List<EntityAdditionalDocument>();
            //Check current user access to given entity (company)
            if (!currentUser.IsBankUser && !await _globalRepository.CheckEntityRelationshipMappingAsync(id, currentUser.Id))
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            // All objects that are queryable are intentionally kept queryable
            var entityDocumentsQueryable = _dataRepository.Fetch<EntityAdditionalDocument>(x => x.EntityId == id);

            if (entityDocumentsQueryable != null && entityDocumentsQueryable.Any())
            {
                //If any records are found with null loan id then they are the latest one and return them in response
                if (entityDocumentsQueryable.Any(x => x.LoanApplicationId == null))
                {
                    // Fetch the documents (with loanId null)
                    additionalDocuments = await entityDocumentsQueryable.Where(x => x.LoanApplicationId == null)
                                            .Include(i => i.Document).Include(i => i.AdditionalDocumentType).ToListAsync();
                }
                // If non-versioned documents don't exist, fetch latest versioned documents, clone them and make new non-versioned documents
                else
                {
                    //Bank user is not allowed to make versioning related changes
                    if (!currentUser.IsBankUser)
                    {
                        // Disabling change tracking to improve performance
                        _dataRepository.DisableChangeTracking();

                        var latestDocuments = entityDocumentsQueryable.GetLatestVersionForLoan().Include(i => i.Document);

                        // Fetch the latest documents (saved for any previous loan), save a copy of it with loanId null
                        // Detch documents
                        var queryableDocuments = _dataRepository.DetachEntities(latestDocuments);

                        //Fetch all documents for the latest records of additional documents of given entity
                        var documentFileIdList = queryableDocuments.Select(x => x.DocumentId).ToList();
                        var queryableDocumentFileList = _dataRepository.Fetch<Document>(x => documentFileIdList.Contains(x.Id));

                        //Detach document file and assign it to new additional document object to add them in DB
                        foreach (var queryableDocument in queryableDocuments)
                        {
                            var documentFile = queryableDocumentFileList.Where(x => x.Id.Equals(queryableDocument.DocumentId));
                            queryableDocument.Document = _dataRepository.DetachEntities(documentFile).First();
                        }
                        await _dataRepository.AddRangeAsync(queryableDocuments);
                        AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, id);
                        _dataRepository.EnableChangeTracking();
                        await _dataRepository.SaveChangesAsync(auditLog);
                        additionalDocuments = await _dataRepository.Fetch<EntityAdditionalDocument>(x => x.EntityId.Equals(id) && x.LoanApplicationId == null)
                                                .Include(i => i.Document).Include(i => i.AdditionalDocumentType).ToListAsync();
                    }
                }
            }
            return additionalDocuments;
        }

        /// <summary>
        /// Method to get additional documents for the resource type loan
        /// </summary>
        /// <param name="id">Resource id</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>List of EntityAdditionalDocument</returns>
        private async Task<List<EntityAdditionalDocument>> GetAdditionalDocumentsForResourceTypeLoanAsync(Guid id, CurrentUserAC currentUser)
        {
            List<EntityAdditionalDocument> additionalDocuments = new List<EntityAdditionalDocument>();
            //Check if current user is not a bank user and also has an access to the given loan
            if (currentUser != null && !currentUser.IsBankUser && !await _globalRepository.CheckUserLoanAccessAsync(currentUser, id, !currentUser.IsBankUser))
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            var documentsQueryable = _dataRepository.Fetch<EntityAdditionalDocument>(x => x.LoanApplicationId.Equals(id));
            //If documents for given loan application are available then only fetch them
            if (documentsQueryable != null)
            {
                additionalDocuments = await documentsQueryable.GetLatestVersionForLoan().Include(i => i.Document).Include(i => i.AdditionalDocumentType).ToListAsync();
            }
            return additionalDocuments;
        }
        #endregion

        #region Public Methods
        #region Entity

        /// <summary>
        /// Check if entity has any open loan application and entity is allowed to start new application.
        /// </summary>
        /// <param name="entityId">Unique identifier of entity object</param>
        /// <param name="currentUser">CurrentUserAC object</param>
        /// <returns></returns>
        public async Task<bool> CheckEntityAllowToStartNewApplicationAsync(Guid entityId, CurrentUserAC currentUser)
        {
            //Check if current user has access to entity
            bool checkEntityAccess = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(entityId) && x.RelativeEntityId.Equals(currentUser.Id)).AnyAsync();

            if (!checkEntityAccess)
            {
                throw new InvalidResourceAccessException(StringConstant.NoAccessToEntity);
            }

            bool checkAnyDraftApplication = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => x.EntityId.Equals(entityId) && x.LoanApplication.Status.Equals(LoanApplicationStatusType.Draft)).AnyAsync();

            if (checkAnyDraftApplication)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Add or update entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="type"></param>
        /// <returns>object of EntityAC</returns>
        public async Task<EntityAC> AddOrUpdateEntityAsync(EntityAC entity, CurrentUserAC loggedInUser, string type)
        {
            if (type == null)
            {
                var checkEntityType = await _dataRepository.Fetch<Entity>(x => x.Id.Equals(entity.Id)).Select(s => s.Type).SingleAsync();
                if (checkEntityType == EntityType.User)
                {
                    type = StringConstant.PeopleEntityType;
                }
                else if (checkEntityType == EntityType.Company)
                {
                    type = StringConstant.CompanyEntityType;
                }
                else
                {
                    throw new InvalidParameterException(StringConstant.InvalidEntityType);
                }
            }
            //Check if bank user is not accessing
            if (loggedInUser.IsBankUser)
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            using (await _dataRepository.BeginTransactionAsync())
            {
                Guid? auditLogBlockId;
                var auditLogBlockName = ResourceType.Company;
                //Check if entity is company
                if (type.ToLowerInvariant() == StringConstant.CompanyEntityType.ToLowerInvariant().Trim() && entity.Company != null)
                {
                    entity = await AddOrUpdateCompanyAsync(entity, loggedInUser);
                    auditLogBlockId = entity.Id;
                }
                else if (type.ToLowerInvariant().Trim() == StringConstant.PeopleEntityType.ToLowerInvariant().Trim() && entity.User != null)
                {
                    entity = await AddOrUpdateUserAsync(entity, loggedInUser);
                    auditLogBlockId = entity.RelationMapping?.PrimaryEntityId;
                }
                else
                {
                    throw new InvalidParameterException(StringConstant.InvalidEntityType);
                }
                // Track the user detail in the loan log from the review and consent page.
                if (entity.LoanId.HasValue)
                {
                    auditLogBlockId = entity.LoanId.Value;
                    auditLogBlockName = ResourceType.Loan;
                }
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(loggedInUser, auditLogBlockName, auditLogBlockId);
                await _dataRepository.SaveChangesAsync(auditLog);
                _dataRepository.CommitTransaction();
            }
            return entity;
        }

        /// <summary>
        /// Remove linked entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task RemoveLinkEntityAsync(EntityAC entity, CurrentUserAC currentUser)
        {
            //Check entity access
            bool companyAccess = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntity.Company.CreatedByUserId.Equals(currentUser.Id) || (entity.RelationMapping.PrimaryEntityId.Equals(x.PrimaryEntity.Company.Id) && entity.Id.Equals(x.RelativeEntityId))).AnyAsync();
            if (!companyAccess)
            {
                throw new InvalidResourceAccessException(StringConstant.NoUpdateAccess);
            }

            List<EntityRelationshipMapping> entityRelationshipMappings = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(entity.RelationMapping.PrimaryEntityId) && x.RelativeEntityId.Equals(entity.Id)).ToListAsync();
            _dataRepository.RemoveRange(entityRelationshipMappings);
            // Prepare the Auditlog object to save the custom fields in the dbcontext.
            AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Company, entityRelationshipMappings[0].PrimaryEntityId);
            await _dataRepository.SaveChangesAsync(auditLog);
        }

        /// <summary>
        /// Get list of entity
        /// </summary>
        /// <param name="filterModel"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<List<EntityAC>> GetEntityListAsync(FilterModelAC filterModel, CurrentUserAC currentUser)
        {
            List<EntityAC> entityList = new List<EntityAC>();
            EntityAC entityAC;

            #region Fetch the entity
            if (currentUser.IsBankUser)
            {
                List<Entity> entities = await _dataRepository.GetAll<Entity>()
                    .Include(x => x.Company)
                    .Include(x => x.User)
                    .Include(x => x.Address)
                    .Include(x => x.Company).ThenInclude(x => x.NAICSIndustryType)
                    .Include(x => x.Company).ThenInclude(x => x.BusinessAge)
                    .Include(x => x.Company).ThenInclude(x => x.CompanySize)
                    .Include(x => x.Company).ThenInclude(x => x.CompanyStructure)
                    .Include(x => x.Company).ThenInclude(x => x.IndustryExperience).ToListAsync();
                foreach (var entity in entities)
                {
                    entityAC = entity.Type == EntityType.Company ? PrepareCompanyEntityACByEntity(entity) : PrepareUserEntityACByEntity(entity);
                    entityList.Add(entityAC);
                }

                BankUser bankUser = await _dataRepository.SingleAsync<BankUser>(x => x.Id == currentUser.Id);
                entityList.Add(new EntityAC { Id = bankUser.Id, User = _mapper.Map<BankUser, UserAC>(bankUser) });
            }
            else
            {
                #region Company entity
                List<Guid> linkedCompanyIdList = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.RelativeEntityId.Equals(currentUser.Id)).Select(x => x.PrimaryEntityId).Distinct().ToListAsync();

                List<Entity> entities = await _dataRepository.Fetch<Entity>(x => linkedCompanyIdList.Contains(x.Id) && x.Type == EntityType.Company)
                    .Include(x => x.Company)
                    .Include(x => x.Address)
                    .Include(x => x.Company).ThenInclude(x => x.NAICSIndustryType)
                    .Include(x => x.Company).ThenInclude(x => x.BusinessAge)
                    .Include(x => x.Company).ThenInclude(x => x.CompanySize)
                    .Include(x => x.Company).ThenInclude(x => x.CompanyStructure)
                    .Include(x => x.Company).ThenInclude(x => x.IndustryExperience).ToListAsync();

                foreach (var entity in entities)
                {
                    entityAC = PrepareCompanyEntityACByEntity(entity);
                    entityList.Add(entityAC);
                }
                #endregion

                #region User entity
                Entity entityUser = await _dataRepository.Fetch<Entity>(x => x.Id.Equals(currentUser.Id))
                    .Include(x => x.User)
                    .Include(x => x.Address).SingleAsync();

                entityAC = PrepareUserEntityACByEntity(entityUser);
                entityList.Add(entityAC);
                #endregion
            }
            #endregion

            #region Entity list filter.
            entityList = GetFilterEntityList(filterModel, entityList);
            #endregion

            return entityList;
        }

        /// <summary>
        /// Get an entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public async Task<EntityAC> GetEntityAsync(Guid entityId, CurrentUserAC currentUser)
        {
            Entity entity = await _dataRepository.Fetch<Entity>(x => x.Id.Equals(entityId)).SingleAsync();
            if (entity.Type == EntityType.Company)
            {
                //Check if logged in user can get company details
                bool checkAccess = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(entityId) && x.RelativeEntityId.Equals(currentUser.Id)).AnyAsync();
                if (checkAccess || currentUser.IsBankUser)
                {
                    List<EntityRelationshipMapping> entityRelationshipMappingList = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(entityId))
                    .Include(x => x.PrimaryEntity).ThenInclude(x => x.Company).ThenInclude(x => x.NAICSIndustryType)
                    .Include(x => x.PrimaryEntity).ThenInclude(x => x.Address)
                    .Include(x => x.PrimaryEntity).ThenInclude(x => x.Company).ThenInclude(x => x.BusinessAge)
                    .Include(x => x.PrimaryEntity).ThenInclude(x => x.Company).ThenInclude(x => x.CompanySize)
                    .Include(x => x.PrimaryEntity).ThenInclude(x => x.Company).ThenInclude(x => x.CompanyStructure)
                    .Include(x => x.PrimaryEntity).ThenInclude(x => x.Company).ThenInclude(x => x.IndustryExperience)
                    .Include(x => x.RelativeEntity).ThenInclude(x => x.User)
                    .Include(x => x.RelativeEntity).ThenInclude(x => x.Address)
                    .Include(x => x.Relationship).ToListAsync();

                    EntityAC companyEntity = new EntityAC
                    {
                        Id = entityRelationshipMappingList.First().PrimaryEntity.Id,
                        Company = new CompanyAC
                        {
                            Name = entityRelationshipMappingList.First().PrimaryEntity.Company.Name,
                            BusinessAge = new BusinessAgeAC
                            {
                                Id = entityRelationshipMappingList.First().PrimaryEntity.Company.BusinessAge.Id,
                                Age = entityRelationshipMappingList.First().PrimaryEntity.Company.BusinessAge.Age,
                                Order = entityRelationshipMappingList.First().PrimaryEntity.Company.BusinessAge.Order
                            },
                            CIN = entityRelationshipMappingList.First().PrimaryEntity.Company.CIN,
                            CompanyFiscalYearStartMonth = entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyFiscalYearStartMonth,
                            CompanyRegisteredState = entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyRegisteredState,
                            IndustryType = new IndustryTypeAC
                            {
                                Id = entityRelationshipMappingList.First().PrimaryEntity.Company.NAICSIndustryTypeId,
                                IndustryType = entityRelationshipMappingList.First().PrimaryEntity.Company.NAICSIndustryType.IndustryType,
                                IndustryCode = entityRelationshipMappingList.First().PrimaryEntity.Company.NAICSIndustryType.IndustryCode,
                                IndustrySectorId = entityRelationshipMappingList.First().PrimaryEntity.Company.NAICSIndustryType.NAICSParentSectorId
                            },
                            CompanySize = new CompanySizeAC
                            {
                                Id = entityRelationshipMappingList.First().PrimaryEntity.Company.CompanySize.Id,
                                Size = entityRelationshipMappingList.First().PrimaryEntity.Company.CompanySize.Size,
                                Order = entityRelationshipMappingList.First().PrimaryEntity.Company.CompanySize.Order
                            },
                            CompanyStructure = new CompanyStructureAC
                            {
                                Id = entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyStructure.Id,
                                Structure = entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyStructure.Structure,
                                Order = entityRelationshipMappingList.First().PrimaryEntity.Company.CompanyStructure.Order
                            },
                            IndustryExperience = new IndustryExperienceAC
                            {
                                Id = entityRelationshipMappingList.First().PrimaryEntity.Company.IndustryExperience.Id,
                                Experience = entityRelationshipMappingList.First().PrimaryEntity.Company.IndustryExperience.Experience,
                                Order = entityRelationshipMappingList.First().PrimaryEntity.Company.IndustryExperience.Order
                            },
                            CreatedByUserId = entityRelationshipMappingList.First().PrimaryEntity.Company.CreatedByUserId
                        },
                        Address = _mapper.Map<Address, AddressAC>(entityRelationshipMappingList.First().PrimaryEntity.Address)
                    };

                    // Concatinating primary number and street line to show them combinely on UI.
                    companyEntity.Address.StreetLine = string.Format("{0} {1} {2} {3} {4}", entityRelationshipMappingList.First().PrimaryEntity.Address.PrimaryNumber, entityRelationshipMappingList.First().PrimaryEntity.Address.StreetLine, entityRelationshipMappingList.First().PrimaryEntity.Address.StreetSuffix, entityRelationshipMappingList.First().PrimaryEntity.Address.SecondaryNumber, entityRelationshipMappingList.First().PrimaryEntity.Address.SecondaryDesignator).Trim();

                    List<EntityAC> entities = new List<EntityAC>();

                    foreach (var entityRelationshipMapping in entityRelationshipMappingList)
                    {
                        EntityAC relativeEntity = new EntityAC
                        {
                            Id = entityRelationshipMapping.RelativeEntity.Id,
                            Address = _mapper.Map<Address, AddressAC>(entityRelationshipMapping.RelativeEntity.Address),
                            User = _mapper.Map<User, UserAC>(entityRelationshipMapping.RelativeEntity.User),
                            RelationMapping = new EntityRelationMappingAC
                            {
                                Id = entityRelationshipMapping.Id,
                                PrimaryEntityId = entityRelationshipMapping.PrimaryEntityId,
                                Relation = new RelationshipAC
                                {
                                    Id = entityRelationshipMapping.Relationship.Id,
                                    Name = entityRelationshipMapping.Relationship.Relation
                                },
                                SharePercentage = entityRelationshipMapping.SharePercentage
                            }
                        };
                        if (entityRelationshipMapping.RelativeEntity.Address != null)
                        {
                            // Concatinating primary number and street line to show them combinely on UI.
                            relativeEntity.Address.StreetLine = string.Format("{0} {1} {2} {3} {4}", entityRelationshipMapping.RelativeEntity.Address.PrimaryNumber, entityRelationshipMapping.RelativeEntity.Address.StreetLine, entityRelationshipMapping.RelativeEntity.Address.StreetSuffix, entityRelationshipMapping.RelativeEntity.Address.SecondaryNumber, entityRelationshipMapping.RelativeEntity.Address.SecondaryDesignator).Trim();
                        }
                        entities.Add(relativeEntity);
                    }
                    companyEntity.LinkedEntities = entities;
                    return companyEntity;
                }
                else
                {
                    throw new InvalidResourceAccessException(StringConstant.InvalidAccessEntity);
                }

            }
            else if (entity.Type == EntityType.User)
            {
                //Check if user has access
                bool userAccess = await _dataRepository.Fetch<User>(x => x.CreatedByUserId.Equals(currentUser.Id) || x.Id.Equals(currentUser.Id)).AnyAsync();

                if (userAccess || currentUser.IsBankUser)
                {
                    User user = await _dataRepository.Fetch<User>(x => x.Id.Equals(entity.Id)).SingleAsync();

                    EntityAC userEntity = new EntityAC
                    {
                        Id = user.Id,
                        User = _mapper.Map<User, UserAC>(user)
                    };
                    return userEntity;
                }
                else
                {
                    throw new InvalidResourceAccessException(StringConstant.InvalidAccessEntity);
                }
            }
            else
            {
                return new EntityAC();
            }
        }

        #endregion

        #region User Credit profile information

        /// <summary>
        /// Save the user credit profile information and if it is valid credit profile based on criteria then return true else return false.
        /// </summary>
        /// <param name="entity">EntityAC object</param>
        /// <returns>if it is valid credit profile based on criteria then return true else return false.</returns>
        public async Task<bool> UpdateUserCreditProfileInformationAsync(EntityAC entity)
        {
            // User must save the credit profile details.
            User dbUser = await _dataRepository.SingleAsync<User>(x => x.Id.Equals(entity.Id));
            dbUser.SelfDeclaredCreditScore = entity.User.SelfDeclaredCreditScore;
            dbUser.HasBankruptcySelfDeclared = entity.User.HasBankruptcySelfDeclared.Value;
            dbUser.HasAnyJudgementsSelfDeclared = entity.User.HasAnyJudgementsSelfDeclared.Value;
            dbUser.UpdatedOn = DateTime.UtcNow;
            _dataRepository.Update(dbUser);
            await _dataRepository.SaveChangesAsync();

            // Check the credit profile details.
            return CheckUserCreditProfileInformation(dbUser);
        }
        #endregion

        #region Entity's loan applications
        /// <summary>
        /// Method fetches the list of loan applications linked with given entity id.
        /// </summary>
        /// <param name="entityId">Entity id whose applications are to be fetched</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>List of ApplicationBasicDetailAC objects</returns>
        public async Task<List<ApplicationBasicDetailAC>> GetLoanApplicationsWithBasicDetailsByEntityIdAsync(Guid entityId, CurrentUserAC currentUser)
        {
            List<ApplicationBasicDetailAC> list = await GetLoanApplicationsWithBasicDetailsAsync(entityId, currentUser);
            return await _globalRepository.CheckUserLoanAccessAsync(currentUser, list);
        }
        #endregion

        #region Credit Report
        /// <summary>
        /// Method to fetch credit report of entity
        /// </summary>
        /// <param name="entityId">Unique identifier for entity</param>
        /// <param name="applicationId">Unique identifier for application</param>
        /// <param name="currentUser">Current User Object</param>
        /// <returns></returns>
        public async Task FetchCreditReportAsync(Guid entityId, Guid applicationId, CurrentUserAC currentUser)
        {
            //Fetch Entity
            Entity entity = await _dataRepository.Fetch<Entity>(x => x.Id.Equals(entityId)).SingleAsync();

            //Check Entity Type
            if (entity.Type == EntityType.Company)
            {
                //Check If entity is linked with Loan
                EntityLoanApplicationMapping entityLoanApplicationMapping = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => x.EntityId.Equals(entityId) && x.LoanApplicationId.Equals(applicationId))
                    .Include(x => x.Entity.Company).SingleOrDefaultAsync();

                if (entityLoanApplicationMapping == null)
                {
                    throw new InvalidResourceAccessException(StringConstant.LoanNotLinkedWithEntity);
                }

                //Fetch all linked entity(user) of primary entity(company)
                List<EntityRelationshipMapping> entityRelationshipMappings = await _dataRepository.Fetch<EntityRelationshipMapping>(x => x.PrimaryEntityId.Equals(entityLoanApplicationMapping.EntityId)).ToListAsync();

                //Check If user is part of entity
                if (entityRelationshipMappings.SingleOrDefault(x => x.RelativeEntityId.Equals(currentUser.Id)) == null)
                {
                    throw new InvalidResourceAccessException(StringConstant.CurrentUserNotLinkedWithEntity);
                }

                bool hasAllRelativesGivenConsent = (_dataRepository.Fetch<EntityLoanApplicationConsent>(x => x.LoanApplicationId.Equals(applicationId) && entityRelationshipMappings.Select(x => x.RelativeEntityId).Contains(x.ConsenteeId) && x.IsConsentGiven.Equals(true)).Select(s => s.ConsenteeId).Distinct().Count() == entityRelationshipMappings.Select(x => x.RelativeEntityId).Count());

                //If Not all linked entity not given consent throw invalid Data Exception
                if (!hasAllRelativesGivenConsent)
                {
                    throw new InvalidParameterException(StringConstant.AllLinkedEntitiesProvideConsent);
                }
                string CommercialAPIBureauBankPreference = _configuration.GetValue<string>("BankPreference:CommercialAPIBureau");
                //if selected bureau(from appsettings) is experian then fetch and save company credit report
                if (CommercialAPIBureauBankPreference == StringConstant.ExperianAPI)
                {
                    await CompanyCreditReportExperianAPIAsync(entityId, applicationId, currentUser);
                }
            }
            else if (entity.Type == EntityType.User)
            {
                if (entityId == currentUser.Id)
                {
                    //Get Company which is linked with application Id
                    var entityLoanApplicationMappingList = await _dataRepository.Fetch<EntityLoanApplicationMapping>(x => x.LoanApplicationId.Equals(applicationId)).ToListAsync();

                    //Check If User has access to entity which is linked with Loan
                    if (await _dataRepository.Fetch<EntityRelationshipMapping>(x => entityLoanApplicationMappingList.Select(x => x.EntityId).Contains(x.PrimaryEntityId) && x.RelativeEntityId.Equals(entityId)).SingleOrDefaultAsync() == null)
                    {
                        throw new InvalidResourceAccessException(StringConstant.NoAccessToEntity);
                    }

                    //If the user has not given his consent for the given loan then it shouldn't allowed fetching his credit report.
                    if (!(await _dataRepository.Fetch<EntityLoanApplicationConsent>(x => x.LoanApplicationId.Equals(applicationId) && x.ConsenteeId.Equals(entityId)).ToListAsync()).Any())
                    {
                        throw new InvalidParameterException(StringConstant.UserHasNotProvidedConsent);
                    }

                    await UserCreditReportAPIAsync(entityId, applicationId, currentUser);
                }
                else
                {
                    throw new InvalidResourceAccessException(StringConstant.NoAccessToEntity);
                }
            }
            else
            {
                throw new InvalidParameterException(StringConstant.InvalidEntityType);
            }

        }

        /// <summary>
        /// Method to get credit report of entity
        /// </summary>
        /// <param name="entityId">Unique identifier for entity</param>
        /// <param name="applicationId">Unique identifier for a loan application</param>
        /// <returns></returns>
        public async Task<EntityAC> GetCreditReportAsync(Guid entityId, Guid? applicationId)
        {
            //Entity type of provided entity Id.
            var entityType = (await _dataRepository.SingleAsync<Entity>(x => x.Id.Equals(entityId))).Type;

            //Set current integrated service id for particular entity type
            Guid integratedServiceId;
            if (entityType == EntityType.Company)
            {
                integratedServiceId = (await _dataRepository.SingleAsync<IntegratedServiceConfiguration>(x => x.Name.Equals(_configuration.GetValue<string>("BankPreference:CommercialAPIBureau")))).Id;
            }
            else if (entityType == EntityType.User)
            {
                integratedServiceId = (await _dataRepository.SingleAsync<IntegratedServiceConfiguration>(x => x.Name.Equals(_configuration.GetValue<string>("BankPreference:ConsumerAPIBureau")))).Id;
            }
            else
            {
                throw new InvalidParameterException(StringConstant.InvalidEntityType);
            }

            CreditReport creditReport;

            //If loan application id is not null then needs to fetch versioned data of the given entity for the given loan.
            if (applicationId.HasValue && !applicationId.Value.Equals(Guid.Empty))
            {
                creditReport = await _dataRepository.Fetch<CreditReport>(x => x.EntityId.Equals(entityId) && x.LoanApplicationId.Equals(applicationId)
                    && x.IntegratedServiceConfigurationId.Equals(integratedServiceId)).GetLatestVersionForLoan().Include(x => x.IntegratedServiceConfiguration).SingleOrDefaultAsync();
            }
            else
            {
                creditReport = await _dataRepository.Fetch<CreditReport>(x => x.EntityId.Equals(entityId) && x.LoanApplicationId == null
                    && x.IntegratedServiceConfigurationId.Equals(integratedServiceId)).GetLatestVersionForLoan().Include(x => x.IntegratedServiceConfiguration).SingleOrDefaultAsync();
            }

            //If credit report does not exist for entity
            if (creditReport == null)
            {
                throw new DataNotFoundException();
            }

            EntityAC entity = new EntityAC
            {
                Id = entityId,
                CreditReport = new CreditReportAC
                {
                    Id = creditReport.Id,
                    CreditReportJson = creditReport.Response,
                    IntegratedServiceConfigurationId = creditReport.IntegratedServiceConfigurationId,
                    IntegratedServiceConfigurationName = creditReport.IntegratedServiceConfiguration.Name
                }
            };
            if (entity.CreditReport.IntegratedServiceConfigurationName == StringConstant.TransunionAPI)
            {
                entity.CreditReport.CreditReportJson = _globalRepository.ConvertXmlToJson(entity.CreditReport.CreditReportJson).ToString();
            }
            return entity;
        }
        #endregion

        #region Additional documents

        /// <summary>
        /// Method to fetch the additional documents of given entity
        /// </summary>
        /// <param name="id">Entity id</param>
        /// <param name="type">Resource type (company or user)</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns>EntityAC object</returns>
        public async Task<EntityAC> GetAdditionalDocumentsByResourceIdAsync(Guid id, ResourceType type, CurrentUserAC currentUser)
        {
            //Check if id is empty guid or not
            if (id.Equals(Guid.Empty))
            {
                throw new InvalidParameterException(StringConstant.InvalidDataProvidedInRequest);
            }

            List<EntityAdditionalDocument> additionalDocuments = new List<EntityAdditionalDocument>();
            EntityAC entityAC = new EntityAC
            {
                AdditionalDocuments = new List<AdditionalDocumentAC>()
            };

            //If resource type is company then consider id as entity id else consider it as application id
            if (type == ResourceType.Company)
            {
                additionalDocuments = await GetAdditionalDocumentsForResourceTypeCompanyAsync(id, currentUser);
                entityAC.Id = id;
            }
            else if (type == ResourceType.Loan)
            {
                additionalDocuments = await GetAdditionalDocumentsForResourceTypeLoanAsync(id, currentUser);
                entityAC.LoanId = id;
            }

            //Mapping of data to ACs
            if (additionalDocuments.Any())
            {
                entityAC.Id = type == ResourceType.Loan ? additionalDocuments.First().EntityId : entityAC.Id;
                entityAC.AdditionalDocuments.AddRange(_mapper.Map<List<AdditionalDocumentAC>>(additionalDocuments));
            }
            return entityAC;
        }

        /// <summary>
        /// Method to save additional documents of an entity
        /// </summary>
        /// <param name="entityId">Entity Id</param>
        /// <param name="entityAC">EntityAC object</param>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        public async Task SaveAdditionalDocumentOfEntityAsync(Guid entityId, EntityAC entityAC, CurrentUserAC currentUser)
        {
            //Check user relation with an entity
            if (!currentUser.IsBankUser && !await _globalRepository.CheckEntityRelationshipMappingAsync(entityId, currentUser.Id))
            {
                throw new InvalidResourceAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            //Check the extensions validity of all documents
            List<string> supportedExtensionList = new List<string> { StringConstant.Pdf, StringConstant.Xls, StringConstant.Png, StringConstant.Jpeg, StringConstant.Csv, StringConstant.Docx, StringConstant.Xlsx, StringConstant.Gif };
            List<string> fileNames = entityAC.AdditionalDocuments.Select(x => x.Document.Name).ToList();
            if (fileNames.Any(x => !supportedExtensionList.Contains(x.Split(".")[1])) || entityId.Equals(Guid.Empty))
            {
                throw new InvalidParameterException(StringConstant.InvalidDataProvidedInRequest);
            }

            List<EntityAdditionalDocument> additionalDocumentsToAdd = new List<EntityAdditionalDocument>();
            List<EntityAdditionalDocument> additionalDocumentsToRemove = new List<EntityAdditionalDocument>();
            List<Guid> documentIdsToRemove = new List<Guid>();

            //Fetch existing additional documents
            var existingAdditionalDocuments = await _dataRepository.Fetch<EntityAdditionalDocument>(x => x.EntityId.Equals(entityId) && x.LoanApplicationId == null)
                                                .Include(i => i.Document).Include(i => i.AdditionalDocumentType).ToListAsync();

            //If there are no any existing documents then create new entries of all the given documents in request
            if (!existingAdditionalDocuments.Any())
            {
                additionalDocumentsToAdd = PrepareAddditionalDocumentsListAndCopyDocumentsOnS3(entityId, entityAC.AdditionalDocuments, currentUser);

            } //If the documents exist for given entity then check which are the new ones to add and which are to remove from DB
            else
            {
                var additionalDocumentACsToAdd = entityAC.AdditionalDocuments.Where(x => !existingAdditionalDocuments.Select(y => y.Id).Contains(x.Id)).ToList();
                additionalDocumentsToRemove = existingAdditionalDocuments.Where(x => !entityAC.AdditionalDocuments.Select(y => y.Id).Contains(x.Id)).ToList();

                if (additionalDocumentACsToAdd.Any())
                {
                    additionalDocumentsToAdd = PrepareAddditionalDocumentsListAndCopyDocumentsOnS3(entityId, additionalDocumentACsToAdd, currentUser);
                }

                if (additionalDocumentsToRemove.Any())
                {
                    documentIdsToRemove = additionalDocumentsToRemove.Select(x => x.DocumentId).ToList();
                }
            }

            //If any of the lists (add or remove) contains any object in it then only performs relevant operation
            if (additionalDocumentsToAdd.Any() || additionalDocumentsToRemove.Any())
            {
                using (await _dataRepository.BeginTransactionAsync())
                {
                    if (additionalDocumentsToAdd.Any())
                    {
                        await _dataRepository.AddRangeAsync<EntityAdditionalDocument>(additionalDocumentsToAdd);
                    }

                    if (additionalDocumentsToRemove.Any())
                    {
                        _dataRepository.RemoveRange<EntityAdditionalDocument>(additionalDocumentsToRemove);

                        //Remove documents from bucket as well
                        List<Document> documentsToRemove = await _dataRepository.Fetch<Document>(x => documentIdsToRemove.Contains(x.Id)).ToListAsync();
                        await _amazonS3Utility.DeleteObjectsAsync(documentsToRemove.Select(x => x.Path).ToList());
                        _dataRepository.RemoveRange<Document>(documentsToRemove);
                    }

                    // Prepare the Auditlog object to save the custom fields in the dbcontext.
                    AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.User, entityId);
                    await _dataRepository.SaveChangesAsync(auditLog);
                    _dataRepository.CommitTransaction();
                }
            }
        }

        #endregion

        #endregion
    }
}
