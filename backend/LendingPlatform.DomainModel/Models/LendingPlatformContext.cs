using Audit.Core;
using Audit.EntityFramework;
using LendingPlatform.DomainModel.Models.EntityInfo;
using LendingPlatform.DomainModel.Models.LoanApplicationInfo;
using LendingPlatform.Utils.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;

namespace LendingPlatform.DomainModel.Models
{
    [AuditDbContext(Mode = AuditOptionMode.OptOut, IncludeEntityObjects = false, AuditEventType = "{database}_{context}")]
    public class LendingPlatformContext : AuditDbContext
    {
        public LendingPlatformContext(DbContextOptions<LendingPlatformContext> options) : base(options)
        {
            Audit.Core.Configuration.Setup()
            .UseEntityFramework(_ => _
                .AuditTypeMapper(t => typeof(AuditLog))
                .AuditEntityAction<AuditLog>((ev, entry, entity) =>
                {
                    entity.Action = entry.Action;
                    entity.TableName = entry.EntityType.Name;
                    entity.TablePk = Guid.Parse(entry.PrimaryKey.First().Value.ToString());
                    entity.AuditJson = entry.ToJson();
                    entity.CreatedOn = DateTime.UtcNow;
                    if (ev.CustomFields != null && ev.CustomFields.Any(s => s.Key.Equals(StringConstant.AuditLogCustomFieldName) && s.Value != null))
                    {
                        AuditLog auditLog = (AuditLog)ev.CustomFields[StringConstant.AuditLogCustomFieldName];
                        entity.LogBlockName = auditLog.LogBlockName;
                        entity.LogBlockNameId = auditLog.LogBlockNameId;
                        entity.CreatedByUserId = auditLog.CreatedByUserId;
                        entity.CreatedByBankUserId = auditLog.CreatedByBankUserId;
                        entity.IpAddress = auditLog.IpAddress;
                    }
                })
            .IgnoreMatchedProperties(true));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BusinessAge>()
                .HasIndex(x => x.Age)
                .IsUnique();
            modelBuilder.Entity<BusinessAge>()
                .HasIndex(x => x.Order)
                .IsUnique();
            modelBuilder.Entity<CompanyStructure>()
                .HasIndex(x => x.Structure)
                .IsUnique();
            modelBuilder.Entity<CompanyStructure>()
                .HasIndex(x => x.Order)
                .IsUnique();
            modelBuilder.Entity<CompanySize>()
                .HasIndex(x => x.Size)
                .IsUnique();
            modelBuilder.Entity<CompanySize>()
                .HasIndex(x => x.Order)
                .IsUnique();
            modelBuilder.Entity<Relationship>()
                .HasIndex(x => x.Relation)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(x => x.SSN)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Phone)
                .IsUnique();

            modelBuilder.Entity<IndustryExperience>()
                .HasIndex(x => x.Experience)
                .IsUnique();
            modelBuilder.Entity<IndustryExperience>()
                .HasIndex(x => x.Order)
                .IsUnique();

            modelBuilder.Entity<LoanApplication>()
                .HasIndex(x => x.LoanApplicationNumber)
                .IsUnique();
            modelBuilder.Entity<LoanPurpose>()
                .HasIndex(x => x.Name)
                .IsUnique();
            modelBuilder.Entity<LoanPurpose>()
                .HasIndex(x => x.Order)
                .IsUnique();

            modelBuilder.Entity<FinancialStatement>()
                .HasIndex(x => x.Name)
                .IsUnique();
            modelBuilder.Entity<Bank>()
                .HasIndex(x => x.SWIFTCode)
                .IsUnique();
            modelBuilder.Entity<IntegratedServiceConfiguration>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<EntityFinance>()
                .HasIndex(x => x.CreatedOn);

            modelBuilder.Entity<EntityFinance>()
                .HasIndex(x => x.Version);

            modelBuilder.Entity<EntityFinance>()
                .Property(x => x.SurrogateId)
                .UseIdentityAlwaysColumn()
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

            modelBuilder.Entity<EntityFinance>()
               .Property(x => x.SurrogateId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);



            modelBuilder.Entity<EntityTaxForm>()
                .HasIndex(x => x.CreatedOn);

            modelBuilder.Entity<EntityTaxForm>()
                .HasIndex(x => x.Version);

            modelBuilder.Entity<EntityTaxForm>()
                .Property(x => x.SurrogateId)
                .UseIdentityAlwaysColumn().Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

            modelBuilder.Entity<EntityTaxForm>()
                .Property(x => x.SurrogateId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);




            modelBuilder.Entity<CreditReport>()
                .HasIndex(x => x.CreatedOn);

            modelBuilder.Entity<CreditReport>()
                .HasIndex(x => x.Version);

