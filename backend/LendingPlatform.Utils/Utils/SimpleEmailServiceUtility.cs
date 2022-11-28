using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using LendingPlatform.Utils.ApplicationClass;
using LendingPlatform.Utils.Constants;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils
{
    public class SimpleEmailServiceUtility : ISimpleEmailServiceUtility
    {
        #region Private variables
        private readonly AmazonSimpleEmailServiceClient _client;
        private readonly IConfiguration _configuration;
        private readonly IFileOperationsUtility _fileOperationsUtility;
        #endregion

        #region Constructor
        public SimpleEmailServiceUtility(IConfiguration configuration, IFileOperationsUtility fileOperationsUtility)
        {
            _configuration = configuration;
            var credentials = new BasicAWSCredentials(configuration.GetValue<string>("Aws:AwsSecretKey"),
                configuration.GetValue<string>("Aws:AwsSecretAccessKey"));
            _client = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.GetBySystemName(configuration.GetValue<string>("Aws:AwsRegion")));
            _fileOperationsUtility = fileOperationsUtility;
        }
        #endregion

        /// <summary>
        /// Method creates and sends an email to shareholders.
        /// </summary>
        /// <param name="emailLoanDetails">List of EmailLoanDetailsAC object</param>
        /// <returns></returns>
        public async Task SendEmailToShareHoldersAsync(List<EmailLoanDetailsAC> emailLoanDetails)
        {
            SendBulkTemplatedEmailRequest emailRequest = new SendBulkTemplatedEmailRequest
            {
                Source = _configuration.GetValue<string>("LenderInstitute:Email"),

                //Assign common variables' values.
                DefaultTemplateData = string.Format("{{ \"{0}\":\"{1}\", \"{2}\":\"{3}\" }}",
                        StringConstant.TodaysDate, string.Format("{0} {1}, {2}", DateTime.Now.ToString("MMM"), DateTime.Now.ToString("dd"), DateTime.Now.ToString("yyy")),
                        StringConstant.LenderInstituteName, _configuration.GetValue<string>("LenderInstitute:Name"))
            };

            string currentTemplateFile = null;
            foreach (var emailLoanDetail in emailLoanDetails)
            {
                // If template changes or it is new then only create new template.
                if (currentTemplateFile == null || currentTemplateFile != emailLoanDetail.TemplateFile)
                {
                    currentTemplateFile = emailLoanDetail.TemplateFile;

                    //Set email template's path
                    string parentPath = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
                    string path = Path.Combine(parentPath, StringConstant.EmailTemplatesFolderName, emailLoanDetail.TemplateFile);

                    //Creates a template.
                    string fileContent = _fileOperationsUtility.ReadFileContent(path);
                    CreateTemplateRequest createTemplateRequest = new CreateTemplateRequest
                    {
                        Template = new Template
                        {
                            TemplateName = emailLoanDetail.Subject.Replace(" ", ""),
                            SubjectPart = emailLoanDetail.Subject,
                            HtmlPart = fileContent
                        }
                    };

                    try
                    {
                        // If the same template is already created on AWS then don't create it again.
                        if (await _client.GetTemplateAsync(new GetTemplateRequest { TemplateName = createTemplateRequest.Template.TemplateName }) != null)
                        {
                            await _client.CreateTemplateAsync(createTemplateRequest);
                            emailRequest.Template = createTemplateRequest.Template.TemplateName;
                        }
                    }
                    catch (AmazonSimpleEmailServiceException exception)
                    {
                        Log.Fatal(exception, StringConstant.EmailSendingException);
                    }
                }

                //Assign shareholders' names and relationships.
                foreach (var shareHolder in emailLoanDetail.ShareHoldersDetails)
                {
                    BulkEmailDestination destination = new BulkEmailDestination
                    {
                        Destination = new Destination { ToAddresses = new List<string> { shareHolder.Email } },
                        ReplacementTemplateData = string.Format("{{ \"{0}\":\"{1}\", \"{2}\":\"{3}\", \"{4}\":\"{5}\", \"{6}\":\"{7}\", \"{8}\":\"{9}\" }}",
                        StringConstant.CustomerName, shareHolder.Name, StringConstant.Relation, shareHolder.Relationship,
                        StringConstant.ApplicationNumber, emailLoanDetail.LoanApplicationNumber, StringConstant.CompanyName, emailLoanDetail.CompanyName,
                        StringConstant.RedirectUrl, emailLoanDetail.LoanApplicationRedirectUrl)
                    };
                    emailRequest.Destinations.Add(destination);

                    if (emailRequest.Destinations.Count == 50)
                    {
                        try
                        {
                            await _client.SendBulkTemplatedEmailAsync(emailRequest);
                            emailRequest.Destinations.Clear();
                        }
                        catch (AmazonSimpleEmailServiceException exception)
                        {
                            Log.Fatal(exception, StringConstant.EmailSendingException);
                        }
                    }
                }
            }
        }
    }
}
