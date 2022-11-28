using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.Product;
using LendingPlatform.Utils.ApplicationClass.TaxForm;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.Seed
{
    public class SeedDatabase
    {
        #region Private variables
        private readonly IDataRepository _dataRepository;
        private readonly IMapper _mapper;
        private readonly IFileOperationsUtility _fileOperationsUtility;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public SeedDatabase(IDataRepository dataRepository, IMapper mapper, IFileOperationsUtility fileOperationsUtility, IConfiguration configuration)
        {
            _dataRepository = dataRepository;
            _mapper = mapper;
            _fileOperationsUtility = fileOperationsUtility;
            _configuration = configuration;
        }
        #endregion

        #region Private Method(s)
        /// <summary>
        /// Seeds Company Structure to the database.
        /// </summary>
        private async Task SeedCompanyStructureAsync()
        {
            List<CompanyStructure> existingCompanyStructures = await _dataRepository.GetAll<CompanyStructure>().OrderBy(o => o.Order).ToListAsync();

            string json = _fileOperationsUtility.ReadFileContent("Dataset_TaxFormData.json");
            var model = JsonConvert.DeserializeObject<TaxFormDataAC>(json);
            List<CompanyStructure> companyStructures = model.CompanyStructures.Select(x => new CompanyStructure
            {
                Structure = x.Structure,
                Order = x.Order
            }).ToList();
            List<CompanyStructure> companyStructuresToAdd = companyStructures.Where(x => !existingCompanyStructures.Any(y => y.Structure == x.Structure)).ToList();
            if (companyStructuresToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<CompanyStructure>(companyStructuresToAdd);
                await _dataRepository.SaveChangesAsync();

            }
        }

        /// <summary>
        /// Seeds Industry Experience to the database.
        /// </summary>
        private async Task SeedIndustryExperienceAsync()
        {
            List<IndustryExperience> existingIndustryExperiences = await _dataRepository.GetAll<IndustryExperience>().OrderBy(o => o.Order).ToListAsync();
            List<string> industryExperiences = new List<string>()
            {
                StringConstant.IndustryExperienceZeroToFiveYears,
                StringConstant.IndustryExperienceAboveFiveYears
            };
            //List to add new data
            List<string> industryExperiencesToAdd = industryExperiences.Except(existingIndustryExperiences.Select(x => x.Experience)).ToList();
            List<IndustryExperience> years = new List<IndustryExperience>();
            if (industryExperiencesToAdd.Any())
            {
                int order = existingIndustryExperiences.Count + 1;
                foreach (var industryExperience in industryExperiencesToAdd)
                {
                    IndustryExperience year = new IndustryExperience()
                    {
                        Experience = industryExperience,
                        Order = order++,
                        IsEnabled = true
                    };
                    years.Add(year);
                }

                await _dataRepository.AddRangeAsync<IndustryExperience>(years);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds Business Age to the database.
        /// </summary>
        private async Task SeedBusinessAgeAsync()
        {
            List<BusinessAge> existingBusinessAges = await _dataRepository.GetAll<BusinessAge>().OrderBy(o => o.Order).ToListAsync();
            List<string> businessAges = new List<string>()
            {
                StringConstant.SixMonthToOneYear,
                StringConstant.OneYearToThreeYear,
                StringConstant.ThreeYearToFiveYear,
                StringConstant.MoreThanFiveYear
            };
            //List to add new data
            List<string> businessAgesToAdd = businessAges.Except(existingBusinessAges.Select(x => x.Age)).ToList();
            List<BusinessAge> ages = new List<BusinessAge>();
            if (businessAgesToAdd.Any())
            {
                int order = existingBusinessAges.Count + 1;
                foreach (var businessAge in businessAgesToAdd)
                {
                    BusinessAge age = new BusinessAge()
                    {
                        Age = businessAge,
                        Order = order++
                    };
                    ages.Add(age);
                }

                await _dataRepository.AddRangeAsync<BusinessAge>(ages);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds Company Size to the database.
        /// </summary>
        private async Task SeedCompanySizeAsync()
        {
            List<CompanySize> existingCompanySizes = await _dataRepository.GetAll<CompanySize>().OrderBy(o => o.Order).ToListAsync();
            List<string> companySizes = new List<string>()
            {
                StringConstant.ZeroToTen,
                StringConstant.AboveTen
            };
            //List to add new data
            List<string> companySizeToAdd = companySizes.Except(existingCompanySizes.Select(x => x.Size)).ToList();
            List<CompanySize> sizes = new List<CompanySize>();
            if (companySizeToAdd.Any())
            {
                int order = existingCompanySizes.Count + 1;
                foreach (var employeeStrength in companySizeToAdd)
                {
                    CompanySize employee = new CompanySize()
                    {
                        Size = employeeStrength,
                        Order = order++,
                        IsEnabled = true
                    };
                    sizes.Add(employee);
                }

                await _dataRepository.AddRangeAsync<CompanySize>(sizes);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds loan purposes into database.
        /// </summary>
        private async Task SeedLoanPurposeAsync()
        {
            List<LoanPurpose> existingLoanPurpose = await _dataRepository.GetAll<LoanPurpose>().Include(x => x.SubLoanPurposes).ToListAsync();
            List<LoanType> loanTypeDbList = await _dataRepository.GetAll<LoanType>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_ProductData.json");
            var model = JsonConvert.DeserializeObject<ProductDataAC>(json);
            List<LoanPurpose> loanPurposes = model.LoanPurposes.Select(s => new LoanPurpose
            {
                Name = s.PurposeName,
                Order = s.Order,
                LoanType = loanTypeDbList.FirstOrDefault(x => x.TypeName == model.LoanTypes.FirstOrDefault(y => y.Id == s.TypeId).TypeName),
                SubLoanPurposes = _mapper.Map<List<SubLoanPurposeAC>, List<SubLoanPurpose>>(model.SubLoanPurposes.Where(x => x.LoanPurposeId.Equals(s.Id)).ToList()),
                IsEnabled = s.IsEnabled
            }).ToList();
            List<LoanPurpose> loanPurposeToAdd = loanPurposes.
                Where(x => !existingLoanPurpose.Any(y => y.Name == x.Name))
                .ToList();

            List<LoanPurpose> loanPurposeToUpdate = existingLoanPurpose.
                Where(x => loanPurposes.Any(y => y.Name == x.Name && y.IsEnabled != x.IsEnabled)).ToList();

            if (loanPurposeToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<LoanPurpose>(loanPurposeToAdd);
            }


            if (loanPurposeToUpdate.Any())
            {
                foreach (var loanPurposeUpdate in loanPurposeToUpdate)
                {
                    loanPurposeUpdate.IsEnabled = !loanPurposeUpdate.IsEnabled;
                }
                _dataRepository.UpdateRange<LoanPurpose>(loanPurposeToUpdate);
            }

            await _dataRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds User Sections into database.
        /// </summary>
        private async Task SeedUserWiseSectionAsync()
        {
            //Remove whole method once data updated in dev, staging and master.
            if (!(await _dataRepository.AnyAsync<UserLoanSectionMapping>()))
            {
                List<LoanApplication> loanApplications = await _dataRepository.GetAll<LoanApplication>()
                    .Include(x => x.EntityLoanApplicationMappings)
                    .ThenInclude(x => x.Entity)
                    .ThenInclude(x => x.PrimaryEntityRelationships)
                    .ThenInclude(x => x.RelativeEntity)
                    .ThenInclude(x => x.EntityConsents)
                    .ToListAsync();

                List<UserLoanSectionMapping> userLoanSectionMappings = new List<UserLoanSectionMapping>();
                Guid personalFinanceSectionId = await _dataRepository.Fetch<Section>(x => x.Name == StringConstant.PersonalFinancesSection).Select(x => x.Id).SingleAsync();
                foreach (var loanApplication in loanApplications)
                {
                    if (loanApplication.EntityLoanApplicationMappings.Any())
                    {
                        foreach (var primaryEntity in loanApplication.EntityLoanApplicationMappings.First().Entity.PrimaryEntityRelationships)
                        {
                            if (primaryEntity.RelativeEntityId == loanApplication.CreatedByUserId || loanApplication.Status != DomainModel.Enums.LoanApplicationStatusType.Draft)
                            {
                                UserLoanSectionMapping userLoanSectionMapping = new UserLoanSectionMapping
                                {
                                    LoanApplicationId = loanApplication.Id,
                                    SectionId = loanApplication.SectionId.Value,
                                    UserId = primaryEntity.RelativeEntityId
                                };
                                userLoanSectionMappings.Add(userLoanSectionMapping);
                            }
                            else if (primaryEntity.RelativeEntity.EntityConsents.Any(x => !x.IsConsentGiven))
                            {
                                UserLoanSectionMapping userLoanSectionMapping = new UserLoanSectionMapping
                                {
                                    LoanApplicationId = loanApplication.Id,
                                    SectionId = personalFinanceSectionId,
                                    UserId = primaryEntity.RelativeEntityId
                                };
                                userLoanSectionMappings.Add(userLoanSectionMapping);
                            }

                        }
                    }
                    else
                    {
                        UserLoanSectionMapping userLoanSectionMapping = new UserLoanSectionMapping
                        {
                            LoanApplicationId = loanApplication.Id,
                            SectionId = loanApplication.SectionId.Value,
                            UserId = loanApplication.CreatedByUserId
                        };
                        userLoanSectionMappings.Add(userLoanSectionMapping);
                    }
                }
                await _dataRepository.AddRangeAsync(userLoanSectionMappings);
                await _dataRepository.SaveChangesAsync();
            }

        }

        /// <summary>
        /// Seeds NAICS Industry Type to the database.
        /// </summary>
        private async Task SeedNAICSIndustryTypeAsync()
        {
            bool industryTypeListExists = await _dataRepository.GetAll<NAICSIndustryType>().AnyAsync();
            //Check if data is already seeded
            if (!industryTypeListExists)
            {
                //Add data to table from file
                string json = _fileOperationsUtility.ReadFileContent("Dataset_IndustryTypeList.json");
                var model = JsonConvert.DeserializeObject<IndustryTypeSeedAC>(json);
                List<NAICSIndustryType> industryTypes = model.IndustryTypes.Select(s => new NAICSIndustryType { IndustryType = s.IndustryType, IndustryCode = s.IndustryCode }).ToList();

                await _dataRepository.AddRangeAsync<NAICSIndustryType>(industryTypes);
                await _dataRepository.SaveChangesAsync();
                List<NAICSIndustryType> industrySectorList = industryTypes.Where(x => x.IndustryCode.Length == 2).ToList();
                foreach (var industrySector in industrySectorList)
                {
                    var industryGroupList = industryTypes.Where(x => x.IndustryCode.StartsWith(industrySector.IndustryCode) && x.IndustryCode != industrySector.IndustryCode);
                    foreach (var industryGroup in industryGroupList)
                    {
                        industryGroup.NAICSParentSectorId = industrySector.Id;
                    }
                    _dataRepository.UpdateRange(industryGroupList);
                }
            }
        }

        /// <summary>
        /// Seed integrated configuration
        /// </summary>
        /// <returns></returns>
        private async Task SeedIntegratedServiceConfigurationAsync()
        {
            List<IntegratedServiceConfiguration> existingIntegratedServiceConfigurations = await _dataRepository.GetAll<IntegratedServiceConfiguration>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_IntegratedServiceConfiguration.json");
            var model = JsonConvert.DeserializeObject<List<IntegratedServiceConfigurationSeedAC>>(json);
            List<IntegratedServiceConfiguration> integratedServiceConfigurations = model.Select(x => new IntegratedServiceConfiguration
            {
                Name = x.Name,
                IsServiceEnabled = x.IsServiceEnabled
            }).ToList();
            List<IntegratedServiceConfiguration> integratedServiceConfigurationsToAdd = integratedServiceConfigurations.Where(x => !existingIntegratedServiceConfigurations.Any(y => y.Name == x.Name)).ToList();

            if (integratedServiceConfigurationsToAdd.Any())
            {
                foreach (var service in integratedServiceConfigurationsToAdd)
                {
                    if (service.Name.Equals(StringConstant.Quickbooks, StringComparison.InvariantCultureIgnoreCase))
                    {
                        service.ConfigurationJson = JsonConvert.SerializeObject(_configuration.GetSection(StringConstant.Quickbooks).GetChildren().ToList());
                    }
                    else if (service.Name.Equals(StringConstant.Xero, StringComparison.InvariantCultureIgnoreCase))
                    {
                        service.ConfigurationJson = JsonConvert.SerializeObject(_configuration.GetSection(StringConstant.Xero).GetChildren().ToList());
                    }

                    else if (service.Name.Equals(StringConstant.SmartyStreets, StringComparison.InvariantCultureIgnoreCase))
                    {
                        service.ConfigurationJson = JsonConvert.SerializeObject(_configuration.GetSection(StringConstant.SmartyStreets).GetChildren().ToList());
                    }
                    else if (service.Name.Equals(StringConstant.ExperianAPI, StringComparison.InvariantCultureIgnoreCase))
                    {
                        service.ConfigurationJson = JsonConvert.SerializeObject(_configuration.GetSection(StringConstant.ExperianAPI).GetChildren().ToList());
                    }
                    else if (service.Name.Equals(StringConstant.TransunionAPI, StringComparison.InvariantCultureIgnoreCase))
                    {
                        service.ConfigurationJson = JsonConvert.SerializeObject(_configuration.GetSection(StringConstant.TransunionAPI).GetChildren().ToList());
                    }
                    else if (service.Name.Equals(StringConstant.EquifaxAPI, StringComparison.InvariantCultureIgnoreCase))
                    {
                        service.ConfigurationJson = JsonConvert.SerializeObject(_configuration.GetSection(StringConstant.EquifaxAPI).GetChildren().ToList());
                    }
                }
                await _dataRepository.AddRangeAsync(integratedServiceConfigurationsToAdd);
            }
            //Remove Once experian configuration json gets updated in db
            IntegratedServiceConfiguration integratedServiceConfigurationExperian = existingIntegratedServiceConfigurations.SingleOrDefault(x => x.Name.Equals(StringConstant.ExperianAPI, StringComparison.InvariantCultureIgnoreCase));
            var experianConfigurationJson = JsonConvert.SerializeObject(_configuration.GetSection(StringConstant.ExperianAPI).GetChildren().ToList());
            if (integratedServiceConfigurationExperian != null && integratedServiceConfigurationExperian.ConfigurationJson != experianConfigurationJson)
            {
                integratedServiceConfigurationExperian.ConfigurationJson = experianConfigurationJson;
                _dataRepository.Update(integratedServiceConfigurationExperian);
            }
            await _dataRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds Loan Range Type to the database
        /// </summary>
        private async Task SeedLoanRangeTypeAsync()
        {
            List<LoanRangeType> existingLoanRangeTypeList = await _dataRepository.GetAll<LoanRangeType>().ToListAsync();

            string json = _fileOperationsUtility.ReadFileContent("Dataset_ProductData.json");
            var model = JsonConvert.DeserializeObject<ProductDataAC>(json);
            List<LoanRangeType> loanRangeTypes = model.LoanRangeTypes.Select(s => new LoanRangeType
            {
                Name = s.Name
            }).ToList();

            List<LoanRangeType> loanRangeTypeToAdd = loanRangeTypes.
                Where(x => !existingLoanRangeTypeList.Any(y => y.Name == x.Name))
                .ToList();
            if (loanRangeTypeToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<LoanRangeType>(loanRangeTypeToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seed Loan Type to the database
        /// </summary>
        private async Task SeedLoanTypeAsync()
        {
            List<LoanType> existingLoanTypeList = await _dataRepository.GetAll<LoanType>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_ProductData.json");
            var model = JsonConvert.DeserializeObject<ProductDataAC>(json);
            List<LoanType> loanTypes = model.LoanTypes.Select(x => new LoanType
            {
                TypeName = x.TypeName
            }).ToList();
            List<LoanType> loanTypeToAdd = loanTypes.
                Where(x => !existingLoanTypeList.Any(y => y.TypeName == x.TypeName))
                .ToList();
            if (loanTypeToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<LoanType>(loanTypeToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seed Product to the database.
        /// </summary>
        private async Task SeedProductAsync()
        {
            List<Product> existingProducts = await _dataRepository.GetAll<Product>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_ProductData.json");
            var model = JsonConvert.DeserializeObject<ProductDataAC>(json);
            List<LoanRangeType> loanRangeTypeDbList = await _dataRepository.GetAll<LoanRangeType>().ToListAsync();
            List<SubLoanPurpose> loanSubPurposeDbList = await _dataRepository.GetAll<SubLoanPurpose>().ToListAsync();
            List<Product> products = model.Products.Select(x => new Product
            {
                Name = x.Name,
                Description = x.Description,
                ProductStartDate = Convert.ToDateTime(x.StartDateAvailability),
                ProductEndDate = Convert.ToDateTime(x.EndDateAvailibility),
                IsEnabled = x.IsEnabled,
                DescriptionPoints = _mapper.Map<List<DescriptionPointSeedAC>, List<DescriptionPoint>>(x.DescriptionPoints),
                ProductRangeTypeMappings = model.ProductRangeTypeMappings.Where(y => y.ProductId == x.Id).Select(s => new ProductRangeTypeMapping
                {
                    LoanRangeType = loanRangeTypeDbList.FirstOrDefault(x => x.Name == model.LoanRangeTypes.FirstOrDefault(y => y.Id == s.RangeTypeId).Name),
                    Minimum = s.Minimum,
                    Maximum = s.Maximum
                }).ToList(),
                ProductSubPurposeMappings = model.ProductSubPurposeMappings.Where(y => y.ProductId == x.Id).Select(s => new ProductSubPurposeMapping
                {
                    SubLoanPurpose = loanSubPurposeDbList.FirstOrDefault(x => x.Name == model.SubLoanPurposes.FirstOrDefault(y => y.Id == s.SubPurposeId).Name)
                }).ToList()
            }).ToList();
            List<Product> productToAdd = products.
                Where(x => !existingProducts.Any(y => y.Name == x.Name))
                .ToList();
            if (productToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<Product>(productToAdd);
            }
            await _dataRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Seed loan product and loan type mapping in database
        /// </summary>
        private async Task SeedProductTypeMappingAsync()
        {
            List<ProductTypeMapping> existingProductTypeMappings = await _dataRepository.GetAll<ProductTypeMapping>().ToListAsync();
            List<Product> productDbList = await _dataRepository.GetAll<Product>().ToListAsync();
            List<LoanType> loanTypeDbList = await _dataRepository.GetAll<LoanType>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_ProductData.json");
            var model = JsonConvert.DeserializeObject<ProductDataAC>(json);
            List<ProductTypeMapping> productTypeMappings = model.ProductTypeMappings.Select(s => new ProductTypeMapping
            {
                Product = productDbList.FirstOrDefault(x => x.Name == model.Products.FirstOrDefault(y => y.Id == s.ProductId).Name),
                LoanType = loanTypeDbList.FirstOrDefault(x => x.TypeName == model.LoanTypes.FirstOrDefault(y => y.Id == s.TypeId).TypeName)
            }).ToList();
            List<ProductTypeMapping> productTypeMappingToAdd = productTypeMappings.
                Where(x => !existingProductTypeMappings.Any(y => y.ProductId == x.Product.Id && y.LoanTypeId == x.LoanType.Id))
                .ToList();
            if (productTypeMappingToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<ProductTypeMapping>(productTypeMappingToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds Relation to the database.
        /// </summary>
        /// <returns></returns>
        private async Task SeedRelationshipAsync()
        {
            List<Relationship> existingRelationships = _dataRepository.GetAll<Relationship>().ToList();
            List<string> relationships = new List<string>()
            {
                StringConstant.Partner,
                StringConstant.Proprietor,
                StringConstant.Shareholder
            };
            IEnumerable<string> relationshipToAdd = relationships.Except(existingRelationships.Select(x => x.Relation));
            //Add new seeded data
            List<Relationship> relationshipList = _mapper.Map<List<string>, List<Relationship>>(relationshipToAdd.ToList());
            if (relationshipList.Any())
            {
                await _dataRepository.AddRangeAsync<Relationship>(relationshipList);
                await _dataRepository.SaveChangesAsync();
            }
        }


        /// <summary>
        /// Seeds financial account type to the database.
        /// </summary>
        private async Task SeedFinancialStatementAsync()
        {
            var existingfinancialStatement = _dataRepository.GetAll<FinancialStatement>();
            var financialStatements = new List<string>()
            {
                StringConstant.IncomeStatement,
                StringConstant.BalanceSheet,
                StringConstant.CreditReport,
                StringConstant.Invoices,
                StringConstant.CashFlow,
                StringConstant.FinancialRatios,
                StringConstant.PersonalFinances
            };

            var financialStatementToAdd = financialStatements.Except(existingfinancialStatement.Select(x => x.Name));

            if (financialStatementToAdd.Any())
            {
                var financialStatementList = new List<FinancialStatement>();

                foreach (var financialStatement in financialStatementToAdd)
                {
                    financialStatementList.Add(new FinancialStatement()
                    {
                        Name = financialStatement,
                        IsAutoCalculated = StringConstant.AutocalculatedReports.Contains(financialStatement, StringComparer.InvariantCulture)
                    });
                }
                await _dataRepository.AddRangeAsync(financialStatementList);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds consents to the database.
        /// </summary>
        /// <returns></returns>
        private async Task SeedConsentsAsync()
        {
            string json = _fileOperationsUtility.ReadFileContent("Dataset_Consents.json");
            var consents = JsonConvert.DeserializeObject<List<Consent>>(json);

            List<Consent> existingConsents = await _dataRepository.GetAll<Consent>().ToListAsync();
            List<Consent> consentsToAdd = consents.Where(x => !existingConsents.Any(y => y.ConsentText == x.ConsentText)).ToList();
            if (consentsToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<Consent>(consentsToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds Banks to the database.
        /// </summary>
        private async Task SeedBanksAsync()
        {
            //Check if data is already seeded
            if (!(await _dataRepository.GetAll<Bank>().ToListAsync()).Any())
            {
                //Add data to table from file
                string json = _fileOperationsUtility.ReadFileContent("Dataset_BankList.json");
                var banks = JsonConvert.DeserializeObject<List<Bank>>(json);
                if (banks.Any())
                {
                    await _dataRepository.AddRangeAsync<Bank>(banks);
                    await _dataRepository.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Seeds Sections to the database.
        /// </summary>
        private async Task SeedSectionsAsync()
        {
            List<Section> existingSections = await _dataRepository.GetAll<Section>().Include(x => x.ParentSection).ToListAsync();

            // Get dataset
            string json = _fileOperationsUtility.ReadFileContent("Dataset_Sections.json");
            var sections = JsonConvert.DeserializeObject<List<SectionAC>>(json);
            var sectionToAdd = new List<Section>();

            // If table is completely empty, seed all sections from dataset
            if (!existingSections.Any())
            {
                SeedAllSections(sections, sectionToAdd);
            }
            else
            {
                AddOrUpdateSections(sections, existingSections, sectionToAdd);
            }


            if (sectionToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<Section>(sectionToAdd);
            }
            _dataRepository.UpdateRange(existingSections);
            await _dataRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Seed all sections
        /// </summary>
        /// <param name="sections"></param>
        /// <param name="sectionToAdd"></param>
        private void SeedAllSections(List<SectionAC> sections, List<Section> sectionToAdd)
        {
            var parentSections = sections.Where(x => x.ParentId == null).ToList();
            foreach (var section in parentSections)
            {
                sectionToAdd.Add(_mapper.Map<Section>(section));
            }

            var childSections = sections.Where(x => x.ParentId != null && x.ParentId > 0).ToList();
            foreach (var section in childSections)
            {
                var childSection = _mapper.Map<Section>(section);
                childSection.ParentSection = sectionToAdd.First(y => y.SectionId == section.ParentId);
                sectionToAdd.Add(childSection);
            }
        }

        /// <summary>
        /// Add or update sections
        /// </summary>
        /// <param name="sections"></param>
        /// <param name="existingSections"></param>
        /// <param name="sectionToAdd"></param>
        private void AddOrUpdateSections(List<SectionAC> sections, List<Section> existingSections, List<Section> sectionToAdd)
        {
            // Seed SectionId first if 0 in db
            foreach (var section in existingSections.Where(x => x.SectionId == 0).ToList())
            {
                var sectionInJson = sections.FirstOrDefault(x => x.Name == section.Name);
                if (section.Name == "Loan Product")
                {
                    sectionInJson = sections.FirstOrDefault(x => x.Name == "Product Selection");
                }
                section.SectionId = sectionInJson?.SectionId ?? 0;
            }

            var existingSectionIdList = existingSections.Where(x => x.SectionId > 0).Select(x => x.SectionId).ToList();

            // Pick and add/update as per flags set in dataset file
            var addedSections = sections.Where(x => x.IsAdded && !existingSectionIdList.Contains(x.SectionId)).ToList();

            var updatedSections = sections.Where(x => x.IsUpdated).ToList();


            // Finish mapping for parent section first
            var addedParentSections = addedSections.Where(x => x.ParentId == null).ToList();
            sectionToAdd.AddRange(_mapper.Map<List<Section>>(addedParentSections));


            var addChildSections = addedSections.Where(x => x.ParentId != null && x.ParentId > 0).ToList();
            foreach (var section in addChildSections)
            {
                var childSection = _mapper.Map<Section>(section);

                // Add parent section for childsection, 
                // When the parent section is new then take it from SectionToAdd list else from existingSectionsList
                if (!existingSections.Any(x => x.SectionId == section.ParentId))
                {
                    childSection.ParentSectionId = sectionToAdd.First(y => y.SectionId == section.ParentId).Id;
                }
                else
                {
                    childSection.ParentSectionId = existingSections.First(x => x.SectionId == section.ParentId).Id;
                }
                sectionToAdd.Add(childSection);
            }




            var updatedParentSections = updatedSections.Where(x => x.ParentId == null).ToList();
            foreach (var section in updatedParentSections)
            {
                _mapper.Map(section, existingSections.First(x => x.SectionId == section.SectionId));
            }


            // Add/Update mapping for child section if any
            var updatedChildSections = updatedSections.Where(x => x.ParentId != null).ToList();
            foreach (var section in updatedChildSections)
            {
                var existingChildSection = existingSections.First(x => x.SectionId == section.SectionId);
                _mapper.Map(section, existingChildSection);
                if (existingChildSection.ParentSection.SectionId != section.ParentId)
                {
                    if (!existingSections.Any(x => x.SectionId == section.ParentId))
                    {
                        existingChildSection.ParentSectionId = sectionToAdd.First(y => y.SectionId == section.ParentId).Id;
                    }
                    else
                    {
                        existingChildSection.ParentSectionId = existingSections.First(x => x.SectionId == section.ParentId).Id;
                    }
                }
            }
        }

        /// <summary>
        /// Seeds Tax Forms to database
        /// </summary>
        /// <returns></returns>
        private async Task SeedTaxFormAsync()
        {
            List<TaxForm> existingTaxForms = await _dataRepository.GetAll<TaxForm>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_TaxFormData.json");
            var model = JsonConvert.DeserializeObject<TaxFormDataAC>(json);
            List<TaxForm> taxForms = model.TaxForms.Select(x => new TaxForm
            {
                Name = x.Name
            }).ToList();
            List<TaxForm> taxFormsToAdd = taxForms.Where(x => !existingTaxForms.Any(y => y.Name == x.Name)).ToList();
            if (taxFormsToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<TaxForm>(taxFormsToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seed TaxForm Company Structure Mapping
        /// </summary>
        /// <returns></returns>
        private async Task SeedTaxFormCompanyStructureMappingAsync()
        {
            List<TaxFormCompanyStructureMapping> existingTaxFormCompanyStructureMappings = await _dataRepository.GetAll<TaxFormCompanyStructureMapping>().ToListAsync();
            List<CompanyStructure> companyStructures = await _dataRepository.GetAll<CompanyStructure>().ToListAsync();
            List<TaxForm> taxForms = await _dataRepository.GetAll<TaxForm>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_TaxFormData.json");
            var model = JsonConvert.DeserializeObject<TaxFormDataAC>(json);

            List<TaxFormCompanyStructureMapping> taxFormCompanyStructureMappings = model.TaxFormCompanyStructureMappings.Select(s => new TaxFormCompanyStructureMapping
            {
                CompanyStructure = companyStructures.FirstOrDefault(x => x.Structure == model.CompanyStructures.FirstOrDefault(y => y.Id == s.CompanyStructureId).Structure),
                TaxForm = taxForms.FirstOrDefault(x => x.Name == model.TaxForms.FirstOrDefault(y => y.Id == s.TaxFormId).Name),
                IsSoleProprietors = s.IsSoleProprietors
            }).ToList();

            List<TaxFormCompanyStructureMapping> taxFormCompanyStructureMappingsToAdd = taxFormCompanyStructureMappings.Where(x => !existingTaxFormCompanyStructureMappings.Any(y => y.TaxFormId == x.TaxForm.Id && y.CompanyStructureId == x.CompanyStructure.Id)).ToList();
            if (taxFormCompanyStructureMappingsToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<TaxFormCompanyStructureMapping>(taxFormCompanyStructureMappingsToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seed mappings for loan purpose and range type.
        /// </summary>
        private async Task SeedLoanPurposeRangeTypeMappingAsync()
        {
            List<LoanPurposeRangeTypeMapping> existingMappings = await _dataRepository.GetAll<LoanPurposeRangeTypeMapping>().ToListAsync();
            List<LoanPurpose> loanPurposes = await _dataRepository.GetAll<LoanPurpose>().ToListAsync();
            List<LoanRangeType> loanRangeTypes = await _dataRepository.GetAll<LoanRangeType>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_ProductData.json");
            var model = JsonConvert.DeserializeObject<ProductDataAC>(json);

            List<LoanPurposeRangeTypeMapping> loanPurposeRangeTypeMappings = model.LoanPurposeRangeTypeMappings.Select(s => new LoanPurposeRangeTypeMapping
            {
                LoanPurpose = loanPurposes.FirstOrDefault(x => x.Name == model.LoanPurposes.FirstOrDefault(y => y.Id == s.LoanPurposeId).PurposeName),
                LoanRangeType = loanRangeTypes.FirstOrDefault(x => x.Name == model.LoanRangeTypes.FirstOrDefault(y => y.Id == s.RangeTypeId).Name),
                Minimum = s.Minimum,
                Maximum = s.Maximum,
                StepperAmount = s.StepperAmount
            }).ToList();

            List<LoanPurposeRangeTypeMapping> mappingsToAdd = loanPurposeRangeTypeMappings.Where(x =>
                !existingMappings.Any(y => y.LoanPurposeId == x.LoanPurpose.Id && y.LoanRangeTypeId == x.LoanRangeType.Id)).ToList();

            if (mappingsToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<LoanPurposeRangeTypeMapping>(mappingsToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seed mapping for label name with tax form
        /// </summary>
        /// <returns></returns>
        private async Task SeedTaxformLabelNameMappingAsync()
        {
            List<TaxFormLabelNameMapping> existingTaxFormLabelNameMappings = await _dataRepository.GetAll<TaxFormLabelNameMapping>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_TaxReturnFields.json");
            var model = JsonConvert.DeserializeObject<TaxFormDataAC>(json);
            var taxReturnId = (await _dataRepository.SingleAsync<TaxForm>(x => x.Name == StringConstant.TaxReturns)).Id;
            
            List<TaxFormLabelNameMapping> taxFormLabelNameMappings = model.Fields.Select(x => new
                TaxFormLabelNameMapping
            {
                TaxFormId = taxReturnId,
                LabelFieldName = x.FieldKey.Split('_')[1],
                Order = Convert.ToInt32(x.FieldKey.Split('_')[0])
            }).ToList();

            List<TaxFormLabelNameMapping> taxFormLabelNamesToAdd = taxFormLabelNameMappings.Where(x => !existingTaxFormLabelNameMappings.Any(y => y.LabelFieldName == x.LabelFieldName && y.TaxFormId == x.TaxFormId)).ToList();
            if (taxFormLabelNamesToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<TaxFormLabelNameMapping>(taxFormLabelNamesToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seed method for adding OCR model data
        /// </summary>
        /// <returns></returns>
        private async Task SeedOCRModelMappingAsync()
        {
            List<OCRModelMapping> existingOCRModelMappings = await _dataRepository.GetAll<OCRModelMapping>().ToListAsync();
            var exisintgCompanyStructures = await _dataRepository.GetAll<CompanyStructure>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_OCRModelData.json");
            var model = JsonConvert.DeserializeObject<List<OCRModelMappingSeedAC>>(json);

            var oCRModelMappings = new List<OCRModelMapping>();
            foreach (var oCRModelMappingSeedAC in model)
            {
                var ocrModelMapping = new OCRModelMapping();
                if (oCRModelMappingSeedAC.Year != null)
                {
                    ocrModelMapping.Year = oCRModelMappingSeedAC.Year;
                }

                if (oCRModelMappingSeedAC.CompanyStructure != null)
                {
                    ocrModelMapping.CompanyStructureId = exisintgCompanyStructures.Single(x => x.Structure == oCRModelMappingSeedAC.CompanyStructure).Id;
                }

                ocrModelMapping.IsEnabled = oCRModelMappingSeedAC.IsEnabled;
                ocrModelMapping.ModelId = oCRModelMappingSeedAC.ModelId;
                oCRModelMappings.Add(ocrModelMapping);
            }

            List<OCRModelMapping> oCRModelMappingsToAdd = oCRModelMappings.Where(x => !existingOCRModelMappings.Any(y => y.ModelId == x.ModelId && y.Year == x.Year)).ToList();
            if (oCRModelMappingsToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<OCRModelMapping>(oCRModelMappingsToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        #region Personal Finance

        private PersonalFinanceAttributeCategoryMapping MapParentAttribute(PersonalFinanceAttributeCategoryMappingSeedDataAC mapping, PersonalFinanceSeedDataAC jsonModel, PersonalFinanceDBFieldsAC dataToAdd)
        {
            var parentMapping = new PersonalFinanceAttributeCategoryMapping
            {
                Category = dataToAdd.Categories.First(x => x.Name.Equals(jsonModel.PersonalFinanceCategories.First(y => y.Id.Equals(mapping.CategoryId)).Name)),
                Attribute = dataToAdd.Attributes.First(x => x.Text.Equals(jsonModel.PersonalFinanceAttributes.First(y => y.Id.Equals(mapping.AttributeId)).Text)),
                Order = mapping.Order,
                IsOriginal = mapping.IsOriginal,
                IsCurrent = mapping.IsCurrent,
                PersonalFinanceConstant = dataToAdd.Constants.FirstOrDefault(x => x.Name.Equals(jsonModel.PersonalFinanceConstants.FirstOrDefault(y => y.Id.Equals(mapping.PersonalFinanceConstantId))?.Name)),
                ChildAttributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>()
            };

            var childACMappings = jsonModel.PersonalFinanceAttributeCategoryMappings.Where(x => x.ParentAttributeCategoryMappingId.Equals(mapping.Id)).ToList();
            if (childACMappings.Any())
            {
                foreach (var child in childACMappings)
                {
                    var childMapping = MapParentAttribute(child, jsonModel, dataToAdd);
                    childMapping.ParentAttributeCategoryMapping = parentMapping;
                    parentMapping.ChildAttributeCategoryMappings.Add(childMapping);
                }
            }
            return parentMapping;
        }

        /// <summary>
        /// Method to flatten attribute list
        /// </summary>
        /// <param name="mappingList">List of parent mappings</param>
        /// <returns></returns>
        private IEnumerable<PersonalFinanceAttributeCategoryMapping> Flatten(List<PersonalFinanceAttributeCategoryMapping> mappingList)
        {
            var flatList = mappingList.SelectMany(c => Flatten(c.ChildAttributeCategoryMappings)).Concat(mappingList);
            return flatList;
        }


        /// <summary>
        /// Method to seed all the static data related to personal finance
        /// </summary>
        /// <returns></returns>
        private async Task SeedPersonalFinanceStaticDataAsync()
        {
            //Read the file content and deserialize it into an object.
            string json = _fileOperationsUtility.ReadFileContent("Dataset_PersonalFinanceStaticData.json");
            var model = JsonConvert.DeserializeObject<PersonalFinanceSeedDataAC>(json);

            //First, seed or update the constant (dropdown related) data.
            var existingConstants = await _dataRepository.GetAll<PersonalFinanceConstant>().ToListAsync();
            List<PersonalFinanceConstant> constants = model.PersonalFinanceConstants.Select(s => new PersonalFinanceConstant
            {
                Name = s.Name,
                ValueJson = JsonConvert.SerializeObject(s.Options)
            }).ToList();

            List<PersonalFinanceConstant> constantsToAdd = constants.Where(x => !existingConstants.Any(y => y.Name.Equals(x.Name))).ToList();
            List<PersonalFinanceConstant> constantsToRemove = existingConstants.Where(x => !constants.Any(y => y.Name.Equals(x.Name))).ToList();

            if (constantsToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<PersonalFinanceConstant>(constantsToAdd);
            }
            if (constantsToRemove.Any())
            {
                _dataRepository.RemoveRange<PersonalFinanceConstant>(constantsToRemove);
            }


            //Add personal finance accounts
            var existingAccounts = await _dataRepository.GetAll<PersonalFinanceAccount>().ToListAsync();
            List<PersonalFinanceAccount> accounts = model.PersonalFinanceAccounts.Select(s => new PersonalFinanceAccount
            {
                Name = s.Name,
                Order = s.Order,
                IsEnabled = s.IsEnabled
            }).ToList();

            List<PersonalFinanceAccount> accountsToAdd = accounts.Where(x => !existingAccounts.Any(y => y.Name.Equals(x.Name))).ToList();
            List<PersonalFinanceAccount> accountsToRemove = existingAccounts.Where(x => !accounts.Any(y => y.Name.Equals(x.Name))).ToList();

            if (accountsToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<PersonalFinanceAccount>(accountsToAdd);
            }
            if (accountsToRemove.Any())
            {
                _dataRepository.RemoveRange<PersonalFinanceAccount>(accountsToRemove);
            }


            //Add personal finance categories
            var existingCategories = await _dataRepository.GetAll<PersonalFinanceCategory>().ToListAsync();
            List<PersonalFinanceCategory> categories = model.PersonalFinanceCategories.Select(s => new PersonalFinanceCategory
            {
                PersonalFinanceAccount = accounts.First(x => x.Name.Equals(model.PersonalFinanceAccounts.First(y => y.Id.Equals(s.PersonalFinanceAccountId)).Name)),
                Name = s.Name,
                Order = s.Order,
                IsEnabled = s.IsEnabled
            }).ToList();

            List<PersonalFinanceCategory> categoriesToAdd = categories.Where(x => !existingCategories.Any(y => y.Name.Equals(x.Name))).ToList();
            List<PersonalFinanceCategory> categoriesToRemove = existingCategories.Where(x => !categories.Any(y => y.Name.Equals(x.Name))).ToList();

            if (categoriesToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<PersonalFinanceCategory>(categoriesToAdd);
            }
            if (categoriesToRemove.Any())
            {
                _dataRepository.RemoveRange<PersonalFinanceCategory>(categoriesToRemove);
            }


            //Add personal finance attributes
            var existingAttributes = await _dataRepository.GetAll<PersonalFinanceAttribute>().ToListAsync();
            List<PersonalFinanceAttribute> attributes = model.PersonalFinanceAttributes.Select(s => new PersonalFinanceAttribute
            {
                Text = s.Text,
                FieldType = (PersonalFinanceAttributeFieldType)Enum.Parse(typeof(PersonalFinanceAttributeFieldType), s.FieldType),
                IsEnabled = s.IsEnabled
            }).ToList();

            List<PersonalFinanceAttribute> attributesToAdd = attributes.Where(x => !existingAttributes.Any(y =>
                y.Text.Equals(x.Text) && y.FieldType.Equals(x.FieldType) && y.IsEnabled.Equals(x.IsEnabled))).ToList();

            List<PersonalFinanceAttribute> attributesToRemove = existingAttributes.Where(x => !attributes.Any(y =>
                y.Text.Equals(x.Text) && y.FieldType.Equals(x.FieldType) && y.IsEnabled.Equals(x.IsEnabled))).ToList();

            if (attributesToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<PersonalFinanceAttribute>(attributesToAdd);
            }
            if (attributesToRemove.Any())
            {
                _dataRepository.RemoveRange<PersonalFinanceAttribute>(attributesToRemove);
            }


            //Add personal finance category-attribute mappings
            var existingAttributeCategoryMappings = await _dataRepository.GetAll<PersonalFinanceAttributeCategoryMapping>()
                                                       .Include(i => i.Category).Include(i => i.Attribute).ToListAsync();

            var personalFinanceDBFields = new PersonalFinanceDBFieldsAC
            {
                Attributes = attributes,
                Constants = constants,
                Categories = categories
            };

            List<PersonalFinanceAttributeCategoryMapping> attributeCategoryMappings = new List<PersonalFinanceAttributeCategoryMapping>();
            List<PersonalFinanceAttributeCategoryMappingSeedDataAC> superParentMappings = model.PersonalFinanceAttributeCategoryMappings.Where(x => x.ParentAttributeCategoryMappingId == null).ToList();
            foreach (var parent in superParentMappings)
            {
                attributeCategoryMappings.Add(MapParentAttribute(parent, model, personalFinanceDBFields));
            }

            //Flatten parent mappings to get list of all the attributes upto end level
            var flattenedAttributeList = new List<PersonalFinanceAttributeCategoryMapping>();
            flattenedAttributeList.AddRange(Flatten(attributeCategoryMappings));

            List<PersonalFinanceAttributeCategoryMapping> attributeCategoryMappingsToAdd = flattenedAttributeList.Where(x => !existingAttributeCategoryMappings.Any(y =>
                y.Category.Name.Equals(x.Category.Name) && y.Attribute.Text.Equals(x.Attribute.Text) && y.Order.Equals(x.Order) && y.IsCurrent.Equals(x.IsCurrent) && y.IsOriginal.Equals(x.IsOriginal))).ToList();

            List<PersonalFinanceAttributeCategoryMapping> attributeCategoryMappingsToRemove = existingAttributeCategoryMappings.Where(x => !flattenedAttributeList.Any(y =>
                y.Category.Name.Equals(x.Category.Name) && y.Attribute.Text.Equals(x.Attribute.Text) && y.Order.Equals(x.Order) && y.IsCurrent.Equals(x.IsCurrent) && y.IsOriginal.Equals(x.IsOriginal))).ToList();

            if (attributeCategoryMappingsToAdd.Any())
            {
                var actualHierarchicalListToAdd = attributeCategoryMappings.Where(x => attributeCategoryMappingsToAdd.Select(y => y.Attribute.Text).Contains(x.Attribute.Text)).ToList();
                await _dataRepository.AddRangeAsync<PersonalFinanceAttributeCategoryMapping>(actualHierarchicalListToAdd);
            }
            if (attributeCategoryMappingsToRemove.Any())
            {
                _dataRepository.RemoveRange<PersonalFinanceAttributeCategoryMapping>(attributeCategoryMappingsToRemove);
            }

            //Add personal finance parent-child category mappings
            var existingParentChildCategoryMappings = await _dataRepository.GetAll<PersonalFinanceParentChildCategoryMapping>()
                                                        .Include(i => i.ParentCategory).Include(i => i.ChildCategory).ToListAsync();

            List<PersonalFinanceParentChildCategoryMapping> parentChildCategoryMappings = model.PersonalFinanceParentChildCategoryMappings.Select(s =>
                new PersonalFinanceParentChildCategoryMapping
                {
                    ParentCategory = categories.First(x => x.Name.Equals(model.PersonalFinanceCategories.First(y => y.Id.Equals(s.ParentCategoryId)).Name)),
                    ChildCategory = categories.First(x => x.Name.Equals(model.PersonalFinanceCategories.First(y => y.Id.Equals(s.ChildCategoryId)).Name)),
                    ParentAttributeCategoryMapping = flattenedAttributeList.FirstOrDefault(x => x.Category.Name.Equals(model.PersonalFinanceCategories.FirstOrDefault(z => z.Id.Equals(model.PersonalFinanceAttributeCategoryMappings.FirstOrDefault(y => y.Id.Equals(s.ParentAttributeCategoryMappingId))?.CategoryId))?.Name)
                        && x.Attribute.Text.Equals(model.PersonalFinanceAttributes.FirstOrDefault(z => z.Id.Equals(model.PersonalFinanceAttributeCategoryMappings.FirstOrDefault(y => y.Id.Equals(s.ParentAttributeCategoryMappingId))?.AttributeId))?.Text)
                        && x.Order.Equals(model.PersonalFinanceAttributeCategoryMappings.FirstOrDefault(y => y.Id.Equals(s.ParentAttributeCategoryMappingId))?.Order)
                    )
                }).ToList();

            List<PersonalFinanceParentChildCategoryMapping> parentChildCategoryMappingsToAdd = parentChildCategoryMappings.Where(x =>
                !existingParentChildCategoryMappings.Any(y => y.ParentCategory.Name.Equals(x.ParentCategory.Name) && y.ChildCategory.Name.Equals(x.ChildCategory.Name))).ToList();

            List<PersonalFinanceParentChildCategoryMapping> parentChildCategoryMappingsToRemove = existingParentChildCategoryMappings.Where(x =>
                !parentChildCategoryMappings.Any(y => y.ParentCategory.Name.Equals(x.ParentCategory.Name) && y.ChildCategory.Name.Equals(x.ChildCategory.Name))).ToList();

            if (parentChildCategoryMappingsToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<PersonalFinanceParentChildCategoryMapping>(parentChildCategoryMappingsToAdd);
            }
            if (parentChildCategoryMappingsToRemove.Any())
            {
                _dataRepository.RemoveRange<PersonalFinanceParentChildCategoryMapping>(parentChildCategoryMappingsToRemove);
            }

            await _dataRepository.SaveChangesAsync();
        }

        #endregion

        /// <summary>
        /// Seed method for additional document types
        /// </summary>
        /// <returns></returns>
        private async Task SeedAdditionalDocumentTypeAsync()
        {
            List<AdditionalDocumentType> existingDocumentTypes = await _dataRepository.GetAll<AdditionalDocumentType>().ToListAsync();
            string json = _fileOperationsUtility.ReadFileContent("Dataset_AdditionalDocumentTypes.json");
            var model = JsonConvert.DeserializeObject<List<AdditionalDocumentTypeSeedAC>>(json);

            List<AdditionalDocumentType> documentTypes = model.Select(s => new AdditionalDocumentType
            {
                Type = s.Type,
                IsEnabled = s.IsEnabled,
                DocumentTypeFor = (ResourceType)Enum.Parse(typeof(ResourceType), s.DocumentTypeFor)
            }).ToList();

            List<AdditionalDocumentType> documentTypesToAdd = documentTypes.Where(x => !existingDocumentTypes.Any(y => y.Type.Equals(x.Type) && y.DocumentTypeFor.Equals(x.DocumentTypeFor))).ToList();

            if (documentTypesToAdd.Any())
            {
                await _dataRepository.AddRangeAsync<AdditionalDocumentType>(documentTypesToAdd);
                await _dataRepository.SaveChangesAsync();
            }
        }

        #endregion

        #region Public Method(s)
        /// <summary>
        /// Seeds necessary data into the database.
        /// </summary>
        public async Task SeedAsync()
        {
            using (await _dataRepository.BeginTransactionAsync())
            {
                // Write private method here to seed data to the database.
                await SeedCompanyStructureAsync();
                await SeedBusinessAgeAsync();
                await SeedCompanySizeAsync();
                await SeedNAICSIndustryTypeAsync();
                await SeedIntegratedServiceConfigurationAsync();
                await SeedIndustryExperienceAsync();
                await SeedRelationshipAsync();
                await SeedFinancialStatementAsync();
                await SeedLoanRangeTypeAsync();
                await SeedLoanTypeAsync();
                await SeedLoanPurposeAsync();
                await SeedProductAsync();
                await SeedProductTypeMappingAsync();
                await SeedConsentsAsync();
                await SeedBanksAsync();
                await SeedSectionsAsync();
                await SeedTaxFormAsync();
                await SeedTaxFormCompanyStructureMappingAsync();
                await SeedLoanPurposeRangeTypeMappingAsync();
                await SeedTaxformLabelNameMappingAsync();
                await SeedOCRModelMappingAsync();
                await SeedUserWiseSectionAsync();
                await SeedPersonalFinanceStaticDataAsync();
                await SeedAdditionalDocumentTypeAsync();
                _dataRepository.CommitTransaction();
            }
        }

        #endregion
    }
}
