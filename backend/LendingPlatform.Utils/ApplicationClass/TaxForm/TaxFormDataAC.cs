using System.Collections.Generic;

namespace LendingPlatform.Utils.ApplicationClass.TaxForm
{
    public class TaxFormDataAC
    {
        public List<TaxFormSeedAC> TaxForms { get; set; }
        public List<CompanyStructureSeedAC> CompanyStructures { get; set; }
        public List<TaxFormCompanyStructureMappingAC> TaxFormCompanyStructureMappings { get; set; }
        public List<TaxFormLabelNameSeedAC> Fields { get; set; }
    }
}
