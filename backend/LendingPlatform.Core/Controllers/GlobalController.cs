using Amazon.S3;
using AutoMapper;
using LendingPlatform.Core.Helpers;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.ApplicationClass.AppSettings;
using LendingPlatform.Repository.ApplicationClass.Others;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LendingPlatform.Core.Controllers
{
    [OpenApiTag("General", Description = "General APIs to retrieve the common data like entity's age range, size range, etc.. or application's purpose, type, etc.. or configurations for any third-party services.")]
    [Produces(StringConstant.HttpHeaderAcceptJsonType)]
    [Route(BaseUrl)]
    [ApiController]
    [Authorize(Roles = StringConstant.UserAllRoles)]
    public class GlobalController : BaseApiController
    {
        #region Private variables
        private const string BaseUrl = "api/common";
        private readonly IGlobalRepository _globalRepository;
        #endregion

        #region Constructor
        public GlobalController(IGlobalRepository globalRepository, IDataRepository dataRepository, IMapper mapper) : base(dataRepository, mapper)
        {
            _globalRepository = globalRepository;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Fetch the list of consent statements.
        /// </summary>
        /// <remarks>
        /// This method will fetch all the consent statements to show to the user to get consent.
        /// </remarks>
        /// <response code="200">Returns the list of ConsentStatementAC objects</response>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("consents")]
        [ProducesResponseType(typeof(List<ConsentStatementAC>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ConsentStatementAC>>> GetConsentStatementsAsync()
        {
            return Ok(await _globalRepository.GetConsentStatementsAsync());
        }

        /// <summary>
        /// Fetch the list of banks.
        /// </summary>
        /// <remarks>
        /// This method will fetch all the banks with name and SWIFT code.
        /// </remarks>
        /// <response code="200">Returns the list of BankAC objects</response>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("banks")]
        [ProducesResponseType(typeof(List<BankAC>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<BankAC>>> GetListOfBanksAsync()
        {
            return Ok(await _globalRepository.GetAllBanksAsync());
        }

        /// <summary>
        /// Fetch the list of loan purposes.
        /// </summary>
        /// <remarks>
        /// This method will fetch all available purposes for the loan application.
        /// </remarks>
        /// <response code="200">Returns the list of LoanPurposeAC objects</response>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("loan-purposes")]
        [ProducesResponseType(typeof(List<LoanPurposeAC>), (int)HttpStatusCode.OK)]
        [AllowAnonymous]
        public async Task<ActionResult<List<LoanPurposeAC>>> GetLoanPurposeListAsync()
        {
            return Ok(await _globalRepository.GetLoanPurposeListAsync());
        }

        /// <summary>
        /// Fetch all the configurations required for UI.
        /// </summary>
        /// <remarks>
        /// This method will fetch all the configurations required in UI like enabled sections, third party services, and other UI specific settings.
        /// </remarks>
        /// <response code="200">Returns the list of BankAC objects</response>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("configurations")]
        [ProducesResponseType(typeof(ConfigurationAC), (int)HttpStatusCode.OK)]
        [AllowAnonymous]
        public async Task<ActionResult<ConfigurationAC>> GetConfigurationsAsync()
        {
            return Ok(await _globalRepository.GetAllConfigurationsAsync());
        }

        /// <summary>
        /// Retrieve list of Company age ranges.
        /// </summary>
        /// <remarks>
        /// Retrieve list of Company age ranges
        /// </remarks>
        /// <response code="200">Returns the list of company age ranges</response>
        /// <returns></returns>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("company/age-ranges")]
        [ProducesResponseType(typeof(List<BusinessAgeAC>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<BusinessAgeAC>>> GetBusinessAgeRangeListAsync()
        {
            List<BusinessAgeAC> companyAges = await _globalRepository.GetBusinessAgeRangeListAsync();
            return Ok(companyAges);
        }

        /// <summary>
        /// Retrieve list of Company types.
        /// </summary>
        /// <remarks>
        /// Retrieve list Company Types
        /// </remarks>
        /// <response code="200">Returns the list of company structure</response>
        /// <returns></returns>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("company/types")]
        [ProducesResponseType(typeof(List<CompanyStructureAC>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CompanyStructureAC>>> GetCompanyStructureListAsync()
        {
            List<CompanyStructureAC> companyStructures = await _globalRepository.GetCompanyStructureListAsync();
            return Ok(companyStructures);
        }

        /// <summary>
        /// Retrieve list of Company size ranges.
        /// </summary>
        /// <remarks>
        /// Retrieve list of Company size ranges.
        /// </remarks>
        /// <response code="200">Returns the list of company size ranges</response>
        /// <returns></returns>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("company/size-ranges")]
        [ProducesResponseType(typeof(List<CompanySizeAC>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<CompanySizeAC>>> GetCompanySizeListAsync()
        {
            List<CompanySizeAC> companySizeList = await _globalRepository.GetCompanySizeListAsync();
            return Ok(companySizeList);
        }

        /// <summary>
        /// Retrieve list of Industry group.
        /// </summary>
        /// <remarks>
        /// Retrieve list of Industry group.
        /// </remarks>
        /// <response code="200">Returns the list of industry group</response>
        /// <returns></returns>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("industry-group")]
        [ProducesResponseType(typeof(List<NAICSIndustryType>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<NAICSIndustryTypeAC>>> GetIndustryGroupListAsync()
        {
            List<NAICSIndustryTypeAC> industryGroupList = await _globalRepository.GetIndustryGroupListAsync();
            return Ok(industryGroupList);
        }

        /// <summary>
        /// Retrieve list of Industry Experiences.
        /// </summary>
        /// <remarks>
        /// Retrieve list of Industry Experiences.
        /// </remarks>
        /// <response code="200">Returns the list of Industry Experiences</response>
        /// <returns></returns>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("company/industry-experience")]
        [ProducesResponseType(typeof(List<IndustryExperienceAC>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<IndustryExperienceAC>>> GetIndustryExperienceListAsync()
        {
            List<IndustryExperienceAC> industryExperienceList = await _globalRepository.GetIndustryExperienceListAsync();
            return Ok(industryExperienceList);
        }

        #region Additional documents

        /// <summary>
        /// Fetch the list of additional document types.
        /// </summary>
        /// <remarks>
        /// This method will fetch all available additional documents types.
        /// </remarks>
        /// <response code="200">Returns the list of AdditionalDocumentTypeAC objects</response>
        /// <response code="204">If there is no finance available for the entity</response>
        [Consumes(StringConstant.HttpHeaderAcceptJsonType)]
        [HttpGet("additional-document-types")]
        [ProducesResponseType(typeof(List<AdditionalDocumentTypeAC>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<AdditionalDocumentTypeAC>>> GetAdditionalDocumentTypesAsync()
        {
            return Ok(await _globalRepository.GetAdditionalDocumentTypesAsync());
        }

        #endregion

        #region Files

        /// <summary>
        /// Get document pre-signed url for the download purpose
        /// </summary>
        /// <remarks>
        /// This method will used to download the uploaded file from the S3 object
        /// </remarks>
        /// <param name="id">Document Id</param>
        /// <response code="201">Returns the newly created Pre-Signed URL string of the document</response>
        /// <response code="400">If the request parameter is invalid or null</response> 
        /// <returns>Generated Pre-Signed URL link</returns>
        [HttpGet("download-file/{id}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GetDocumentAsync([FromRoute] Guid id)
        {
            if (id != Guid.Empty)
            {
                try
                {
                    string url = await _globalRepository.GetPreSignedUrlAsync(id);
                    return url;
                }
                catch (AmazonS3Exception)
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Get upload presigned URL
        /// </summary>
        /// <returns></returns>
        /// <param name="fileName">name of the file to be uploaded.</param>
        /// <response code="200">Returns upload presigned URL.</response>
        [HttpGet("upload-presignedURL/{fileName}")]
        [ProducesResponseType(typeof(AwsSettings), (int)HttpStatusCode.OK)]
        public ActionResult<AwsSettings> GetUploadPreSignedURL([FromRoute] string fileName)
        {
            return _globalRepository.GetUploadPreSignedURL(fileName);
        }

        #endregion

        #region AuditLog

        /// <summary>
        /// Retrieve the list of fields of the log by field primary key of the old value and old value.
        /// </summary>
        /// <remarks>
        /// This method will retrieve the list of fields of the primary key which is in the old and new value.
        /// </remarks>
        /// <param name="auditLogField">Field object.</param>
        /// <response code="200">Returns the list of fields of the log.</response>
        /// <response code="400">If the request parameter is invalid.</response>
        /// <response code="204">Activity log is not exist in the system.</response>
        [HttpPost("activity-log")]
        [ProducesResponseType(typeof(List<AuditLogFieldAC>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize(Roles = StringConstant.BankUserRole)]
        public async Task<ActionResult<List<AuditLogFieldAC>>> GetAuditLogByPkIdAsync([FromBody] AuditLogFieldAC auditLogField)
        {
            var auditLogFields = await _globalRepository.GetAuditLogByPkIdAsync(auditLogField);
            return Ok(auditLogFields);
        }
        #endregion
        #endregion

    }

}
