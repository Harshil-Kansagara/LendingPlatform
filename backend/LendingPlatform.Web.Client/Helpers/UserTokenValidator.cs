using LendingPlatform.DomainModel.Enums;
using LendingPlatform.DomainModel.Models;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.Repository.CustomException;
using LendingPlatform.Repository.Repository.GlobalHelpers;
using LendingPlatform.Utils.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LendingPlatform.Web.Client.Helpers
{
    public class UserTokenValidator : JwtBearerEvents
    {

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            // Get resolved service instance
            LendingPlatformContext dbContext = context.HttpContext.RequestServices.GetRequiredService<LendingPlatformContext>();

            List<Claim> userClaims = context.Principal.Claims.ToList();
            var email = userClaims.FirstOrDefault(y => y.Type == "preferred_username").Value;
            var role = userClaims.FirstOrDefault(y => y.Type == "UserRole").Value;
            if (email == null)
            {
                throw new CliamNotFoundException("Missing claims");
            }

            // if user arrived for the first time
            if (role.Contains(StringConstant.ClientRole))
            {
                User checkUser = dbContext.Set<User>().FirstOrDefault(x => x.Email == email.ToLowerInvariant());
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    if (checkUser == null)
                    {
                        var entity = new Entity
                        {
                            Type = EntityType.User
                        };
                        await dbContext.AddAsync(entity);
                        await dbContext.SaveChangesAsync();
                        var firstName = userClaims.FirstOrDefault(x => x.Type.Contains("givenname", StringComparison.InvariantCultureIgnoreCase))?.Value;
                        var lastName = userClaims.FirstOrDefault(x => x.Type.Contains("surname", StringComparison.InvariantCultureIgnoreCase))?.Value;

                        checkUser = new User
                        {
                            Id = entity.Id,
                            FirstName = firstName?.Trim(),
                            LastName = lastName?.Trim(),
                            Email = email.Trim(),
                            Phone = userClaims.FirstOrDefault(x => x.Type.Contains(StringConstant.PhoneNumber, StringComparison.InvariantCultureIgnoreCase))?.Value,
                            CreatedByUserId = entity.Id,
                            CreatedOn = DateTime.UtcNow,
                            IsRegistered = true
                        };

                        checkUser = SetUserFromUserClaims(checkUser, userClaims);

                        await dbContext.AddAsync(checkUser);
                    }//Update User if already exists and came from keycloak for the first time
                    else if (!checkUser.IsRegistered)
                    {
                        checkUser.FirstName = userClaims.FirstOrDefault(x => x.Type.Contains("givenname", StringComparison.InvariantCultureIgnoreCase))?.Value;
                        checkUser.LastName = userClaims.FirstOrDefault(x => x.Type.Contains("surname", StringComparison.InvariantCultureIgnoreCase))?.Value;
                        checkUser.Phone = userClaims.FirstOrDefault(x => x.Type.Contains(StringConstant.PhoneNumber, StringComparison.InvariantCultureIgnoreCase))?.Value;
                        checkUser.UpdatedByUserId = checkUser.Id;
                        checkUser.IsRegistered = true;
                        checkUser = SetUserFromUserClaims(checkUser, userClaims);

                        dbContext.Update(checkUser);
                    }

                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

            }
            else if (role.Contains(StringConstant.BankUserRole))
            {
                BankUser checkUser = dbContext.Set<BankUser>().FirstOrDefault(x => x.Email == email);
                if (checkUser == null)
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {

                        checkUser = new BankUser
                        {
                            Name = userClaims.FirstOrDefault(x => x.Type.Equals("name", StringComparison.InvariantCultureIgnoreCase))?.Value?.Trim(),
                            Email = email.Trim(),
                            Phone = userClaims.FirstOrDefault(x => x.Type.Contains(StringConstant.PhoneNumber, StringComparison.InvariantCultureIgnoreCase))?.Value?.Trim(),
                        };

                        await dbContext.AddAsync(checkUser);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }

                }
            }

        }

        /// <summary>
        /// Method to set user(Residency Status & DOB) from user claim
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userClaims"></param>
        /// <returns></returns>
        public User SetUserFromUserClaims(User user, List<Claim> userClaims)
        {
            var residencyStatus = userClaims.FirstOrDefault(x => x.Type.Contains("ResidencyStatus", StringComparison.InvariantCultureIgnoreCase))?.Value;
            if (residencyStatus != null)
            {
                user.ResidencyStatus = Enum.GetValues(typeof(ResidencyStatus))
                .Cast<ResidencyStatus>()
                .FirstOrDefault(v => GlobalRepository.GetEnumDescription(v).Equals(residencyStatus, StringComparison.InvariantCultureIgnoreCase));

            }

            // If claims for DOB of user exists then only add value in DOB field of User object.
            var dobClaim = userClaims.FirstOrDefault(x => x.Type == "Birthdate");
            if (dobClaim != null)
            {
                DateTime birthDate;
                if (DateTime.TryParse(dobClaim.Value, out birthDate))
                {
                    user.DOB = birthDate;
                }

            }
            return user;
        }

    }
}

