namespace LendingPlatform.Repository.ApplicationClass.Products
{
    public class ProductDetailsAC
    {
        #region Public Properties

        /// <summary>
        /// Interest rate based on self declared credit score of loan initiator
        /// </summary>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Minimum product tenure 
        /// </summary>
        public decimal MinProductTenure { get; set; }

        /// <summary>
        /// Maximum product tenure
        /// </summary>
        public decimal MaxProductTenure { get; set; }

        /// <summary>
        /// Tenure stepper count
        /// </summary>
        public decimal TenureStepperCount { get; set; }

        /// <summary>
        /// Laon amount 
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Loan Period.
        /// </summary>
        public decimal Period { get; set; }

        /// <summary>
        /// Minimum product amount
        /// </summary>
        public decimal MinProductAmount { get; set; }

        /// <summary>
        /// Maximum product amount
        /// </summary>
        public decimal MaxProductAmount { get; set; }

        /// <summary>
        /// Amount stepper count
        /// </summary>
        public decimal AmountStepperCount { get; set; }

        /// <summary>
        /// Monthly payment amount
        /// </summary>
        public decimal MonthlyPayment { get; set; }
        /// <summary>
        /// Minimum monthly payment amount
        /// </summary>
        public decimal MinMonthlyPayment { get; set; }
        /// <summary>
        /// Maximum monthly payment amount
        /// </summary>
        public decimal MaxMonthlyPayment { get; set; }
        /// <summary>
        /// Total payment amount
        /// </summary>
        public string TotalPayment { get; set; }
        /// <summary>
        /// Total interest percentage
        /// </summary>
        public string TotalInterest { get; set; }
        #endregion
    }
}