            modelBuilder.Entity<CreditReport>()
                .Property(x => x.SurrogateId)
                .UseIdentityAlwaysColumn()
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            modelBuilder.Entity<CreditReport>()
                .Property(x => x.SurrogateId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            modelBuilder.Entity<SubLoanPurpose>()
                .HasIndex(x => x.Name)
                .IsUnique();

            // Delete versions of loan independent entities for each loan automaticallhy when that loan gets deleted.
            modelBuilder.Entity<EntityFinance>()
                .HasOne(a => a.LoanApplication)
                .WithMany(b => b.EntityFinances)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EntityTaxForm>()
                .HasOne(a => a.LoanApplication)
                .WithMany(b => b.EntityTaxForms)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EntityAdditionalDocument>()
                .HasOne(a => a.LoanApplication)
                .WithMany(b => b.AdditionalDocuments)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CreditReport>()
                .HasOne(a => a.LoanApplication)
                .WithMany(b => b.CreditReports)
                .OnDelete(DeleteBehavior.Cascade);


        }

        public DbSet<AuditLog> AuditLog { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<BusinessAge> BusinessAge { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<CompanyStructure> CompanyStructure { get; set; }
        public DbSet<CompanySize> CompanySize { get; set; }
        public DbSet<Entity> Entity { get; set; }
        public DbSet<EntityFinance> EntityFinance { get; set; }
        public DbSet<EntityFinanceYearlyMapping> EntityFinanceYearlyMapping { get; set; }
        public DbSet<EntityFinanceStandardAccount> EntityFinanceStandardAccount { get; set; }
        public DbSet<EntityLoanApplicationMapping> EntityLoanApplicationMapping { get; set; }
        public DbSet<EntityRelationshipMapping> EntityRelationshipMapping { get; set; }

        public DbSet<FinancialStatement> FinancialStatement { get; set; }
        public DbSet<LoanApplication> LoanApplication { get; set; }
        public DbSet<LoanPurpose> LoanPurpose { get; set; }
        public DbSet<Relationship> Relationship { get; set; }
        public DbSet<NAICSIndustryType> NAICSIndustryType { get; set; }
        public DbSet<Document> Document { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<BankUser> BankUser { get; set; }
        public DbSet<UserLoanSectionMapping> UserLoanSectionMapping { get; set; }

        // Loan Product
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductSubPurposeMapping> ProductSubPurposeMapping { get; set; }
        public DbSet<ProductRangeTypeMapping> ProductRangeTypeMapping { get; set; }
        public DbSet<ProductTypeMapping> ProductTypeMapping { get; set; }
        public DbSet<LoanRangeType> LoanRangeType { get; set; }
        public DbSet<LoanType> LoanType { get; set; }

        public DbSet<Consent> Consent { get; set; }
        public DbSet<EntityLoanApplicationConsent> EntityLoanApplicationConsent { get; set; }

        public DbSet<CreditReport> CreditReport { get; set; }
        public DbSet<DescriptionPoint> DescriptionPoint { get; set; }

        // Yodlee
        public DbSet<ProviderBank> ProviderBank { get; set; }
        public DbSet<BankAccountTransaction> BankAccountTransaction { get; set; }

        public DbSet<Bank> Bank { get; set; }

        public DbSet<EntityBankDetail> EntityBankDetail { get; set; }

        public DbSet<Section> Section { get; set; }

        public DbSet<IntegratedServiceConfiguration> IntegratedServiceConfiguration { get; set; }

        public DbSet<IndustryExperience> IndustryExperience { get; set; }

        // Tax Return
        public DbSet<TaxForm> TaxForm { get; set; }
        public DbSet<TaxFormCompanyStructureMapping> TaxFormCompanyStructureMapping { get; set; }
        public DbSet<EntityTaxForm> EntityTaxForm { get; set; }
        public DbSet<EntityTaxYearlyMapping> EntityTaxYearlyMapping { get; set; }
        public DbSet<TaxFormLabelNameMapping> TaxFormLabelNameMapping { get; set; }
        public DbSet<TaxFormValueLabelMapping> TaxFormValueLabelMapping { get; set; }
        public DbSet<OCRModelMapping> OCRModelMapping { get; set; }
        public DbSet<LoanApplicationSnapshot> LoanApplicationSnapshot { get; set; }
        public DbSet<LoanPurposeRangeTypeMapping> LoanPurposeRangeTypeMapping { get; set; }
        public DbSet<SubLoanPurpose> SubLoanPurpose { get; set; }

        //Pesonal Finances
        public DbSet<PersonalFinanceAccount> PersonalFinanceAccount { get; set; }
        public DbSet<PersonalFinanceCategory> PersonalFinanceCategory { get; set; }
        public DbSet<PersonalFinanceAttribute> PersonalFinanceAttribute { get; set; }
        public DbSet<PersonalFinanceConstant> PersonalFinanceConstant { get; set; }
        public DbSet<PersonalFinanceParentChildCategoryMapping> ParentChildCategoryMapping { get; set; }
        public DbSet<PersonalFinanceAttributeCategoryMapping> AttributeCategoryMapping { get; set; }
        public DbSet<PersonalFinanceResponse> PersonalFinanceResponse { get; set; }

        //Additional Documents
        public DbSet<EntityAdditionalDocument> EntityAdditionalDocument { get; set; }
    }
}
