using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using LendingPlatform.Utils.ApplicationClass.TaxForm;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils.OCR
{
    public class OCRUtility : IOCRUtility
    {
        #region Private Variable
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public OCRUtility(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Method is to get the extracted value of the PDF from the form recognnizer OCR
        /// </summary>
        /// <param name="modelId">Model Id</param>
        /// <param name="pdfURL">Pdf URL</param>
        /// <returns>List of OCRExtractedValueAC</returns>
        public async Task<List<OCRExtractedValueAC>> RecognizeContentModelAsync(string modelId, string pdfURL)
        {
            List<OCRExtractedValueAC> ocrExtractedValues = new List<OCRExtractedValueAC>();
            var recognizeClient = AuthenticateClient();
            RecognizedFormCollection forms = await recognizeClient
            .StartRecognizeCustomFormsFromUri(modelId, new Uri(pdfURL, UriKind.Absolute), new RecognizeCustomFormsOptions
            {
                ContentType = FormContentType.Pdf,
                IncludeFieldElements = true,

            }).WaitForCompletionAsync();
            foreach (RecognizedForm form in forms)
            {
                foreach (FormField field in form.Fields.Values)
                {
                    if (field != null)
                    {
                        string value;
                        if (field.ValueData?.Text?.Equals("Unselected") ?? true)
                        {
                            value = "No";
                        }
                        else if (field.ValueData.Text.Equals("Selected"))
                        {
                            value = "Yes";
                        }
                        else
                        {
                            value = field.ValueData.Text;
                        }

                        ocrExtractedValues.Add(new OCRExtractedValueAC()
                        {
                            Label = field.Name.Split('_')[1],
                            Value = value,
                            Confidence = field.Confidence
                        });
                    }
                }
            }
            return ocrExtractedValues;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Method is to authenticate client for form recognizer
        /// </summary>
        /// <returns></returns>
        private FormRecognizerClient AuthenticateClient()
        {
            string endpoint = _configuration.GetSection("Cognitive:Endpoint").Value;
            string apiKey = _configuration.GetSection("Cognitive:Apikey").Value;
            var credential = new AzureKeyCredential(apiKey);
            var client = new FormRecognizerClient(new Uri(endpoint), credential);
            return client;
        }
        #endregion
    }
}
