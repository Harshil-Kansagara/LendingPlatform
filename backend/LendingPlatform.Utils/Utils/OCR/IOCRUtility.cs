using LendingPlatform.Utils.ApplicationClass.TaxForm;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingPlatform.Utils.Utils.OCR
{
    public interface IOCRUtility
    {
        /// <summary>
        /// Method is to get the extracted value of the PDF from the form recognnizer OCR
        /// </summary>
        /// <param name="modelId">Model Id</param>
        /// <param name="pdfURL">Pdf URL</param>
        /// <returns>List of OCRExtractedValueAC</returns>
        Task<List<OCRExtractedValueAC>> RecognizeContentModelAsync(string modelId, string pdfURL);
    }
}
