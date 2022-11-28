namespace LendingPlatform.Utils.ApplicationClass.Transunion.ConsumerCreditReportAPI
{
    public class ProductAC
    {
        #region Public Properties
        public string Code { get; set; }
        public SubjectAC Subject { get; set; }
        public ResponseInstructionsAC ResponseInstructions { get; set; }
        public PermissiblePurposeAC PermissiblePurpose { get; set; }
        #endregion
    }
}
