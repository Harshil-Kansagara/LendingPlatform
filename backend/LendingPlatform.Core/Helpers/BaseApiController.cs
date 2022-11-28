using AutoMapper;
using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.ApplicationClass;
using LendingPlatform.Utils.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LendingPlatform.Core.Helpers
{
    public abstract class BaseApiController : ControllerBase
    {
        #region "Private variable(s)"

        private readonly IDataRepository _dataRepository;
        private User _currentUser;
        private CurrentUserAC _currentUserAC;
        private readonly IMapper _mapper;

        #endregion

        #region "Public method(s)"

        protected BaseApiController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }
        protected BaseApiController(IDataRepository dataRepository, IMapper mapper)
        {
            _dataRepository = dataRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Method to get current user
        /// </summary>
        /// <returns>CurrentUser</returns>
        protected User CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    string email = HttpContext.User.Claims.First(x => x.Type == "name").Value;
                    _currentUser = _dataRepository.FirstOrDefaultAsync<User>(x => x.Email == email.ToLowerInvariant()).GetAwaiter().GetResult();

                }
                return _currentUser;
            }
        }
        /// <summary>
        /// Fetch the user object by the User role.
        /// </summary>
        protected CurrentUserAC CurrentUserAC
        {
            get
            {
                if (_currentUserAC == null)
                {
                    string userRole = HttpContext.User.Claims.First(x => x.Type == "UserRole").Value;
                    string email = HttpContext.User.Claims.First(x => x.Type == "preferred_username").Value;
                    if (userRole.Equals(StringConstant.BankUserRole))
                    {
                        var user = _dataRepository.FirstOrDefaultAsync<BankUser>(x => x.Email == email.ToLowerInvariant()).GetAwaiter().GetResult();
                        _currentUserAC = _mapper.Map<CurrentUserAC>(user);
                        _currentUserAC.IsBankUser = true;
                    }
                    else
                    {
                        var user = _dataRepository.FirstOrDefaultAsync<User>(x => x.Email == email.ToLowerInvariant()).GetAwaiter().GetResult();
                        _currentUserAC = _mapper.Map<CurrentUserAC>(user);
                    }
                    _currentUserAC.IpAddress = GetClientIpAddress();
                }
                return _currentUserAC;
            }
        }
        /// <summary>
        /// Fetch the User's local IP Address.
        /// </summary>
        /// <returns></returns>
        private string GetClientIpAddress()
        {
            string remoteIpAddress;
            // if proxy is there then x-forwarded-for header will be there so for orignal client ip we need to extract orignal client ip from below logic.
            var headers = Request.HttpContext.Request.Headers;
            if (headers.ContainsKey(StringConstant.IpAddressHeader))
            {
                remoteIpAddress = headers.GetCommaSeparatedValues(StringConstant.IpAddressHeader).First();
            }
            else
            {
                remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            return remoteIpAddress;
        }
        #endregion
    }
}
