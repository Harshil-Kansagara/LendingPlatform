using Newtonsoft.Json;
using System.Collections.Generic;

namespace LendingPlatform.Repository.ApplicationClass
{
    public class FilterModelAC
    {
        #region Public Properties
        /// <summary>
        /// Page number
        /// </summary>
        public int? PageNo { get; set; }
        /// <summary>
        /// Page Record Count
        /// </summary>
        public int? PageRecordCount { get; set; }
        [JsonIgnore]
        public List<FilterAC> Filters { get; set; }
        /// <summary>
        /// Filter Object
        /// [{"field1","operator","value"},{"field2","operator","value"}]
        /// </summary>
        public string Filter
        {
            get
            {
                return null;
            }
            set
            {
                this.Filters = JsonConvert.DeserializeObject<List<FilterAC>>(value);
            }
        }
        /// <summary>
        /// Field name for sorting.
        /// </summary>
        public string SortField { get; set; }
        /// <summary>
        /// Sorting option
        /// </summary>
        public string SortBy { get; set; }
        #endregion
    }
}
