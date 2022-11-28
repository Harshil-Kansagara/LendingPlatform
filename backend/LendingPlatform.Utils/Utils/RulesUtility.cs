using LendingPlatform.Utils.ApplicationClass.Product;
using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class RulesUtility : IRulesUtility
    {
        private readonly IConfiguration _configuration;
        public RulesUtility(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Method to execute drools rules for mapping standard accounts of finances
        /// </summary>
        /// <param name="inputObj"></param>
        /// <returns></returns>
        public async Task<JToken> ExecuteRuleForFetchingFinanceStandardAccountsAsync(object inputObj)
        {
            JToken defaultJToken = null;
            var requestObj = new
            {
                modelNamespace = _configuration.GetValue<string>("Drools:FinancialStatemetsModelNamespace"),
                modelName = _configuration.GetValue<string>("Drools:FinancialStatemetsModelName"),
                dmnContext = JsonConvert.SerializeObject(inputObj, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                })
            };

            var responseObject = await SendRequestAsync(requestObj);
            if (responseObject != null)
            {
                var dmnDecision = responseObject["result"]["dmn-evaluation-result"]["dmn-context"]["StandardJamoonStatement"];
                return dmnDecision;
            }

            return defaultJToken;
        }

        /// <summary>
        /// Method to Execute rule for evaluating loan 
        /// </summary>
        /// <param name="inputObj"></param>
        /// <returns></returns>
        public async Task<JToken> ExecuteRuleForEvaluatingLoanAsync(object inputObj)
        {
            JToken defaultJToken = null;
            var requestObj = new
            {
                modelNamespace = _configuration.GetValue<string>("Drools:LoanEvaluationModelNamespace"),
                modelName = _configuration.GetValue<string>("Drools:LoanEvaluationModelName"),
                dmnContext = JsonConvert.SerializeObject(inputObj, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                })
            };

            // Call http request
            var responseObject = await SendRequestAsync(requestObj);

            if (responseObject != null)
            {
                var dmnDecision = responseObject["result"]["dmn-evaluation-result"]["dmn-context"]["Evaluate Loan"];
                return dmnDecision;
            }

            return defaultJToken;
        }

        /// <summary>
        /// Method is to execute product drool rule
        /// </summary>
        /// <param name="productRule">ProductRuleAC object</param>
        /// <returns>List of ProductPercentageSuitabilityAC</returns>
        public async Task<List<ProductPercentageSuitabilityAC>> ExecuteProductRules(ProductRuleAC productRule)
        {
            var productsPercentageSuitability = new List<ProductPercentageSuitabilityAC>();
            var productRuleObjectAsInput = new
            {
                ProductRuleData = productRule
            };

            var requestObj = new
            {
                modelNamespace = _configuration.GetSection("Drools:ProductRulesModelNamespace").Value,
                modelName = _configuration.GetSection("Drools:ProductRulesModelName").Value,
                dmnContext = JsonConvert.SerializeObject(productRuleObjectAsInput, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                })
            };

            // Call Http Request
            var responseObject = await SendRequestAsync(requestObj);

            if (responseObject != null)
            {
                var dmnDecisionArray = responseObject["result"]["dmn-evaluation-result"]["dmn-context"]["ProductIdWithSuitabilityPercentageList"] as JArray;
                if (dmnDecisionArray != null && dmnDecisionArray.Any())
                {
                    var productPercentageSuitabilities = JsonConvert.DeserializeObject<List<ProductPercentageSuitabilityAC>>(JsonConvert.SerializeObject(dmnDecisionArray));

                    productPercentageSuitabilities = productPercentageSuitabilities.OrderByDescending(x => x.PercentageSuitability).ToList();

                    foreach (var productPercentageSuitability in productPercentageSuitabilities)
                    {
                        productPercentageSuitability.PercentageSuitability = Math.Round(productPercentageSuitability.PercentageSuitability);
                        if (productPercentageSuitability.PercentageSuitability == productPercentageSuitabilities.First().PercentageSuitability)
                        {
                            productPercentageSuitability.IsRecommended = true;
                        }
                    }
                    productsPercentageSuitability.AddRange(productPercentageSuitabilities);
                }
                else
                {
                    var dmnDecision = responseObject["result"]["dmn-evaluation-result"]["dmn-context"]["ProductIdWithSuitabilityPercentageList"];

                    if (dmnDecision != null)
                    {
                        var productPercentageSuitabilityAC = JsonConvert.DeserializeObject<ProductPercentageSuitabilityAC>(JsonConvert.SerializeObject(dmnDecision));
                        productPercentageSuitabilityAC.IsRecommended = true;

                        productsPercentageSuitability.Add(productPercentageSuitabilityAC);
                    }
                }
            }

            return productsPercentageSuitability;
        }

        /// <summary>
        /// Method to send http request to rule engine
        /// </summary>
        /// <param name="requestObj"></param>
        /// <returns></returns>
        private async Task<JObject> SendRequestAsync(dynamic requestObj)
        {
            JObject defaultResponse = null;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(StringConstant.HttpMethodPost), _configuration.GetValue<string>("Drools:ContainerUrl")))
                {

                    string userNamePasswordKey = _configuration.GetValue<string>("Drools:Username") + ":" + _configuration.GetValue<string>("Drools:Password");
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(userNamePasswordKey));
                    request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAuthorization, $"Basic {base64authorization}");

                    request.Headers.TryAddWithoutValidation(StringConstant.HttpHeaderAccept, StringConstant.HttpHeaderAcceptJsonType);
                    string requestContent = string.Format("{{ \"model-namespace\" : \"{0}\", \"model-name\" : \"{1}\", \"dmn-context\" : {2} }}", requestObj.modelNamespace, requestObj.modelName, requestObj.dmnContext);

                    request.Content = new StringContent(requestContent);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var theResult = await response.Content.ReadAsStringAsync();
                        return JObject.Parse(theResult);
                    }

                }
            }
            return defaultResponse;

        }
    }
}
