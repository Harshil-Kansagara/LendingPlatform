using LendingPlatform.Repository.CustomException;
using LendingPlatform.Utils.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LendingPlatform.Web.Client.Helpers
{
    public static class GlobalExceptionHandler
    {
        /// <summary>
        /// Catch the custom exception and update status code.
        /// </summary>
        /// <param name="app">IApplicationBuilder obeject.</param>
        public static void HandleCustomException(IApplicationBuilder app)
        {
            // Global handled but common catch exception.
            app.UseExceptionHandler(
                                        options =>
                                        {
                                            options.Run(
                                            async context =>
                                            {
                                                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                                                var problemDetails = new ProblemDetails();
                                                if (exceptionHandlerPathFeature != null)
                                                {
                                                    #region Update Http status code.
                                                    if (exceptionHandlerPathFeature.Error is InvalidResourceAccessException)
                                                    {
                                                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                                        problemDetails.Status = StatusCodes.Status403Forbidden;
                                                    }
                                                    else if (exceptionHandlerPathFeature.Error is DataNotFoundException)
                                                    {
                                                        context.Response.StatusCode = StatusCodes.Status204NoContent;
                                                        problemDetails.Status = StatusCodes.Status204NoContent;
                                                    }
                                                    else if (exceptionHandlerPathFeature.Error is ValidationException || exceptionHandlerPathFeature.Error is InvalidParameterException)
                                                    {
                                                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                                        problemDetails.Status = StatusCodes.Status400BadRequest;
                                                    }

                                                    #endregion

                                                    #region Write message and response content type.

                                                    context.Response.ContentType = StringConstant.HttpHeaderAcceptJsonType;
                                                    // Set the write message if exist and status code is not 204.
                                                    if (!string.IsNullOrEmpty(exceptionHandlerPathFeature.Error?.Message) && problemDetails.Status != StatusCodes.Status204NoContent)
                                                    {
                                                        problemDetails.Detail = exceptionHandlerPathFeature.Error.Message;

                                                        await context.Response.WriteAsync(JsonConvert.SerializeObject(problemDetails, new JsonSerializerSettings
                                                        {
                                                            ContractResolver = new DefaultContractResolver
                                                            {
                                                                NamingStrategy = new CamelCaseNamingStrategy()
                                                            },
                                                            Formatting = Formatting.Indented
                                                        }));
                                                    }

                                                    #endregion
                                                }
                                            });
                                        }
                                    );
        }
    }
}
