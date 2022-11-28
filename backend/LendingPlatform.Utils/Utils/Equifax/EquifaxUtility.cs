using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.Equifax;
using LendingPlatform.Utils.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class EquifaxUtility : IEquifaxUtility
    {
        #region Constructor
        public EquifaxUtility()
        {
        }
        #endregion

        #region Public Method(s)

        /// <summary>
        /// Method to fetch credit report of people from equifax consumer credit report API.
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="equifaxConfigurationJson">Equifax ConfigurationJson</param>
        /// <returns></returns>
        public async Task<UserInfoAC> FetchUserCreditScoreEquifaxAsync(UserInfoAC userInfo, string equifaxConfigurationJson)
        {
            var configurationList = JsonConvert.DeserializeObject<List<ThirdPartyConfigurationAC>>(equifaxConfigurationJson);

            //Get access token from Token API of equifax
            //Setup http request with header values with credentials from appsettings(Username and Password) and call Token API to get token.
            EquifaxTokenResponseAC tokenResponse;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), configurationList.Single(x => x.Path == "Equifax:TokenAPI").Value))
                {
                    var base64Authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(StringConstant.Base64Authorization, configurationList.Single(x => x.Path == "Equifax:ClientId").Value, configurationList.Single(x => x.Path == "Equifax:ClientSecret").Value)));
                    request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationBasicAuth, base64Authorization));
                    var contentList = new List<string>
                    {
                        $"grant_type={Uri.EscapeDataString(StringConstant.HttpHeaderClientCredentials)}",
                        $"scope={Uri.EscapeDataString(configurationList.Single(x => x.Path == "Equifax:ScopeConsumerCreditReportAPI").Value)}"
                    };
                    request.Content = new StringContent(string.Join("&", contentList));
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderAcceptJsonType);

                    var response = await httpClient.SendAsync(request);

                    //Check ensure success status code for exception if success code is not returned.
                    response.EnsureSuccessStatusCode();

                    var tokenResponseJson = await response.Content.ReadAsStringAsync();

                    DefaultContractResolver contractResolverSnakeCase = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    };

                    //Deserialize the reponse of token api json into EqifaxTokenResponseAC object with snake case
                    tokenResponse = JsonConvert.DeserializeObject<EquifaxTokenResponseAC>(tokenResponseJson, new JsonSerializerSettings
                    {
                        ContractResolver = contractResolverSnakeCase,
                        Formatting = Formatting.Indented
                    });
                }
            }

            ConsumerCreditReportRequestAC consumerCreditReportRequest = new ConsumerCreditReportRequestAC()
            {
                Consumers = new ConsumersAC
                {
                    Addresses = new List<EquifaxAddressAC>()
                    {
                        new EquifaxAddressAC
                        {
                            Identifier = StringConstant.Current,
                            City = userInfo.Address.City,
                            State = userInfo.Address.StateAbbreviation,
                            StreetName = userInfo.Address.StreetLine,
                            ZipCode = userInfo.Address.ZipCode
                        }
                    },
                    DateOfBirth = Convert.ToDateTime(userInfo.DOB).ToString(StringConstant.DatePattern),
                    Name = new List<EquifaxNameAC>()
                    {
                        new EquifaxNameAC
                        {
                            Identifier = StringConstant.Current,
                            FirstName = userInfo.FirstName,
                            LastName = userInfo.LastName,
                            MiddleName = userInfo.MiddleName
                        }
                    },
                    PhoneNumbers = new List<PhoneNumbersAC>()
                    {
                        new PhoneNumbersAC
                        {
                            Identifier = StringConstant.Current,
                            Number = userInfo.PhoneNumber
                        }
                    },
                    SocialNum = new List<SocialNumAC>()
                    {
                        new SocialNumAC
                        {
                            Identifier = StringConstant.Current,
                            Number = userInfo.SSN
                        }
                    }
                }
            };

            DefaultContractResolver contractResolverCamelCase = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var consumerCreditReportJson = JsonConvert.SerializeObject(consumerCreditReportRequest, new JsonSerializerSettings
            {
                ContractResolver = contractResolverCamelCase,
                Formatting = Formatting.Indented
            });

            //Setup http request with header values with access token and serialize ConsumerCreditReportRequestAC into json in camel case 
            //and call consumer credit report api
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), configurationList.Single(x => x.Path == "Equifax:ConsumerCreditReportAPI").Value))
                {
                    request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, string.Format(StringConstant.HttpHeaderAuthorizationToken, tokenResponse.AccessToken));
                    request.Content = new StringContent(consumerCreditReportJson);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderAcceptJsonType);

                    var response = await httpClient.SendAsync(request);

                    //Check if IsSuccessCode then save json in userInfo
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.Content.Headers.ContentEncoding.Any(x => x == StringConstant.HttpHeaderGzip))
                        {
                            // Decompress manually
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                using (var decompressed = new GZipStream(stream, CompressionMode.Decompress))
                                {
                                    using (var streamReader = new StreamReader(decompressed))
                                    {
                                        userInfo.ConsumerCreditReportResponse = await streamReader.ReadToEndAsync();
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Use standard implementation if not compressed
                            userInfo.ConsumerCreditReportResponse = await response.Content.ReadAsStringAsync();
                        }
                    }
                    //Check if status code not found then assign null in userInfo
                    else if (response.StatusCode.Equals(System.Net.HttpStatusCode.InternalServerError))
                    {
                        userInfo.ConsumerCreditReportResponse = null;
                    }
                    else
                    {
                        throw new HttpRequestException(StringConstant.UnexpectedStatusCodeConsumerCreditReportEquifax);
                    }
                }
            }
            return userInfo;
        }

        #endregion
    }
}
