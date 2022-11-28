using AutoMapper;
using LendingPlatform.Core.Helpers;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Enums;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Repository.ApplicationClass.Applications;
using LendingPlatform.Repository.ApplicationClass.Entity;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LendingPlatform.Core.Controllers
{
    [OpenApiTag("Entity", Description = "All the entity related API. Which includes all the api's of entity and also financial details linked with entity.")]
    [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
    [Produces(StringConstant.HttpHeaderAcceptJsonType)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route(BaseUrl)]
    [ApiController]
    [Authorize(Roles = StringConstant.UserAllRoles)]

    public class EntityController : BaseApiController
    {
        #region Private Methods
        private const string BaseUrl = "api/entities";
        private readonly IEntityRepository _entityRepository;
        private readonly IEntityFinanceRepository _entityFinanceRepository;
        private readonly IConfiguration _configuration;
        private readonly IGlobalRepository _globalRepository;
        private readonly IEntityTaxReturnRepository _entityTaxReturnRepository;
        #endregion

        #region Constructor

        public EntityController(IDataRepository dataRepository,
            IEntityRepository entityRepository,
            IEntityFinanceRepository entityFinanceRepository,
            IMapper mapper,
            IConfiguration configuration,
            IGlobalRepository globalRepository,
            IEntityTaxReturnRepository entityTaxReturnRepository)
            : base(dataRepository, mapper)
        {
            _entityRepository = entityRepository;
            _entityFinanceRepository = entityFinanceRepository;
            _configuration = configuration;
            _entityTaxReturnRepository = entityTaxReturnRepository;
            _globalRepository = globalRepository;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Capture credit profile information.
        /// </summary>
        /// <remarks>
        /// Save the user credit profile information and return boolean if it's valid user credit profile self declartion.
        /// </remarks>
        /// <param name="entity">Entity object</param>
        /// <response code="201">Returns OkResult object that produces an empty Http.StatusCodes.Status200OK</response>
        /// <response code="400">If the request parameter is invalid or null.</response> 
        /// <returns>OkResult object</returns>
        [HttpPost("credit-profile")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<bool>> AddUserCreditProfileAsync([FromBody] EntityAC entity)
        {
            if (ModelState.IsValid && entity != null && !CurrentUserAC.IsBankUser && entity.Id == CurrentUserAC.Id)
            {
                return Ok(await _entityRepository.UpdateUserCreditProfileInformationAsync(entity));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Retrieve the list of loan applications.
        /// </summary>
        /// <remarks>
        /// This method will retrieve the list of loan applications linked with given entity with their basic details. 
        /// </remarks>
        /// <param name="id">Id of an entity whose loan application list is to be retrived</param>
        /// <response code="200">Returns the list of applications linked with given entity</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        [HttpGet("{id}/applications")]
        [ProducesResponseType(typeof(List<ApplicationBasicDetailAC>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ApplicationBasicDetailAC>>> GetLoanApplicationListByEntityIdAsync([FromRoute] Guid id)
        {
            if (id != Guid.Empty)
            {
                return Ok(await _entityRepository.GetLoanApplicationsWithBasicDetailsByEntityIdAsync(id, CurrentUserAC));
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Check if entity has any open loan application.
        /// </summary>
        /// <remarks>
        /// Method to check if entity has any open loan application.
        /// </remarks>
        /// <param name="entityId">Unique identifier of entity object</param>
        /// <response code="201">Returns true or false whether entity is allowed to start new application or not.</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpGet("{entityId}/loans/draft")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<bool>> CheckEntityAllowToStartNewApplicationAsync([FromRoute] Guid entityId)
        {
            if (ModelState.IsValid && entityId != Guid.Empty)
            {
                return Ok(await _entityRepository.CheckEntityAllowToStartNewApplicationAsync(entityId, CurrentUserAC));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Create an entity.
        /// </summary>
        /// <remarks>
        /// It will create an entity and also link related details of the provided entity
        /// </remarks>
        /// <param name="type">Type of entity</param>
        /// <param name="entity">Entity object</param>
        /// <response code="201">Returns the newly created entity object</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpPost("{type}")]
        [ProducesResponseType(typeof(EntityAC), (int)HttpStatusCode.Created)]
        public async Task<ActionResult<EntityAC>> AddEntityAsync([FromRoute] string type, [FromBody] EntityAC entity)
        {
            if (/*ModelState.IsValid &&*/ type != null)
            {
                entity.Id = null;
                EntityAC addedEntity = await _entityRepository.AddOrUpdateEntityAsync(entity, CurrentUserAC, type);
                return StatusCode((int)HttpStatusCode.Created, addedEntity);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Update an entity.
        /// </summary>
        /// <remarks>
        /// It will update an entity and also link related details of the provided entity
        /// </remarks>
        /// <param name="id">Unique identier for the entity object</param>
        /// <param name="entity">Entity object</param>
        /// <response code="201">Returns the newly created entity object</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EntityAC), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<EntityAC>> UpdateEntityAsync([FromRoute] Guid id, [FromBody] EntityAC entity)
        {
            if (ModelState.IsValid && !id.Equals(Guid.Empty))
            {
                EntityAC addedEntity = await _entityRepository.AddOrUpdateEntityAsync(entity, CurrentUserAC, null);
                return Ok(addedEntity);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Remove an entity.
        /// </summary>
        /// <remarks>
        /// Remove an entity and/or linking.
        /// </remarks>
        /// <param name="id">Unique identier for the entity object</param>
        /// <param name="entity">Unique identier for the entity object</param>
        /// <response code="200">Delete particular linking</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RemoveLinkEntityAsync([FromRoute] Guid id, [FromBody] EntityAC entity)
        {
            if (ModelState.IsValid && !id.Equals(Guid.Empty))
            {
                entity.Id = id;
                await _entityRepository.RemoveLinkEntityAsync(entity, CurrentUserAC);
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Retrieve list of all entities.
        /// </summary>
        /// <remarks>
        /// Retrieve list of all entities.
        /// </remarks>
        /// <param name="filterModel">Filter object</param>
        /// <response code="200">Get list of entity</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If user hasn't permission.</response> 
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EntityAC>>> GetEntityListAsync([FromQuery] FilterModelAC filterModel)
        {
            if (ModelState.IsValid)
            {
                List<EntityAC> entityList = await _entityRepository.GetEntityListAsync(filterModel, CurrentUserAC);
                return Ok(entityList);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Retrieve an entity.
        /// </summary>
        /// <remarks>
        /// Get entity details by Id - basic details and self declaration form.
        /// </remarks>
        /// <param name="id">Unique identier for the entity object</param>
        /// <response code="200">Retrieve an entity</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<EntityAC>> GetEntityAsync([FromRoute] Guid id)
        {
            if (ModelState.IsValid)
            {
                EntityAC entity = await _entityRepository.GetEntityAsync(id, CurrentUserAC);
                return Ok(entity);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }


        #region Finances
        /// <summary>
        /// Retrieve Finances for a company
        /// </summary>
        /// <remarks>
        /// Get standard financial information for an entity
        /// </remarks>
        /// <param name="id">Id of Entity for which financial information is needed</param>
        /// <param name="statementCsv">Comma separated list of report names. Any of these ["Income Statement","Balance Sheet","Cash Flow","Financial Ratios"]</param>
        /// <response code="200">Retrieve the entity finances</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <response code="204">If there is no finance available for the entity</response>
        /// <returns></returns>
        [HttpGet("{id}/finances/company/{statementCsv}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CompanyFinanceAC>>> GetCompanyFinancesAsync(Guid id, string statementCsv)
        {
            return (await _entityFinanceRepository.GetFinancesAsync(id, ResourceType.Company, statementCsv, CurrentUserAC));
        }

        /// <summary>
        /// Add finances of company
        /// </summary>
        /// <remarks>
        /// Add finance information from third party service like Quickbooks, Xero
        /// </remarks>
        /// <param name="id">Id of entity for which finances are added</param>
        /// <param name="statementCsv">Comma separated list of financial reports that are to be saved. Any of these ["Income Statement","Balance Sheet","Cash Flow","Financial Ratios"]</param>
        /// <param name="redirectCallbackData">Metadata received with callback url of third party service like Quickbooks or Xero</param>
        /// <response code="201">Save entity finance</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost("{id}/finances/company/{statementCsv}")]
        public async Task<StatusCodeResult> AddCompanyFinancesAsync(Guid id, string statementCsv, [FromBody] ThirdPartyServiceCallbackDataAC redirectCallbackData)
        {
            if (redirectCallbackData != null)
                redirectCallbackData.EntityId = id;
            await _entityFinanceRepository.AddOrUpdateFinancesAsync(redirectCallbackData, statementCsv, CurrentUserAC);

            return StatusCode((int)HttpStatusCode.Created);
        }

        /// <summary>
        /// Retrieve authorization url
        /// </summary>
        /// <remarks>
        /// Get constructed temporary auth url to connect to Third party finance service like Quickbooks, Xero
        /// </remarks>
        /// <param name="id">Id of entity, this will be used to feed in state variable of auth-url</param>
        /// <param name="source">Name of third party service whose auth url is requested. Can be Quickbooks or Xero</param>
        /// <response code="200">Retrieve a constructed auth-url</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpGet("{id}/finances/{source}/auth-url")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<string> GetAuthUrlAsync(Guid id, string source)
        {

            return await _entityFinanceRepository.GetAuthorizationUrlAsync(id, source, CurrentUserAC);
        }

        /// <summary>
        /// Add finances for Jamoon's standard accounts
        /// </summary>
        /// <remarks>
        /// Map and save Finance response of third party services like Quickbooks, Xero to Standard Jamoon accounts
        /// </remarks>
        /// <param name="id">Id of entity for which standard finances are to be saved</param>
        /// <param name="siteSharedKey">secret key that will be used by Lambda while invoking this API</param>
        /// <response code="200">Map third party finances data to standard accounts</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the request is not accessible</response>
        /// <response code="401">If the secret key is null</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("{id}/standard-statements")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> MapFinancesAsync(Guid id, [FromBody] Guid siteSharedKey)
        {
            if (siteSharedKey == _configuration.GetValue<Guid>("Aws:AwsFinanceLambdaSecretKey") || _configuration.GetValue<string>("Environment") == StringConstant.Local)
            {
                await _entityFinanceRepository.MapToStandardChartOfAccountsAsync(id);
                return Ok();
            }
            return Unauthorized();
        }

        #region Personal Finances

        /// <summary>
        /// Retrieve finances of a user
        /// </summary>
        /// <remarks>
        /// Retrieve personal finances of a user
        /// </remarks>
        /// <param name="id">Id of user whose finances need to be fetched</param>
        /// <param name="scopeCsv">Comma separated list of scopes of data to be fetched. ["details","summary"]</param>
        /// <response code="200">Retrieved the personal finances</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <response code="204">If there is no required data available</response>
        /// <returns></returns>
        [HttpGet("{id}/finances/personal/{scopeCsv}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PersonalFinanceAC>> GetPersonalFinancesAsync([FromRoute] Guid id, [FromRoute] string scopeCsv)
        {
            return Ok(await _entityFinanceRepository.GetPersonalFinancesAsync(id, scopeCsv, CurrentUserAC));
        }

        /// <summary>
        /// Add finances of a user
        /// </summary>
        /// <remarks>
        /// Add personal finances of a user
        /// </remarks>
        /// <param name="id">Id of an entity whose finances need to be added</param>
        /// <param name="category">Finance category whose details need to be added</param>
        /// <response code="201">Added entity finance</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost("{id}/finances/personal")]
        public async Task<StatusCodeResult> AddPersonalFinancesAsync([FromRoute] Guid id, [FromBody] PersonalFinanceCategoryAC category)
        {
            await _entityFinanceRepository.SavePersonalFinancesAsync(id, category, CurrentUserAC);
            return StatusCode((int)HttpStatusCode.Created);
        }

        /// <summary>
        /// Update finances of a user
        /// </summary>
        /// <remarks>
        /// Update personal finances of a user
        /// </remarks>
        /// <param name="id">Id of an entity whose finances need to be updated</param>
        /// <param name="category">Finance category whose details need to be updated</param>
        /// <response code="201">Updated entity finance</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [ProducesResponseType(typeof(EntityAC), (int)HttpStatusCode.NoContent)]
        [HttpPut("{id}/finances/personal")]
        public async Task<StatusCodeResult> UpdatePersonalFinancesAsync([FromRoute] Guid id, [FromBody] PersonalFinanceCategoryAC category)
        {
            await _entityFinanceRepository.SavePersonalFinancesAsync(id, category, CurrentUserAC);
            return StatusCode((int)HttpStatusCode.NoContent);
        }
        #endregion

        #endregion

        #region Taxes

        /// <summary>
        /// Retrieve the list of uploaded tax forms for the entity
        /// </summary>
        /// <remarks>
        /// This API will retrieve the list of uploaded tax forms for the given entity
        /// </remarks>
        /// <param name="id">Entity Id</param>
        /// <response code="200">Returns the list of tax forms linked with given entity</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        [HttpGet("{id}/taxes")]
        [ProducesResponseType(typeof(EntityAC), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<EntityAC>> GetTaxListByEntityIdAsync([FromRoute] Guid id)
        {
            if (id != Guid.Empty)
            {
                return Ok(await _entityTaxReturnRepository.GetTaxReturnInfoAsync(id, ResourceType.Company, CurrentUserAC));
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Capture the tax forms related to the given entity
        /// </summary>
        /// <remarks>
        /// This API will be adding the tax forms related to the given entity
        /// </remarks>
        /// <param name="id">Id object</param>
        /// <param name="entityAC">EntityAC object</param>
        /// <returns></returns>
        /// <response code="201">Returns the newly created </response>
        /// <response code="400">If the object has invalid field or it is null</response>
        [HttpPost("{id}/taxes")]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> AddTaxFormAsync([FromRoute] Guid id, [FromBody] EntityAC entityAC)
        {
            if (ModelState.IsValid && id != Guid.Empty)
            {
                await _entityTaxReturnRepository.SaveTaxReturnInfoAsync(id, entityAC, CurrentUserAC);
                return StatusCode((int)HttpStatusCode.Created);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        #endregion

        #region Credit Report
        /// <summary>
        /// Fetch credit report.
        /// </summary>
        /// <remarks>
        /// This API will fetch credit report of entity
        /// </remarks>
        /// <param name="id">unique identifier of entity</param>
        /// <param name="application">application object</param>
        /// <response code="200">Successfully fetched and saved</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [HttpPost("{id}/credit-report")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> FetchCreditReportAsync([FromRoute] Guid id, [FromBody] ApplicationBasicDetailAC application)
        {
            if (application.Id != null)
            {
                await _entityRepository.FetchCreditReportAsync(id, application.Id.Value, CurrentUserAC);
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Get credit report.
        /// </summary>
        /// <remarks>
        /// This API will get credit report of entity
        /// </remarks>
        /// <param name="id">unique identifier of entity</param>
        /// <response code="200">Retrieve the credit report</response>
        /// <response code="400">If the request parameter is invalid or null</response>
        /// <response code="204">If credit report doesn't exist</response>
        /// <returns></returns>
        [HttpGet("{id}/credit-report")]
        [ProducesResponseType(typeof(EntityAC), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<EntityAC>> GetCreditReportAsync([FromRoute] Guid id)
        {
            EntityAC entity = await _entityRepository.GetCreditReportAsync(id, null);
            return Ok(entity);
        }
        #endregion

        #region AuditLog

        /// <summary>
        /// Retrieve the entity audit logs.
        /// </summary>
        /// <remarks>
        /// This method will retrieve the entity audit logs.
        /// </remarks>
        /// <param name="id">Entity Id.</param>
        /// <param name="auditLogFilter">Audit log filter object.</param>
        /// <response code="200">Returns the list of Audit logs</response>
        /// <response code="400">If the request parameter is invalid</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        [HttpPost("{id}/activity-logs")]
        [ProducesResponseType(typeof(List<AuditDateWiseLogsAC>), StatusCodes.Status200OK)]
        [Authorize(Roles = StringConstant.BankUserRole)]
        public async Task<ActionResult<List<AuditDateWiseLogsAC>>> GetAuditLogsAsync([FromRoute] Guid id, [FromBody] AuditLogFilterAC auditLogFilter)
        {
            auditLogFilter.LogBlockNameId = id;
            var result = await _globalRepository.GetAuditLogsByLogBlockNameIdAsync(CurrentUserAC, auditLogFilter);
            return Ok(result);
        }
        #endregion

        #region Additional documents

        /// <summary>
        /// Retrieve the list of additional documents of an entity
        /// </summary>
        /// <remarks>
        /// This API will retrieve the additional documents uploaded for the given entity
        /// </remarks>
        /// <param name="id">Id of an entity whose additional documents need to be fetched</param>
        /// <response code="200">Retrieved the additional documents</response>
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="204">If there is no required data available</response>
        /// <returns></returns>
        [HttpGet("{id}/additional-documents")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<EntityAC>> GetAdditionalDocumentsForEntityAsync([FromRoute] Guid id)
        {
            return Ok(await _entityRepository.GetAdditionalDocumentsByResourceIdAsync(id, ResourceType.Company, CurrentUserAC));
        }

        /// <summary>
        /// Add additional documents of an entity
        /// </summary>
        /// <remarks>
        /// Add all additional documents of given entity
        /// </remarks>
        /// <param name="id">Id of an entity whose additional documents need to be added</param>
        /// <param name="entityAC">Object of an entity, containing additional documents to add</param>
        /// <response code="201">Added entity finance</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <response code="403">If the user haven't permission to access the request</response>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost("{id}/additional-documents")]
        public async Task<StatusCodeResult> SaveAdditionalDocumentsForEntityAsync([FromRoute] Guid id, [FromBody] EntityAC entityAC)
        {
            await _entityRepository.SaveAdditionalDocumentOfEntityAsync(id, entityAC, CurrentUserAC);
            return StatusCode((int)HttpStatusCode.Created);
        }

        #endregion

        #endregion
    }
}
