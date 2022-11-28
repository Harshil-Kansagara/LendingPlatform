using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.Constants;
using LendingPlatform.Utils.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LendingPlatform.Repository.Repository.EntityInfo
{
    public class EntityTaxReturnRepository : IEntityTaxReturnRepository
    {
        #region Private Variables
        private readonly IDataRepository _dataRepository;
        private readonly IConfiguration _configuration;
        private readonly IGlobalRepository _globalRepository;
        private readonly IMapper _mapper;
        private readonly IAmazonServicesUtility _amazonS3Utility;
        #endregion

        #region Constructor
        public EntityTaxReturnRepository(IDataRepository dataRepository, IGlobalRepository globalRepository, IMapper mapper, IAmazonServicesUtility amazonS3Utility, IConfiguration configuration)
        {
            _dataRepository = dataRepository;
            _configuration = configuration;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _amazonS3Utility = amazonS3Utility;
        }
        #endregion

        #region Public Method
        /// <summary>
        /// Method to get the tax return documents by entity id
        /// </summary>
        /// <param name="id">Entity Id or application Id</param>
        /// <param name="type">Enum ResourceType can be company or loan</param>
        /// <param name="currentUser">Current User</param>
        /// <returns>EntityAC object</returns>
        public async Task<EntityAC> GetTaxReturnInfoAsync(Guid id, ResourceType type, CurrentUserAC currentUser)
        {
            //Fetch linked entity Id with given application Id.
            var entityAC = new EntityAC();
            entityAC.Periods = new List<string>();
            entityAC.Taxes = new List<TaxAC>();

            var lastNYears = _configuration.GetValue<int>("TaxConfig:Years");
            while (lastNYears > 0)
            {
                entityAC.Periods.Add(DateTime.UtcNow.AddYears(-lastNYears).Year.ToString());
                lastNYears--;
            }

            List<EntityTaxForm> entityTaxForms = new List<EntityTaxForm>();

            if (type == ResourceType.Company)
            {
                if (!currentUser.IsBankUser && !await _globalRepository.CheckEntityRelationshipMappingAsync(id, currentUser.Id))
                {
                    throw new UnauthorizedAccessException(StringConstant.UnauthorizedResourceAccess);
                }

                var entityTaxFormsQueryable = _dataRepository.Fetch<EntityTaxForm>(x => x.EntityId == id);

                if (entityTaxFormsQueryable != null)
                {
                    if (entityTaxFormsQueryable.Any(x => x.LoanApplicationId == null))
                    {
                        // Fetch the taxes (with loanId null)
                        entityTaxForms = await entityTaxFormsQueryable.Where(x => x.LoanApplicationId == null)
                            .Include(x => x.TaxForm)
                            .ToListAsync();
                    }
                    // If non-versioned tax form doesn't exist, fetch latest versioned taxes, clone them and make a new non-versioned tax form
                    else
                    {
                        if (!currentUser.IsBankUser)
                            await CloneEntityTaxesAndRelatedTablesAsync(entityTaxFormsQueryable);
                        AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, id);
                        await _dataRepository.SaveChangesAsync(auditLog);
                        entityTaxForms = await _dataRepository.Fetch<EntityTaxForm>(x => x.EntityId.Equals(id) && x.LoanApplicationId == null).Include(x => x.TaxForm).ToListAsync();
                    }
                }
            }
            else if (type == ResourceType.Loan)
            {
                if (currentUser != null && !currentUser.IsBankUser && !await _globalRepository.CheckUserLoanAccessAsync(currentUser, id, !currentUser.IsBankUser))
                {
                    throw new UnauthorizedAccessException(StringConstant.UnauthorizedResourceAccess);
                }
                entityTaxForms = await _dataRepository.Fetch<EntityTaxForm>(x => x.LoanApplicationId.Equals(id)).GetLatestVersionForLoan().Include(x => x.TaxForm).ToListAsync();
            }

            if (entityTaxForms.Any())
            {
                entityAC.Id = entityTaxForms.First().EntityId;
                entityAC.Taxes.AddRange(_mapper.Map<List<TaxAC>>(entityTaxForms));
            }

            return entityAC;
        }

        /// <summary>
        /// Method is to get the list of extracted values from the document
        /// </summary>
        /// <param name="loanApplicationId">LoanApplication Id</param>
        /// <param name="documentId">Document Id</param>
        /// <param name="currentUser">Current User</param>
        /// <returns>List of TaxFormValueLabelMappingAC object</returns>
        public async Task<List<TaxFormValueLabelMappingAC>> GetExtractedValuesOfDocumentAsync(Guid loanApplicationId, Guid documentId, CurrentUserAC currentUser)
        {
            if (!currentUser.IsBankUser)
            {
                throw new UnauthorizedAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            var loanApplication = await _dataRepository.SingleAsync<LoanApplication>(x => x.Id == loanApplicationId);

            if (loanApplication.Status == LoanApplicationStatusType.Draft)
            {
                throw new DataNotFoundException();
            }
            else
            {
                var entityTaxYearlyMapping = await _dataRepository.SingleOrDefaultAsync<EntityTaxYearlyMapping>(x => x.DocumentId == documentId);
                if (entityTaxYearlyMapping == null)
                {
                    throw new InvalidParameterException();
                }
                var taxFormValueLabelMappings = await _dataRepository.Fetch<TaxFormValueLabelMapping>(x => x.EntityTaxYearlyMappingId == entityTaxYearlyMapping.Id).Include(x => x.TaxformLabelNameMapping).OrderBy(x => x.TaxformLabelNameMapping.Order).ToListAsync();
                return _mapper.Map<List<TaxFormValueLabelMapping>, List<TaxFormValueLabelMappingAC>>(taxFormValueLabelMappings);
            }

        }

        /// <summary>
        /// Method to add or update the tax return documents
        /// </summary>
        /// <param name="entityId">Entity Id</param>
        /// <param name="entityAC">EntityAC object</param>
        /// <param name="currentUser">Current User</param>
        /// <returns></returns>
        public async Task SaveTaxReturnInfoAsync(Guid entityId, EntityAC entityAC, CurrentUserAC currentUser)
        {
            if (!currentUser.IsBankUser && !await _globalRepository.CheckEntityRelationshipMappingAsync(entityId, currentUser.Id))
            {
                throw new UnauthorizedAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            List<EntityTaxForm> entityTaxFormsToAdd = new List<EntityTaxForm>();
            List<EntityTaxYearlyMapping> entityTaxYearlyMappingsToAdd = new List<EntityTaxYearlyMapping>();
            List<Document> documentsToAdd = new List<Document>();
            List<Guid> documentIdsToRemove = new List<Guid>();

            CheckFileValidation(entityAC);

            var entityTaxFormsDbData = await _dataRepository.Fetch<EntityTaxForm>(y => y.EntityId == entityId && y.LoanApplicationId == null)
                    .Include(y => y.TaxForm)
                    .Include(y => y.EntityTaxYearlyMappings).ToListAsync();

            if (entityTaxFormsDbData.Any())
            {
                if (entityAC.Taxes.All(x => x.EntityTaxAccount.Document.Id == null))
                {
                    documentIdsToRemove.AddRange(entityTaxFormsDbData.SelectMany(x => x.EntityTaxYearlyMappings.Select(y => y.DocumentId)).ToList());
                }
                else
                {
                    entityTaxFormsDbData.ForEach(x =>
                        documentIdsToRemove.AddRange(x.EntityTaxYearlyMappings.FindAll(x => !entityAC.Taxes.Select(y => y.EntityTaxAccount.Document.Id).Contains(x.DocumentId)).Select(x => x.DocumentId).ToList())
                    );
                }
            }

            // Tax Form need to be added 
            var entityTaxFormsNeedToAdd = entityAC.Taxes.Where(y => y.Id == Guid.Empty).ToList();
            var taxForms = await _dataRepository.GetAll<TaxForm>().ToListAsync();
            if (entityTaxFormsNeedToAdd.Any())
            {
                foreach (var entityTax in entityTaxFormsNeedToAdd)
                {
                    if (!entityTaxFormsDbData.Any() && !entityTaxFormsToAdd.Any())
                    {
                        entityTaxFormsToAdd.Add(new EntityTaxForm()
                        {
                            EntityId = entityId,
                            CreatedByUserId = currentUser.Id,
                            CreatedOn = DateTime.UtcNow,
                            TaxFormId = taxForms.Single(x => x.Name == StringConstant.TaxReturns).Id,
                            LoanApplicationId = null
                        });
                    }

                    if (entityTax.EntityTaxAccount.Document.Path.StartsWith(StringConstant.FileTemp))
                    {
                        var destinationObjectKey = _globalRepository.GetPathForKeyNameBucket(entityId, entityTax.EntityTaxAccount);

                        _amazonS3Utility.CopyObject(entityTax.EntityTaxAccount.Document.Path, destinationObjectKey);
                        entityTax.EntityTaxAccount.Document.Path = destinationObjectKey;
                    }
                    documentsToAdd.Add(_mapper.Map<DocumentAC, Document>(entityTax.EntityTaxAccount.Document));
                }
            }

            using (await _dataRepository.BeginTransactionAsync())
            {
                if (documentIdsToRemove.Any())
                {
                    await DocumentsToRemoveAsync(documentIdsToRemove);
                }

                // Add entity tax forms mapping
                if (entityTaxFormsToAdd.Any())
                {
                    await _dataRepository.AddRangeAsync<EntityTaxForm>(entityTaxFormsToAdd);
                    entityTaxFormsDbData = entityTaxFormsToAdd;
                }
                // Add Document and entity tax yearly mapping
                if (documentsToAdd.Any())
                {
                    await _dataRepository.AddRangeAsync<Document>(documentsToAdd);
                    foreach (var tax in entityAC.Taxes)
                    {
                        if (tax.Id == Guid.Empty)
                        {
                            entityTaxYearlyMappingsToAdd.Add(new EntityTaxYearlyMapping()
                            {
                                EntityTaxFormId = entityTaxFormsDbData.First().Id,
                                Period = tax.EntityTaxAccount.Period,
                                DocumentId = documentsToAdd.First(x => x.Name == tax.EntityTaxAccount.Document.Name).Id,
                                UploadedDocument = documentsToAdd.First(x => x.Name == tax.EntityTaxAccount.Document.Name)
                            });
                        }
                    }
                    await _dataRepository.AddRangeAsync<EntityTaxYearlyMapping>(entityTaxYearlyMappingsToAdd);
                }
                // Prepare the Auditlog object to save the custom fields in the dbcontext.
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Company, entityId);
                await _dataRepository.SaveChangesAsync(auditLog);
                _dataRepository.CommitTransaction();
            }
        }

        /// <summary>
        /// Method is to update the tax return value in the DB
        /// </summary>
        /// <param name="applicationId">application unique identifier</param>
        /// <param name="documentId">Document unique identifier</param>
        /// <param name="taxFormValueLabelMappings">List of TaxFormValueLabelMapping obj</param>
        /// <param name="currentUser">Current User</param>
        /// <returns>Updated value of TaxFormValueLabelMappingAC list object</returns>
        public async Task<List<TaxFormValueLabelMappingAC>> UpdateTaxFormValueAsync(Guid applicationId, Guid documentId, List<TaxFormValueLabelMappingAC> taxFormValueLabelMappings, CurrentUserAC currentUser)
        {
            //Only bank user is allowed to get all the loans.
            if (!currentUser.IsBankUser)
            {
                throw new UnauthorizedAccessException(StringConstant.UnauthorizedResourceAccess);
            }

            var checkEntityTaxYearlyMapping = await _dataRepository.Fetch<EntityTaxYearlyMapping>(x => x.DocumentId == documentId).Include(x => x.UploadedDocument).SingleOrDefaultAsync();
            if (checkEntityTaxYearlyMapping == null)
            {
                throw new InvalidParameterException();
            }
            var existingTaxFormValueLabelMappings = _dataRepository.Fetch<TaxFormValueLabelMapping>(x => x.EntityTaxYearlyMappingId == checkEntityTaxYearlyMapping.Id).Include(x => x.TaxformLabelNameMapping).OrderBy(x => x.TaxformLabelNameMapping.Order);
            // Disabling change tracking to improve performance
            _dataRepository.DisableChangeTracking();
            var existingTaxFormValueLabelMappingDetach = _dataRepository.DetachEntities(existingTaxFormValueLabelMappings, false);
            foreach (var existingTaxFormValueLabelMapping in existingTaxFormValueLabelMappingDetach)
            {
                if (taxFormValueLabelMappings.Any(x => x.Id == existingTaxFormValueLabelMapping.Id))
                {
                    existingTaxFormValueLabelMapping.CorrectedValue = taxFormValueLabelMappings.Single(x => x.Id == existingTaxFormValueLabelMapping.Id).CorrectedValue;
                }
            }
            existingTaxFormValueLabelMappingDetach = existingTaxFormValueLabelMappingDetach.SanitizeIdentifiers();
            var correctedTaxFormValueLabelMapping = _mapper.Map<List<TaxFormValueLabelMapping>, List<TaxFormValueLabelMappingAC>>(existingTaxFormValueLabelMappingDetach.OrderBy(x => x.TaxformLabelNameMapping.Order).ToList());

            using (await _dataRepository.BeginTransactionAsync())
            {
                // All objects that are queryable are intentionally kept queryable
                // Fetch the latest taxes (saved for any previous loan), save a copy of it with loanId
                // Detch EnitityTaxForm
                var entityTaxFormsQueryable = _dataRepository
                    .Fetch<EntityTaxForm>(x => x.Id.Equals(checkEntityTaxYearlyMapping.EntityTaxFormId));

                var latestTaxes = _dataRepository.DetachEntities(entityTaxFormsQueryable
                    .Include(x => x.EntityTaxYearlyMappings)
                    .ThenInclude(x => x.UploadedDocument)
                    .GetLatestVersionForLoan()).ToList();

                Guid version = Guid.NewGuid();
                var entityTaxYearlyMappingList = latestTaxes.SelectMany(x => x.EntityTaxYearlyMappings).ToList();
                var entityTaxYearlyMappingIdList = entityTaxYearlyMappingList.Select(x => x.Id).Distinct().ToList();
                var entityTaxYearlyMappingDataSet = _dataRepository.Fetch<EntityTaxYearlyMapping>(x => entityTaxYearlyMappingIdList.Contains(x.Id));

                var entityTaxYearlyDocumentIdList = entityTaxYearlyMappingList.Select(x => x.DocumentId).Distinct().ToList();
                var yearlyDocumentList = _dataRepository.Fetch<Document>(x => entityTaxYearlyDocumentIdList.Contains(x.Id));
                foreach (var tax in latestTaxes)
                {
                    tax.LoanApplicationId = applicationId;
                    tax.Version = version;
                    tax.EntityTaxYearlyMappings = _dataRepository.DetachEntities(entityTaxYearlyMappingDataSet.Where(x => tax.EntityTaxYearlyMappings.Select(y => y.Id).Contains(x.Id)));

                    foreach (var entityTaxYearly in tax.EntityTaxYearlyMappings)
                    {
                        entityTaxYearly.EntityTaxFormId = Guid.Empty;
                        if (entityTaxYearly.DocumentId == checkEntityTaxYearlyMapping.DocumentId)
                        {
                            entityTaxYearly.TaxFormValueLabelMappings = existingTaxFormValueLabelMappingDetach;
                            foreach (var taxFormValue in entityTaxYearly.TaxFormValueLabelMappings)
                            {
                                taxFormValue.TaxformLabelNameMapping = null;
                                taxFormValue.EntityTaxYearlyMappingId = Guid.Empty;
                            }
                        }
                        var uploadedDocument = yearlyDocumentList.Where(x => entityTaxYearly.DocumentId == x.Id);
                        entityTaxYearly.UploadedDocument = _dataRepository.DetachEntities(uploadedDocument).Single();
                        entityTaxYearly.DocumentId = Guid.Empty;
                    }
                }
                await _dataRepository.AddRangeAsync(latestTaxes);
                AuditLog auditLog = _globalRepository.GetAuditLogForCustomFields(currentUser, ResourceType.Loan, applicationId);
                _dataRepository.EnableChangeTracking();
                await _dataRepository.SaveChangesAsync(auditLog);
                _dataRepository.CommitTransaction();
            }
            return correctedTaxFormValueLabelMapping;
        }

        /// <summary>
        /// Method is to get the taxes data from the DB
        /// </summary>
        /// <param name="taxes">List of TaxAC object</param>
        /// <param name="entityTaxForms">List of EntityTaxForms object</param>
        /// <returns>List of TaxAC project</returns>
        public List<TaxAC> GetTaxes(List<TaxAC> taxes, List<EntityTaxForm> entityTaxForms)
        {
            var entityTaxYearlyMappings = _dataRepository.GetAll<EntityTaxYearlyMapping>().Include(x => x.UploadedDocument).Where(x => entityTaxForms.Select(y => y.Id).Contains(x.EntityTaxFormId)).Include(x => x.TaxFormValueLabelMappings).ToListAsync().Result;

            if (!entityTaxYearlyMappings.Any())
            {
                throw new InvalidParameterException(StringConstant.NoDocumentFound);
            }

            foreach (var entityTaxYearlyMapping in entityTaxYearlyMappings)
            {
                foreach (var entityTaxForm in entityTaxForms)
                {
                    if (entityTaxYearlyMapping.EntityTaxFormId == entityTaxForm.Id)
                    {
                        var taxAC = new TaxAC();
                        taxAC.Id = entityTaxForm.Id;
                        taxAC.CreationDateTime = _globalRepository.ConvertUtcDateToLocalDate(entityTaxForm.CreatedOn);
                        taxAC.EntityTaxAccount = _mapper.Map<EntityTaxYearlyMapping, EntityTaxAccountAC>(entityTaxYearlyMapping);
                        taxAC.EntityTaxAccount.Document = _mapper.Map<Document, DocumentAC>(entityTaxYearlyMapping.UploadedDocument);
                        taxes.Add(taxAC);
                    }
                }
            }
            return taxes;
        }

        #endregion

        #region Private Method
        /// <summary>
        /// Method is to check file validation conditions
        /// </summary>
        /// <param name="entityAC">EntityAC object</param>
        private void CheckFileValidation(EntityAC entityAC)
        {
            var isTaxFormJourneyAllowed = _configuration.GetValue<bool>("TaxConfig:IsTaxFormJourneyAllowed");
            if (isTaxFormJourneyAllowed)
            {
                var minimumNumberOfYearlyTaxForm = _configuration.GetValue<string>("TaxConfig:MinimumNumberOfYearlyTaxForm");

                // Validation for If only one tax return file needed or each year file needed
                int totalTaxCount = entityAC.Periods.Count;
                int count = entityAC.Taxes.Count;

                // If all year tax return file needed
                if (minimumNumberOfYearlyTaxForm.Equals("All") && count != totalTaxCount)
                {
                    throw new ValidationException(StringConstant.AllTaxFormValidationError);
                }
                // If anyone year tax return file needed
                else if (minimumNumberOfYearlyTaxForm.Equals("AnyOne") && count == 0)
                {
                    throw new ValidationException(StringConstant.AtleastOneTaxFormValidationError);
                }

                // Check if document comes without adding tax year
                var documentWithEmptyPeriodCount = entityAC.Taxes.Count(x => x.EntityTaxAccount.Period == null);
                if (documentWithEmptyPeriodCount > 0)
                {
                    throw new ValidationException(StringConstant.DocumentPeriodEmpty);
                }
            }
            else
            {
                throw new InvalidResourceAccessException();
            }
        }

        /// <summary>
        /// Method is to remove the documents from database and S3 bucket
        /// </summary>
        /// <param name="documentIds">Document Id</param>
        /// <returns></returns>
        private async Task DocumentsToRemoveAsync(List<Guid> documentIds)
        {
            List<Document> documentsToRemove = await _dataRepository.Fetch<Document>(x => documentIds.Contains(x.Id)).ToListAsync();

            if (!documentsToRemove.Any())
            {
                var entityTaxYearlyMappings = await _dataRepository.Fetch<EntityTaxYearlyMapping>(x => documentIds.Contains(x.DocumentId)).ToListAsync();
                _dataRepository.RemoveRange<EntityTaxYearlyMapping>(entityTaxYearlyMappings);
            }

            await _amazonS3Utility.DeleteObjectsAsync(documentsToRemove.Select(x => x.Path).ToList());

            _dataRepository.RemoveRange<Document>(documentsToRemove);
        }

        /// <summary>
        /// Clone rows of entitytaxform, and all child tables with sanitizing ids
        /// </summary>
        /// <param name="entityTaxFormsQueryable"></param>
        /// <returns></returns>
        private async Task CloneEntityTaxesAndRelatedTablesAsync(IQueryable<EntityTaxForm> entityTaxFormsQueryable)
        {
            // Disabling change tracking to improve performance
            _dataRepository.DisableChangeTracking();
            // All objects that are queryable are intentionally kept queryable
            // Fetch the latest taxes (saved for any previous loan), save a copy of it with loanId null
            // Detch EnitityTaxForm
            var latestTaxes = _dataRepository.DetachEntities(entityTaxFormsQueryable.Include(x => x.EntityTaxYearlyMappings).ThenInclude(x => x.UploadedDocument).GetLatestVersionForLoan()).ToList();

            var entityTaxYearlyMappingList = latestTaxes.SelectMany(x => x.EntityTaxYearlyMappings).ToList();
            var entityTaxYearlyMappingIdList = entityTaxYearlyMappingList.Select(x => x.Id).Distinct().ToList();
            var entityTaxYearlyMappingDataSet = _dataRepository.Fetch<EntityTaxYearlyMapping>(x => entityTaxYearlyMappingIdList.Contains(x.Id));
            var entityTaxYearlyDocumentIdList = entityTaxYearlyMappingList.Select(x => x.DocumentId).Distinct().ToList();
            var yearlyDocumentList = _dataRepository.Fetch<Document>(x => entityTaxYearlyDocumentIdList.Contains(x.Id));
            var taxFormValueLabelMappingList = _dataRepository.Fetch<TaxFormValueLabelMapping>(x => entityTaxYearlyMappingIdList.Contains(x.EntityTaxYearlyMappingId));
            foreach (var tax in latestTaxes)
            {
                var entityTaxYearlyMapping = entityTaxYearlyMappingDataSet.Where(x => tax.EntityTaxYearlyMappings.Select(x => x.Id).Contains(x.Id));
                tax.EntityTaxYearlyMappings = _dataRepository.DetachEntities(entityTaxYearlyMapping);
                foreach (var entityTaxYearly in tax.EntityTaxYearlyMappings)
                {
                    var uploadedDocument = yearlyDocumentList.Where(x => entityTaxYearly.DocumentId == x.Id);
                    entityTaxYearly.UploadedDocument = _dataRepository.DetachEntities(uploadedDocument).Single();
                    if (entityTaxYearly.TaxFormValueLabelMappings != null)
                    {
                        var taxFormValueLabelMappings = taxFormValueLabelMappingList.Where(x => entityTaxYearly.TaxFormValueLabelMappings.Select(y => y.Id).Contains(x.Id));
                        entityTaxYearly.TaxFormValueLabelMappings = _dataRepository.DetachEntities(taxFormValueLabelMappings);
                    }
                }
            }
            await _dataRepository.AddRangeAsync(latestTaxes);

            _dataRepository.EnableChangeTracking();
        }
        #endregion
    }
}
