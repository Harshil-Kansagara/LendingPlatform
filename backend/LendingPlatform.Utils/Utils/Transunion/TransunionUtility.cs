using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.ApplicationClass.Transunion.ConsumerCreditReportAPI;
using LendingPlatform.Utils.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LendingPlatform.Utils.Utils.Transunion
{
    public class TransunionUtility : ITransunionUtility
    {
        #region Private Variable(s)
        private readonly IAmazonServicesUtility _amazonS3Utility;
        #endregion

        #region Contructor
        public TransunionUtility(IAmazonServicesUtility amazonS3Utility)
        {
            _amazonS3Utility = amazonS3Utility;
        }
        #endregion

        #region Private Method(s)
        private string ToCamelCasing(string input) => char.ToLowerInvariant(input[0]) + input.Substring(1);
        #endregion

        #region Public method(s)
        /// <summary>
        /// Method to fetch consumer credit report from transunion API
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="transunionConfigurationJson">Transunion ConfigurationJson</param>
        /// <returns></returns>
        public async Task<UserInfoAC> FetchConsumerCreditReportAsync(UserInfoAC userInfo, string transunionConfigurationJson)
        {
            var configurationList = JsonConvert.DeserializeObject<List<ThirdPartyConfigurationAC>>(transunionConfigurationJson);

            //Initialize the CreditBureauAC class object with values
            CreditBureauAC creditBureauRequest = new CreditBureauAC
            {
                Document = StringConstant.TransunionDocument,
                Version = configurationList.Single(x => x.Path == "Transunion:Version").Value,
                TransactionControl = new TransactionControlAC
                {
                    UserRefNumber = configurationList.Single(x => x.Path == "Transunion:UserRefNumber").Value,
                    Subscriber = new SubscriberAC
                    {
                        IndustryCode = configurationList.Single(x => x.Path == "Transunion:SubscriberIndustryCode").Value,
                        MemberCode = configurationList.Single(x => x.Path == "Transunion:SubscriberMemberCode").Value,
                        InquirySubscriberPrefixCode = configurationList.Single(x => x.Path == "Transunion:SubscriberInquirySubscriberPrefixCode").Value,
                        Password = configurationList.Single(x => x.Path == "Transunion:SubscriberPassword").Value
                    },
                    Options = new OptionsAC
                    {
                        ProcessingEnvironment = configurationList.Single(x => x.Path == "Transunion:OptionsProcessingEnvironment").Value,
                        Country = configurationList.Single(x => x.Path == "Transunion:OptionsCountry").Value,
                        Language = configurationList.Single(x => x.Path == "Transunion:OptionsLanguage").Value,
                        ContractualRelationship = configurationList.Single(x => x.Path == "Transunion:OptionsContractualRelationship").Value,
                        PointOfSaleIndicator = configurationList.Single(x => x.Path == "Transunion:OptionsPointOfSaleIndicator").Value
                    }
                },
                Product = new ProductAC
                {
                    Code = StringConstant.TransunionProductCode,
                    Subject = new SubjectAC
                    {
                        Number = StringConstant.TransunionSubjectNumber,
                        SubjectRecord = new SubjectRecordAC
                        {
                            Indicative = new IndicativeAC
                            {
                                Name = new ApplicationClass.Transunion.ConsumerCreditReportAPI.NameAC
                                {
                                    Person = new PersonAC
                                    {
                                        First = userInfo.FirstName,
                                        Middle = userInfo.MiddleName,
                                        Last = userInfo.LastName
                                    }
                                },
                                Address = new ApplicationClass.Transunion.ConsumerCreditReportAPI.AddressAC
                                {
                                    Status = StringConstant.TransunionAddressStatus,
                                    Street = new StreetAC
                                    {
                                        Number = userInfo.Address.StreetLine.Split(" ")[0],
                                        Name = string.Join(" ", userInfo.Address.StreetLine.Split().Skip(1))
                                    },
                                    Location = new LocationAC
                                    {
                                        City = userInfo.Address.City,
                                        State = userInfo.Address.StateAbbreviation,
                                        ZipCode = userInfo.Address.ZipCode
                                    }
                                },
                                SocialSecurity = new SocialSecurityAC
                                {
                                    Number = userInfo.SSN
                                },
                                DateOfBirth = userInfo.DOB.ToString(StringConstant.DatePatternWithDash)
                            },
                            AddOnProduct = new AddOnProductAC
                            {
                                Code = configurationList.Single(x => x.Path == "Transunion:AddOnProductCode").Value,
                                ScoreModelProduct = true
                            }
                        }
                    },
                    ResponseInstructions = new ResponseInstructionsAC
                    {
                        ReturnErrorText = true
                    },
                    PermissiblePurpose = new PermissiblePurposeAC
                    {
                        InquiryECOADesignator = StringConstant.TransunionInquiryECOADesignator
                    }
                }
            };

            //To convert Pascal case to camel case First need to serialize class to json with Camel Case Naming Strategy and then deserialize to XNode object
            var camelCaseResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = camelCaseResolver };

            //serialize class to json
            var jsonRequest = JsonConvert.SerializeObject(creditBureauRequest, jsonSerializerSettings);

            //desrialize to XNode object
            XNode xmlRequest = JsonConvert.DeserializeXNode(jsonRequest, ToCamelCasing(nameof(CreditBureauAC)));

            //Assign namespace() to each node of xml
            var xmlDocument = XDocument.Parse(xmlRequest.ToString());
            xmlDocument.Root.Name = StringConstant.TransunionRootNodeName;
            XNamespace xs = StringConstant.TransunionNamespace;
            foreach (var element in xmlDocument.Descendants())
            {
                element.Name = xs + element.Name.LocalName;
            }

            //Use Aws Utility GetPreSignedURL() method to get presigned url for certificate
            string certificateUrl = _amazonS3Utility.GetPreSignedURL(configurationList.Single(x => x.Path == "Transunion:CertificateAwsBucketKey").Value);

            //Get certificate from url presigned url
            using var webClient = new WebClient();
            var certificateData = webClient.DownloadData(certificateUrl);

            //Use HttpClient Handler to attach certificate
            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new X509Certificate2(certificateData, configurationList.Single(x => x.Path == "Transunion:CertificatePassword").Value));
            handler.ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true;

            //Using HttpClient request send xml request
            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), configurationList.Single(x => x.Path == "Transunion:BaseAPIUrl").Value))
                {
                    request.Content = new StringContent(xmlDocument.ToString());
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(StringConstant.HttpHeaderAcceptXmlType);
                    var response = await httpClient.SendAsync(request);
                    userInfo.ConsumerCreditReportResponse = await response.Content.ReadAsStringAsync();

                    //Check if request was sucessful
                    response.EnsureSuccessStatusCode();
                    try
                    {
                        var XDocumentHitInfoNode = XDocument.Parse(userInfo.ConsumerCreditReportResponse).Descendants().FirstOrDefault(x => x.Name.LocalName == StringConstant.TransunionFileHitIndicatorNodeName).Value;

                        //If Transunion does not found any credit report linked with provided details then set null
                        if (XDocumentHitInfoNode == StringConstant.TransunionRegularNoHitInfo || XDocumentHitInfoNode == null)
                        {
                            userInfo.ConsumerCreditReportResponse = null;
                        }
                        else
                        {
                            //Add score of consumer
                            userInfo.Score = Int32.Parse(XDocument.Parse(userInfo.ConsumerCreditReportResponse).Descendants().First(x => x.Name.LocalName == StringConstant.TransunionScoreResultsNodeName).Value);

                            //Check for Bankruptcy
                            if (XDocument.Parse(userInfo.ConsumerCreditReportResponse).Descendants().Any(x => x.Name.LocalName == StringConstant.TransunionPublicRecordNodeName))
                            {
                                userInfo.Bankruptcy = true;
                            }
                        }
                    }
                    catch
                    {
                        throw new InvalidDataException(String.Format("{0} {1} {2} {3}", StringConstant.InvalidTransunionRequest, xmlDocument.ToString(SaveOptions.DisableFormatting), StringConstant.InvalidResponse, userInfo.ConsumerCreditReportResponse));
                    }

                }
            }
            return userInfo;
        }
        #endregion
    }
}
