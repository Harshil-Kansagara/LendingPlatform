using System;

namespace LendingPlatform.Repository.ApplicationClass.Entity
{
    public class TaxFormValueLabelMappingAC
    {
        /// <summary>
        /// Unique Identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Extracted value from PDF
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Corrected value by banker
        /// </summary>
        public string CorrectedValue { get; set; }

        /// <summary>
        /// Label for the given value
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Confidence of extracted value (Range in between 0-1 value) 
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// Unique identifier of EntityTaxYearlyMapping object
        /// </summary>
        public Guid EntityTaxYearlyMappingId { get; set; }
    }
}
