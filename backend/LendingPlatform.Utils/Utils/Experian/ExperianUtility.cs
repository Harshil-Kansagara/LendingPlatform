using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class ExperianUtility : IExperianUtility
    {
        #region Constructor
        public ExperianUtility()
        {
        }
        #endregion

        #region Private Method(s)

        /// <summary>
        /// Get Experian Access token
        /// </summary>
        /// <param name="configurationList">List of Experian configuration</param>
        /// <returns>Return access token</returns>
        private async Task<string> GetExperianAccessToken(List<ThirdPartyConfigurationAC> configurationList)
        {
            TokenRequestAC tokenRequest = new TokenRequestAC()
            {
                Username = configurationList.Single(x => x.Path == "Experian:Username").Value,
                Password = configurationList.Single(x => x.Path == "Experian:Password").Value,
                ClientId = configurationList.Single(x => x.Path == "Experian:ClientId").Value,
                ClientSecret = configurationList.Single(x => x.Path == "Experian:ClientSecret").Value,
            };

            DefaultContractResolver contractResolverSnakeCase = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };



            var tokenJson = JsonConvert.SerializeObject(tokenRequest, new JsonSerializerSettings
            {
                ContractResolver = contractResolverSnakeCase,
                Formatting = Formatting.Indented
            });

            TokenResponseAC tokenResponse;

            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), configurationList.Single(x => x.Path == "Experian:OauthTokenAPI").Value);
                request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAccept, StringConstant.HttpHeaderAcceptJsonType);
                request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderGrantType, StringConstant.HttpHeaderPassword);
                request.Content = new StringContent(tokenJson);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderAcceptJsonType);
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var tokenResponseJson = await response.Content.ReadAsStringAsync();

                tokenResponse = JsonConvert.DeserializeObject<TokenResponseAC>(tokenResponseJson, new JsonSerializerSettings
                {
                    ContractResolver = contractResolverSnakeCase,
                    Formatting = Formatting.Indented
                });
            }
            return tokenResponse.AccessToken;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fetch credit report of company from experian api
        /// </summary>
        /// <param name="experianConfigurationJson">Experian ConfigurationJson</param>
        /// <param name="bin"></param>
        /// <returns></returns>
        public async Task<PremierProfilesResponseAC> FetchCompanyCreditScoreExperianAsync(string bin, string experianConfigurationJson)
        {
            var configurationList = JsonConvert.DeserializeObject<List<ThirdPartyConfigurationAC>>(experianConfigurationJson);

            string accessToken = await GetExperianAccessToken(configurationList);

            DefaultContractResolver contractResolverCamelCase = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            PremierProfilesRequestAC premierProfilesRequest = new PremierProfilesRequestAC()
            {
                Bin = bin,
                ModelCode = configurationList.Single(x => x.Path == "Experian:ModelCode").Value,
                Subcode = configurationList.Single(x => x.Path == "Experian:Subcode").Value
            };


            var premierProfilesJson = JsonConvert.SerializeObject(premierProfilesRequest, new JsonSerializerSettings
            {
                ContractResolver = contractResolverCamelCase,
                Formatting = Formatting.Indented
            });

            PremierProfilesResponseAC premierProfilesResponse;

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), configurationList.Single(x => x.Path == "Experian:PremierProfileAPI").Value);
                request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationToken, accessToken));
                request.Content = new StringContent(premierProfilesJson);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderAcceptJsonType);

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    premierProfilesResponse = await response.Content.ReadAsAsync<PremierProfilesResponseAC>();
                    premierProfilesResponse.JsonResponse = await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode.Equals(System.Net.HttpStatusCode.InternalServerError))
                {
                    premierProfilesResponse = new PremierProfilesResponseAC();
                }
                else
                {
                    throw new HttpRequestException(StringConstant.UnexpectedStatusCodePremierProfilesExperian);
                }
            }

            return premierProfilesResponse;
        }

        /// <summary>
        /// Fetch credit report of user from experian api
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="experianConfigurationJson">Experian ConfigurationJson</param>
        /// <returns></returns>
        public async Task<UserInfoAC> FetchUserCreditScoreExperianAsync(UserInfoAC user, string experianConfigurationJson)
        {
            var configurationList = JsonConvert.DeserializeObject<List<ThirdPartyConfigurationAC>>(experianConfigurationJson);

            string accessToken = await GetExperianAccessToken(configurationList);

            DefaultContractResolver contractResolverCamelCase = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            AutomotiveCreditProfileRequestAC automotiveCreditProfileRequest = new AutomotiveCreditProfileRequestAC
            {
                ConsumerPii = new ConsumerPiiAC
                {
                    PrimaryApplicant = new PrimaryApplicantAC
                    {
                        Name = new NameAC
                        {
                            LastName = user.LastName,
                            FirstName = user.FirstName,
                            MiddleName = user.MiddleName
                        },
                        SSN = new SSNAC
                        {
                            SSN = user.SSN
                        },
                        CurrentAddress = new CurrentAddressAC
                        {
                            Line1 = user.Address.StreetLine,
                            City = user.Address.City,
                            State = user.Address.StateAbbreviation,
                            ZipCode = user.Address.ZipCode
                        }
                    }
                },
                Requestor = new RequestorAC
                {
                    SubscriberCode = configurationList.Single(x => x.Path == "Experian:SubscriberCode").Value
                },
                AddOns = new AddOnsAC
                {
                    Demographics = StringConstant.Demographics,
                    RiskModels = new RiskModelsAC
                    {
                        ModelIndicator = new List<string>(new string[]{
                                StringConstant.ModelIndicatorAE,
                                StringConstant.ModelIndicatorF3,
                                StringConstant.ModelIndicatorFM,
                                StringConstant.ModelIndicatorW,
                                StringConstant.ModelIndicatorQ
                            })
                    },
                    FraudShield = StringConstant.FraudShield
                }
            };


            var automotiveCreditProfileJson = JsonConvert.SerializeObject(automotiveCreditProfileRequest, new JsonSerializerSettings
            {
                ContractResolver = contractResolverCamelCase,
                Formatting = Formatting.Indented
            });

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), configurationList.Single(x => x.Path == "Experian:AutomotiveCreditProfileAPI").Value);

                request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationToken, accessToken));
                request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderClientReferenceId, StringConstant.HttpHeaderSBMYSQL);
                request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAccept, StringConstant.HttpHeaderAcceptJsonType);

                request.Content = new StringContent(automotiveCreditProfileJson);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderAcceptJsonType);

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    user.ConsumerCreditReportResponse = await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode.Equals(System.Net.HttpStatusCode.BadRequest))
                {
                    user.ConsumerCreditReportResponse = null;
                }
                else
                {
                    throw new HttpRequestException(StringConstant.UnexpectedStatusCodePremierProfilesExperian);
                }
            }

            return user;
        }

        #endregion
    }
}
