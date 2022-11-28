using AutoMapper;
using LendingPlatform.Core.Helpers;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.Application;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LendingPlatform.Core.Controllers
{
    [OpenApiTag("Application(Loan)", Description = "Loan application related APIs Which includes all the APIs to capture/update/retrieve all the information related to loan application.")]
    [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
    [Produces(StringConstant.HttpHeaderAcceptJsonType)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route(BaseUrl)]
    [ApiController]
    [Authorize(Roles = StringConstant.UserAllRoles)]
    public class ApplicationController : BaseApiController
    {
        #region Private variables
        private const string BaseUrl = "api/applications";
        private readonly IApplicationRepository _applicationRepository;
        private readonly IGlobalRepository _globalRepository;
        private readonly IProductRepository _productRepository;
        private readonly IEntityTaxReturnRepository _entityTaxReturnRepository;
        private readonly IEntityFinanceRepository _entityFinanceRepository;
        private readonly IEntityRepository _entityRepository;
        #endregion

        #region Constructor
        public ApplicationController(IDataRepository dataRepository,
            IApplicationRepository applicationRepository,
            IMapper mapper, IGlobalRepository globalRepository,
            IProductRepository productRepository,
            IEntityTaxReturnRepository entityTaxReturnRepository,
            IEntityFinanceRepository entityFinanceRepository,
            IEntityRepository entityRepository)
            : base(dataRepository, mapper)
        {
            _applicationRepository = applicationRepository;
            _globalRepository = globalRepository;
            _productRepository = productRepository;
            _entityTaxReturnRepository = entityTaxReturnRepository;
            _entityFinanceRepository = entityFinanceRepository;
            _entityRepository = entityRepository;
        }
        #endregion

        #region Public methods

        #region Loan needs

        /// <summary>
        /// Retrieve list of all applications.
        /// </summary>
        /// <remarks>
        /// This method will retrieve a list of all loan applications with details according to the filtering parameters.
        /// </remarks>
        /// <param name="filterModel">Filter object</param>
        /// <response code="200">Returns list of loans</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = StringConstant.BankUserRole)]
        [ProducesResponseType(typeof(PagedLoanApplicationsAC), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PagedLoanApplicationsAC>> GetLoanApplicationListAsync([FromQuery] FilterModelAC filterModel)
        {
            return Ok(await _applicationRepository.GetAllLoanApplicationsAsync(filterModel, CurrentUserAC));
        }

        /// <summary>
        /// Retrieve the loan application details.
        /// </summary>
        /// <remarks>
        /// This method will retrieve the loan application's basic details along with its borrowers' details, consents' details, etc..
        /// </remarks>
        /// <param name="id">Id of the loan application whose details are to be fetched</param>
        /// <response code="200">Returns the list of recommended loan products</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <response code="204">If the relevant data not found</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApplicationAC), (int)HttpStatusCode.OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<ApplicationAC>> GetLoanApplicationDetailsByIdAsync([FromRoute] Guid id)
        {
            if (ModelState.IsValid && id != Guid.Empty)
            {
                return Ok(await _applicationRepository.GetLoanApplicationDetailsByIdAsync(id, CurrentUserAC));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Capture a loan application.
        /// </summary>
        /// <remarks>
        /// This method will save the new loan application with its basic details which include loan amount, type, subtype, and period.
        /// </remarks>
        /// <param name="application">Application is the object which can be use to fill the loan application's basic details</param>
        /// <response code="201">Returns the newly created loan application object</response>
        /// <response code="400">If the object has invalid field or it is null</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApplicationBasicDetailAC), (int)HttpStatusCode.Created)]
        public async Task<ActionResult<ApplicationBasicDetailAC>> AddLoanApplicationBasicDetailsAsync([FromBody] ApplicationBasicDetailAC application)
        {
            // Check if model is valid.
            if (ModelState.IsValid)
            {
                return StatusCode((int)HttpStatusCode.Created, await _applicationRepository.SaveLoanApplicationAsync(application, CurrentUserAC, null));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Update an existing loan application.
        /// </summary>
        /// <remarks>
        /// This method will update the basic details of an existing loan application which include loan amount, type, subtype, and period.
        /// </remarks>
        /// <param name="application">Application is the object which can be use to fill the loan application's basic details</param>
        /// <param name="id">Id of the loan application whose details need to be updated</param>
        /// <response code="200">Returns the updated loan application object</response>
        /// <response code="400">If the object has invalid field or it is null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApplicationBasicDetailAC), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApplicationBasicDetailAC>> UpdateLoanApplicationBasicDetailsAsync([FromRoute] Guid id, [FromBody] ApplicationBasicDetailAC application)
        {
            // Check if model is valid.
            if (ModelState.IsValid)
            {
                return Ok(await _applicationRepository.SaveLoanApplicationAsync(application, CurrentUserAC, id));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Delete all loan application
        /// </summary>
        /// <remarks>
        /// This method will enable delete of all loan applications by authorized bank users
        /// </remarks>
        /// <response code="200"></response>
        /// <response code="403">If the user haven't permission to access the request</response>
        [Authorize(Roles = StringConstant.BankUserRole)]
        [HttpDelete()]
        public async Task DeleteAllLoanApplicationAsync()
        {
            await _applicationRepository.DeleteLoanApplicationsAsync(CurrentUserAC);
        }

        #endregion

        #region Loan product
        /// <summary>
        /// Retrieve list of recommended products.
        /// </summary>
        /// <remarks>
        ///  This API will retrieve list of recommended loan products for the specific loan application.
        /// </remarks>
        /// <param name="id">Id of the loan application for which the recommended loan products are to be fetched</param>
        /// <response code="200">Returns the list of recommended loan products</response>
        /// <response code="204">If the relevant data not found</response>
        /// <response code="403">If the request doesn't authenticate to access the resource</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        [HttpGet("{id}/recommended-products")]
        [ProducesResponseType(typeof(List<RecommendedProductAC>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<RecommendedProductAC>>> GetRecommendedLoanProductsAsync([FromRoute] Guid id)
        {
            if (id != Guid.Empty)
            {
                return Ok(await _productRepository.GetRecommendedProductsListAsync(id, CurrentUserAC));
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Retrieve the detail of selected product as per loan application
        /// </summary>
        /// <remarks>
        /// This API will retrieve selected product as per loan application
        /// </remarks>
        /// <param name="id">Loan Application Id</param>
        /// <returns>ApplicationDetailAC object</returns>
        /// <response code="200">Returns the list of recommended loan products</response>
        /// <response code="403">If the request doesn't authenticate to access the resource</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        [HttpGet]
        [Route("{id}/product")]
        [ProducesResponseType(typeof(RecommendedProductAC), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<RecommendedProductAC>> GetSelectedProductAsync([FromRoute] Guid id)
        {
            if (id != Guid.Empty)
            {
                var recommendedProducts = await _productRepository.GetRecommendedProductsListAsync(id, CurrentUserAC, true);
                return Ok(await _productRepository.GetSelectedProductDataAsync(id, CurrentUserAC, recommendedProducts));
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Capture loan product
        /// </summary>
        /// <remarks>
        /// This API capture loan product for current loan application
        /// </remarks>
        /// <param name="id">Loan application id</param>
        /// <param name="productId">Product Id</param>
        /// <response code="201">Return status created when product is link with loan application</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/product/{productId}")]
        [ProducesResponseType(typeof(ApplicationAC), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> SaveProductAsync([FromRoute] Guid id, [FromRoute] Guid productId)
        {
            if (ModelState.IsValid && id != Guid.Empty && productId != Guid.Empty)
            {
                await _productRepository.SaveLoanProductAsync(id, productId, CurrentUserAC);
                return StatusCode((int)HttpStatusCode.Created);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        #endregion

        #region Bank details

        /// <summary>
        /// Retrieve the bank details of the loan application.
        /// </summary>
        /// <remarks>
        /// This method will retrieve the bank details information along with deposite bank details, EMI deduct bank details.
        /// </remarks>
        /// <param name="id">Id of the loan application whose details are to be fetched</param>
        /// <response code="200">Returns the bank details.</response>
        /// <response code="204">If the relevant data not found.</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        [HttpGet("{id}/bank-info")]
        [ProducesResponseType(typeof(LoanEntityBankDetailsAC), (int)HttpStatusCode.OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<LoanEntityBankDetailsAC>> GetBankDetailsOfLoanApplicationAsync([FromRoute] Guid id)
        {
            return Ok(await _applicationRepository.GetBankDetailsByLoanIdAsync(id, CurrentUserAC));
        }

        /// <summary>
        /// Capture the loan application's bank informations.
        /// </summary>
        /// <remarks>
        /// This method will save the details of both deductee and depositee banks of a loan application, which include bank account number, name and swift code.
        /// </remarks>
        /// <param name="loanEntityBankDetailsAC">LoanEntityBankDetailsAC is the object which can be use to fill the details of entity's banks for a particular loan application.</param>
        /// <response code="201">Returns the status of successful add operation</response>
        /// <response code="204">If the relevant data not found</response>
        /// <response code="400">If the object has invalid field or it is null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        [HttpPost("{id}/bank-info")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> AddBankDetailsAsync([FromBody] LoanEntityBankDetailsAC loanEntityBankDetailsAC)
        {
            if (ModelState.IsValid)
            {
                await _applicationRepository.AddOrUpdateBankDetailsAsync(loanEntityBankDetailsAC, CurrentUserAC);
                return StatusCode((int)HttpStatusCode.Created);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Update the loan application's bank informations.
        /// </summary>
        /// <remarks>
        /// This method will update the details of both deductee and depositee banks of a loan application, which include bank account number, name and swift code.
        /// </remarks>
        /// <param name="loanEntityBankDetailsAC">LoanEntityBankDetailsAC is the object which can be use to fill the details of entity's banks for a particular loan application.</param>
        /// <response code="200">Returns the status of successful update operation</response>
        /// <response code="204">If the relevant data not found</response>
        /// <response code="400">If the object has invalid field or it is null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        [HttpPut("{id}/bank-info")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> UpdateBankDetailsAsync([FromBody] LoanEntityBankDetailsAC loanEntityBankDetailsAC)
        {
            if (ModelState.IsValid)
            {
                await _applicationRepository.AddOrUpdateBankDetailsAsync(loanEntityBankDetailsAC, CurrentUserAC);
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        #endregion

        #region Entity
        /// <summary>
        /// Link loan application with borrowing entity.
        /// </summary>
        /// <remarks>
        /// Link loan application with borrowing entity..
        /// </remarks>
        /// <param name="applicationId">Unique identifier of application object</param>
        /// <param name="borrowingEntityId">Unique identifier of entity object</param>
        /// <response code="201">Returns nothing</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpGet("applications/{applicationId}/entity/{borrowingEntityId}")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> LinkApplicationWithEntityAsync([FromRoute] Guid applicationId, [FromRoute] Guid borrowingEntityId)
        {
            if (ModelState.IsValid && applicationId != Guid.Empty && borrowingEntityId != Guid.Empty)
            {
                await _applicationRepository.LinkApplicationWithEntityAsync(applicationId, borrowingEntityId, CurrentUserAC);
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Retrieve company finances for a loan application
        /// </summary>
        /// <remarks>
        /// Retrieve company finances for a loan application
        /// </remarks>
        /// <param name="id">Id of Loan for which financial information is needed</param>
        /// <param name="statementCsv">Comma separated list of report names. Any of these ["Income Statement","Balance Sheet","Cash Flow","Financial Ratios"]</param>
        /// <response code="200">Retrieve the finances</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <response code="204">If there is no finance available for the loan</response>
        /// <returns></returns>
        [HttpGet("{id}/finances/company/{statementCsv}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CompanyFinanceAC>>> GetFinancesAsync(Guid id, string statementCsv)
        {
            return Ok(await _entityFinanceRepository.GetFinancesAsync(id, ResourceType.Loan, statementCsv, CurrentUserAC));
        }

        /// <summary>
        /// Retrieve personal finances for a loan application
        /// </summary>
        /// <remarks>
        /// Retrieve personal finances of all shareholders linked with given loan application.
        /// </remarks>
        /// <param name="id">Id of an application for which finances need to be fetched</param>
        /// <param name="scopeCsv">Comma separated list of scopes of data to be fetched ex ["details","summary"]</param>
        /// <response code="200">Retrieved the finances</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <response code="204">If there is no finance available for the loan</response>
        /// <returns></returns>
        [HttpGet("{id}/finances/personal/{scopeCsv}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EntityAC>>> GetPersonalFinancesAsync([FromRoute] Guid id, [FromRoute] string scopeCsv)
        {
            return Ok(await _entityFinanceRepository.FetchPersonalFinancesForApplicationAsync(id, scopeCsv, CurrentUserAC));
        }

        #region Credit Report
        /// <summary>
        /// Fetch credit report.
        /// </summary>
        /// <remarks>
        /// This API will fetch entity's credit report saved for given loan application.
        /// </remarks>
        /// <param name="id">Unique identifier for a loan application</param>
        /// <param name="entityId">Unique identifier for entity</param>
        /// <response code="200">Successfully fetched and saved</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpPost("{id}/credit-report/{entityId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> FetchCreditReportAsync([FromRoute] Guid id, [FromRoute] Guid entityId)
        {
            await _entityRepository.FetchCreditReportAsync(entityId, id, CurrentUserAC);
            return Ok();
        }

        /// <summary>
        /// Get credit report.
        /// </summary>
        /// <remarks>
        /// This API will fetch the entity's latest credit report for a given loan.
        /// </remarks>
        /// <param name="id">Unique identifier for a loan application</param>
        /// <param name="entityId">Unique identifier for entity</param>
        /// <response code="200">Retrieve the credit report</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        /// <response code="204">If credit report doesn't exist</response>
        /// <returns></returns>
        [HttpGet("{id}/credit-report/{entityId}")]
        [ProducesResponseType(typeof(EntityAC), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<EntityAC>> GetCreditReportAsync([FromRoute] Guid id, [FromRoute] Guid entityId)
        {
            EntityAC entity = await _entityRepository.GetCreditReportAsync(entityId, id);
            return Ok(entity);
        }
        #endregion

        #endregion

        #region SectionName

        /// <summary>
        /// Update the current section name
        /// </summary>
        /// <param name="id">Loan application id</param>
        /// <param name="currentSectionName">Current Section Name</param>
        /// <response code="200">Returns the string of updated section name</response>
        /// <response code="204">If the relevant data not found</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns>Updated section name</returns>
        [HttpPut("{id}/progress/{currentSectionName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<string>> UpdateCurrentSectionNameAsync([FromRoute] Guid id, [FromRoute] string currentSectionName)
        {
            if (id != Guid.Empty || currentSectionName != null)
            {
                try
                {
                    return Ok(await _globalRepository.UpdateSectionNameAsync(id, currentSectionName, CurrentUserAC));
                }
                catch (DataNotFoundException)
                {
                    return NoContent();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        #endregion

        #region Consent

        /// <summary>
        /// Capture the consent of an entity for an application.
        /// </summary>
        /// <remarks>
        /// This method will save the consent of a logged in user for given loan application.
        /// </remarks>
        /// <param name="id">Application id for which the user's consent needs to be saved</param>
        /// <response code="200">Returns the OK if the consent is saved successfully</response>
        /// <response code="400">If the invalid application id is sent</response>
        [HttpPost("{id}/consent")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> SaveLoanConsentOfUserAsync([FromRoute] Guid id)
        {
            //If the provided loan application id is not empty then only call the repository method.
            if (id != Guid.Empty)
            {
                await _applicationRepository.SaveLoanConsentOfUserAsync(id, CurrentUserAC);
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Lock the application with its details.
        /// </summary>
        /// <remarks>
        /// This method will lock the loan application with all its details.
        /// </remarks>
        /// <param name="id">Id of the loan application which needs to locked</param>
        /// <response code="200">Returns the OK if the loan application is locked successfully</response>
        /// <response code="400">If the invalid application id is sent</response>
        [HttpPost("{id}/lock")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> LockLoanApplicationAsync([FromRoute] Guid id)
        {
            //If the provided loan application id is not empty then only call the repository method.
            if (id != Guid.Empty)
            {
                //Fetch personal finance summary JSON for all shareholders of given loan
                List<EntityAC> entities = await _entityFinanceRepository.FetchPersonalFinancesForApplicationAsync(id, StringConstant.PersonalFinanceSummary, CurrentUserAC);

                //Lock the loan application
                await _applicationRepository.LockLoanApplicationByIdAsync(id, entities, CurrentUserAC);
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Evaluate loan and get loan status
        /// </summary>
        /// <param name="id">Loan application Id</param>
        /// <response code="200">Returns the OK if the loan application is evaluated successfully</response>
        /// <response code="400">If the invalid application id is sent</response>
        /// <response code="403">If the application to evaluate is still not locked</response>
        /// <returns></returns>
        [HttpGet("{id}/status")]
        [ProducesResponseType(typeof(ApplicationBasicDetailAC), (int)HttpStatusCode.OK)]
        public async Task<ApplicationBasicDetailAC> EvaluateLoanAndGetLoanStatusAsync(Guid id)
        {
            return await _applicationRepository.EvaluateLoanAsync(id, CurrentUserAC);
        }

        /// <summary>
        /// Update the loan application's status.
        /// </summary>
        /// <remarks>
        /// This method will update the status of a loan application.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="application">Application basic details' object with updated values</param>
        /// <response code="200">Returns the status of successful update operation</response>
        /// <response code="400">If the object has invalid field or it is null</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(ApplicationBasicDetailAC), (int)HttpStatusCode.OK)]
        [Authorize(Roles = StringConstant.BankUserRole)]
        public async Task<ActionResult<ApplicationBasicDetailAC>> UpdateLoanApplicationStatusAsync([FromRoute] Guid id, [FromBody] ApplicationBasicDetailAC application)
        {
            if (ModelState.IsValid && !id.Equals(Guid.Empty))
            {
                return Ok(await _applicationRepository.UpdateLoanApplicationStatusAsync(application, CurrentUserAC));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Retrieve extracted value of document
        /// </summary>
        /// <remarks>
        /// This API will retrieve list of extracted value of document
        /// </remarks>
        /// <param name="id">LoanApplication Id</param>
        /// <param name="documentId">Document Id</param>
        /// <returns>List of TaxFormValueLabelMappingAC objects</returns>
        /// <response code="200">Returns the list of TaxFormValueLabelMapping object</response>
        /// <response code="204">No relevant data found</response>
        /// <response code="400">If the object has invalid field or it is null</response>
        [HttpGet("{id}/documentExtractedValue/{documentId}")]
        [ProducesResponseType(typeof(List<TaxFormValueLabelMappingAC>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = StringConstant.BankUserRole)]
        public async Task<ActionResult<List<TaxFormValueLabelMappingAC>>> GetDocumentExtractedValueAsync([FromRoute] Guid id, [FromRoute] Guid documentId)
        {
            if (id != Guid.Empty && documentId != Guid.Empty)
            {
                return Ok(await _entityTaxReturnRepository.GetExtractedValuesOfDocumentAsync(id, documentId, CurrentUserAC));
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Update the tax forms values related to the given application for entity
        /// </summary>
        /// <remarks>
        /// This API will be updating the tax forms values related to the given application for entity
        /// </remarks>
        /// <param name="id">Current application unique identifier </param>
        /// <param name="taxFormValueLabelMappings">List of updated taxFormValueLabelMappingAC object</param>
        /// <param name="documentId">Document unique identifier</param>
        /// <returns></returns>
        /// <response code="200">Returns success code </response>
        /// <response code="400">If the object has invalid field or it is null</response>
        [HttpPut("{id}/taxes/{documentId}")]
        [ProducesResponseType(typeof(List<TaxFormValueLabelMappingAC>), (int)HttpStatusCode.OK)]
        [Authorize(Roles = StringConstant.BankUserRole)]
        public async Task<ActionResult<List<TaxFormValueLabelMappingAC>>> UpdateTaxReturnValueAsync([FromRoute] Guid id, [FromRoute] Guid documentId, [FromBody] List<TaxFormValueLabelMappingAC> taxFormValueLabelMappings)
        {
            if (ModelState.IsValid && id != Guid.Empty && documentId != Guid.Empty && taxFormValueLabelMappings.Any())
            {
                return Ok(await _entityTaxReturnRepository.UpdateTaxFormValueAsync(id, documentId, taxFormValueLabelMappings, CurrentUserAC));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        #endregion

        #region AuditLog

        /// <summary>
        /// Retrieve the loan application audit logs.
        /// </summary>
        /// <remarks>
        /// This method will retrieve the loan application's audit logs.
        /// </remarks>
        /// <param name="id">Id of the loan application.</param>
        /// <response code="200">Returns the list of Audit logs</response>
        /// <response code="400">If the request parameter is invalid</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        [HttpGet("{id}/activity-logs")]
        [ProducesResponseType(typeof(List<AuditDateWiseLogsAC>), StatusCodes.Status200OK)]
        [Authorize(Roles = StringConstant.BankUserRole)]
        public async Task<ActionResult<List<AuditDateWiseLogsAC>>> GetAuditLogsAsync([FromRoute] Guid id)
        {
            AuditLogFilterAC auditLogFilter = new AuditLogFilterAC() { LogBlockNameId = id };
            var result = await _globalRepository.GetAuditLogsByLogBlockNameIdAsync(CurrentUserAC, auditLogFilter);
            return Ok(result);
        }
        #endregion

        #region Taxes
        /// <summary>
        /// Retrieve the list of uploaded tax forms for the loan
        /// </summary>
        /// <remarks>
        /// This API will retrieve the list of uploaded tax forms for the given loan
        /// </remarks>
        /// <param name="id">Application Id</param>
        /// <response code="200">Returns the list of tax forms linked with given loan</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        [HttpGet("{id}/taxes")]
        [ProducesResponseType(typeof(EntityAC), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<EntityAC>> GetTaxListByApplicationIdAsync([FromRoute] Guid id)
        {
            if (id != Guid.Empty)
            {
                return Ok(await _entityTaxReturnRepository.GetTaxReturnInfoAsync(id, ResourceType.Loan, CurrentUserAC));
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion

        #region Additional documents

        /// <summary>
        /// Retrieve the list of additional documents of a loan application
        /// </summary>
        /// <remarks>
        /// This API will retrieve the additional documents uploaded in given loan application.
        /// </remarks>
        /// <param name="id">Id of a loan application whose additional documents need to be fetched</param>
        /// <response code="200">Retrieved the additional documents</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <response code="204">If there is no required data available</response>
        /// <returns></returns>
        [HttpGet("{id}/additional-documents")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<EntityAC>> GetAdditionalDocumentsForApplicationAsync([FromRoute] Guid id)
        {
            return Ok(await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(id, ResourceType.Loan, CurrentUserAC));
        }

        #endregion

        #endregion
    }
}
