using LendingPlatform.DomainModel.DataRepository;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.ApplicationClass.EntityInfo;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.EntityInfo;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Utils;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using SmartyStreets.USStreetApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace LendingPlatform.Repository.Test.EntityInfo
{
    [Collection("Register Dependency")]
    public class UserInfoRepositoryTest : BaseTest
    {
        #region Private variables
        private readonly Mock<IDataRepository> _dataRepositoryMock;
        private readonly Mock<IGlobalRepository> _globalRepositoryMock;
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly Mock<ISmartyStreetsUtility> _smartyStreetsUtilityMock;
        #endregion

        #region Constructor
        public UserInfoRepositoryTest(Bootstrap bootstrap) : base(bootstrap)
        {
            _dataRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IDataRepository>>();
            _globalRepositoryMock = bootstrap.ServiceProvider.GetService<Mock<IGlobalRepository>>();
            _userInfoRepository = bootstrap.ServiceProvider.GetService<IUserInfoRepository>();
            _smartyStreetsUtilityMock = bootstrap.ServiceProvider.GetService<Mock<ISmartyStreetsUtility>>();
            _dataRepositoryMock.Reset();
            _globalRepositoryMock.Reset();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get current logged in user details.
        /// </summary>
        /// <returns></returns>
        private User GetCurrentUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Email = "john@doe.com",
                Phone = "9898989898",
                SSN = "123456789"
            };
        }

        /// <summary>
        /// Get UserInfoAddressAC object.
        /// </summary>
        /// <returns></returns>
        private UserInfoAddressAC GetUserInfoAddressACObject()
        {
            return new UserInfoAddressAC
            {
                User = new UserAC()
                {
                    Id = Guid.NewGuid(),
                    Name = "Abhi Kansagra",
                    SSN = "920891236",
                    DOB = DateTime.UtcNow,
                    Email = "abhi@lendingplatform.com",
                    Phone = "+919999999988"
                },
                Address = GetAddressACObject()
            };
        }

        /// <summary>
        /// Get AddressAC object.
        /// </summary>
        /// <returns></returns>
        private AddressAC GetAddressACObject()
        {
            return new AddressAC
            {
                StreetLine = "86 Frontage Rd",
                City = "Belmont",
                StateAbbreviation = "MA"
            };
        }

        /// <summary>
        /// Get Address object.
        /// </summary>
        /// <returns></returns>
        private Address GetAddressObject()
        {
            return new Address
            {
                Id = Guid.NewGuid(),
                PrimaryNumber = "86",
                StreetLine = "Frontage Rd",
                City = "Belmont",
                StateAbbreviation = "MA",
                ZipCode = "361111"
            };
        }

        /// <summary>
        /// Get validated address from smarty streets.
        /// </summary>
        /// <returns></returns>
        private SmartyStreets.USStreetApi.Candidate GetValidatedAdress()
        {
            return new SmartyStreets.USStreetApi.Candidate()
            {
                Components = new SmartyStreets.USStreetApi.Components
                {
                    PrimaryNumber = "86",
                    StreetName = "Frontage Rd",
                    CityName = "Belmont",
                    State = "MA",
                    ZipCode = "22001"
                }
            };
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Method tests if user details are not being updated by the same user then throws exception.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_UpdateDifferentUserInfo_ThrowsError()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();

            //Act

            //Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser));
        }

        /// <summary>
        /// Method tests if user doen't exist whose details are being updated then throws exception.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_UpdateInfoOfNonExistingUser_ThrowsError()
        {
            //Arrange
            List<User> existingUser = new List<User>();
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(existingUser.AsQueryable().BuildMock().Object);

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser));
        }

        /// <summary>
        /// Method tests if user's SSN is not valid then throws validation exception.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_InvalidUserSSN_ThrowsValidationException()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            userInfo.User.SSN = "1234567890";
            List<User> existingUser = new List<User> { GetCurrentUser() };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(existingUser.AsQueryable().BuildMock().Object);
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(false);

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser));
        }

        /// <summary>
        /// Method tests if user's SSN is not unique then throws validation exception.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_NotUniqueUserSSN_ThrowsValidationException()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = existingUser.First();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser));
        }

        /// <summary>
        /// Method tests if user's phone number is not unique then throws validation exception.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_NotUniquePhoneNumber_ThrowsValidationException()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = null;
            User userWithSamePhone = existingUser.First();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                   .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN))
                    .Returns(Task.FromResult(userWithSamePhone));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser));
        }

        /// <summary>
        /// Method tests if user's email is is not unique then throws validation exception.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_NotUniqueEmailId_ThrowsValidationException()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = null;
            User userWithSamePhone = null;
            User userWithSameEmail = existingUser.First();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                   .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN))
                    .Returns(Task.FromResult(userWithSamePhone))
                    .Returns(Task.FromResult(userWithSameEmail));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser));
        }

        /// <summary>
        /// Method tests if address id doesn't exist in given data then fetch valid address to add it.
        /// If not valid address then throws validation exception.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_AddressIdNotExistAddAddress_ThrowsExceptionNotValidAddress()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = null;
            User userWithSamePhone = null;
            User userWithSameEmail = null;
            userInfo.Address.Id = Guid.Empty;
            Candidate validAddress = null;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                   .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN))
                    .Returns(Task.FromResult(userWithSamePhone))
                    .Returns(Task.FromResult(userWithSameEmail));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);
            _smartyStreetsUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                    .Returns(validAddress);

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser));
        }

        /// <summary>
        /// Method tests if address id exists in given data then fetch existing address to update it.
        /// If existing address is null then throws exception.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_AddressIdExistFetchExistingAddress_ThrowsExceptionAddressNotExistInDatabase()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            userInfo.Address.Id = Guid.NewGuid();
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = null;
            User userWithSamePhone = null;
            User userWithSameEmail = null;
            Address existingAddress = null;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                   .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN))
                    .Returns(Task.FromResult(userWithSamePhone))
                    .Returns(Task.FromResult(userWithSameEmail));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                    .Returns(Task.FromResult(existingAddress));

            //Act

            //Assert
            await Assert.ThrowsAsync<DataNotFoundException>(async () => await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser));
        }

        /// <summary>
        /// Method tests if address id exists in given data then fetch valid address to update it.
        /// If existing address is not null and given address is not same as exsisting address and 
        /// is not valid then throws validation exception.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_AddressIdExistsUpdateAddress_ThrowsExceptionNotValidAddress()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            userInfo.Address.Id = Guid.NewGuid();
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = null;
            User userWithSamePhone = null;
            User userWithSameEmail = null;
            Address existingAddress = GetAddressObject();
            existingAddress.StateAbbreviation = "NY";
            Candidate validAddress = null;

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                   .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN))
                    .Returns(Task.FromResult(userWithSamePhone))
                    .Returns(Task.FromResult(userWithSameEmail));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                    .Returns(Task.FromResult(existingAddress));
            _smartyStreetsUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                    .Returns(validAddress);

            //Act

            //Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser));
        }

        /// <summary>
        /// Method tests if address is new then add the address and update address id in entity table.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_AddressIdNotExistNewAddress_AddAddressUpdateEntityTable()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = null;
            User userWithSamePhone = null;
            User userWithSameEmail = null;
            Candidate validAddress = GetValidatedAdress();
            existingUser.First().Entity = new Entity
            {
                Id = Guid.NewGuid()
            };

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                   .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN))
                    .Returns(Task.FromResult(userWithSamePhone))
                    .Returns(Task.FromResult(userWithSameEmail));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);
            _smartyStreetsUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                    .Returns(validAddress);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act
            await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Entity>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method tests if address is changed and address id exists in the given data then udpate the address.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_AddressIdExistChangedAddress_UpdateAddress()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            userInfo.Address.Id = Guid.NewGuid();
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = null;
            User userWithSamePhone = null;
            User userWithSameEmail = null;
            Address existingAddress = GetAddressObject();
            existingAddress.StateAbbreviation = "NY";
            Candidate validAddress = GetValidatedAdress();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                   .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN))
                    .Returns(Task.FromResult(userWithSamePhone))
                    .Returns(Task.FromResult(userWithSameEmail));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                    .Returns(Task.FromResult(existingAddress));
            _smartyStreetsUtilityMock.Setup(x => x.GetValidatedAddress(It.IsAny<AddressAC>()))
                    .Returns(validAddress);
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act
            await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method tests if address is as same as existing address then do not udpate or add the address.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_AddressUnchanged_NoAddOrUpdateAddress()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            userInfo.Address.Id = Guid.NewGuid();
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = null;
            User userWithSamePhone = null;
            User userWithSameEmail = null;
            Address existingAddress = GetAddressObject();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                   .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN))
                    .Returns(Task.FromResult(userWithSamePhone))
                    .Returns(Task.FromResult(userWithSameEmail));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                    .Returns(Task.FromResult(existingAddress));
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act
            await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Address>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<Entity>()), Times.Never);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        /// <summary>
        /// Method tests if all the user details are valid then udpate the user and returns updated object.
        /// </summary>
        [Fact]
        public async Task SaveUserInfoAsync_ValidUserDetails_UpdateUser()
        {
            //Arrange
            User currentUser = GetCurrentUser();
            UserInfoAddressAC userInfo = GetUserInfoAddressACObject();
            userInfo.User.Id = currentUser.Id;
            userInfo.Address.Id = Guid.NewGuid();
            List<User> existingUser = new List<User> { GetCurrentUser() };
            User userWithSameSSN = null;
            User userWithSamePhone = null;
            User userWithSameEmail = null;
            Address existingAddress = GetAddressObject();

            _dataRepositoryMock.Setup(x => x.Fetch(It.IsAny<Expression<Func<User, bool>>>()))
                   .Returns(existingUser.AsQueryable().BuildMock().Object);
            _dataRepositoryMock.SetupSequence(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                    .Returns(Task.FromResult(userWithSameSSN))
                    .Returns(Task.FromResult(userWithSamePhone))
                    .Returns(Task.FromResult(userWithSameEmail));
            _globalRepositoryMock.Setup(x => x.IsValidCIN(It.IsAny<string>()))
                    .Returns(true);
            _dataRepositoryMock.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>()))
                    .Returns(Task.FromResult(existingAddress));
            _dataRepositoryMock.Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(It.IsAny<IDbContextTransaction>()));

            //Act
            await _userInfoRepository.SaveUserInfoAsync(userInfo, currentUser);

            //Assert
            _dataRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
            _dataRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _dataRepositoryMock.Verify(x => x.CommitTransaction(), Times.Once);
        }
        #endregion
    }
}
