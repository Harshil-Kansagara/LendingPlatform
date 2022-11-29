import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AppService } from '../../../../services/app.service';
import { Router } from '@angular/router';
import { Constant } from '../../../../shared/constant';
import { ToastrService } from 'ngx-toastr';
import { NgxFileDropEntry, FileSystemFileEntry } from 'ngx-file-drop';
import {
  EntityAC, TaxAC, ConfigurationAC, EntityService, GlobalService,
  EntityTaxAccountAC, DocumentAC, AwsSettings, ApplicationService, LoanApplicationStatusType, ProblemDetails, ResourceType
} from '../../../../utils/serviceNew';
import { EventEmitterService } from '../../../../services/event-emitter.service';

@Component({
  selector: 'app-taxes',
  templateUrl: './taxes.component.html',
  styleUrls: ['./taxes.component.scss'],
})
export class TaxesComponent implements OnInit {

  entityId: string;
  applicationId: string;
  hasTaxesAlready = false;
  taxesRetrievalDate: Date;
  taxesInfoTitle = Constant.taxesInfoTitle;
  entityAC: EntityAC;
  entityData: EntityAC;
  deletedTaxes:TaxAC[];
  configurationAC: ConfigurationAC;
  financeRedirectUrl = Constant.personalFinancesRedirectUrl;
  fileData;
  fileName;
  years: string[];
  currentSection: string;
  isFileUploadAllowed = true;
  isViewOnlyMode = false;

  pdfIcon = '../../assets/images/pdf.svg';
  loader = true;

  constructor(readonly appService: AppService,
    private readonly router: Router,
    private readonly toastrService: ToastrService,
    private readonly entityService: EntityService,
    private readonly globalService: GlobalService,
    private readonly eventEmitterService: EventEmitterService,
    private readonly applicationService: ApplicationService, 
    private readonly changeDetectorRef: ChangeDetectorRef) {
    this.deletedTaxes = [];
    this.years = [];
    this.appService.updateLoader(false);
  }

  async ngOnInit() {
    if (!this.appService.taxFileUploadingInProgress && this.appService.showTaxTimer) {
      this.appService.showTaxTimer = false;
    }
    this.currentSection = await this.appService.getCurrentSectionName();
    this.isViewOnlyMode = await this.appService.isViewOnlyMode();
    const currentStatus = LoanApplicationStatusType[await this.appService.getCurrentLoanApplicationStatus()];
    this.entityId = await this.appService.getCurrentCompanyId();
    this.applicationId = await this.appService.getCurrentLoanApplicationId();
    
    if (currentStatus && LoanApplicationStatusType[currentStatus] !== LoanApplicationStatusType.Draft) {
      // Get the tax returns files from the Database.
      this.initializeTaxReturnInfo(ResourceType.Loan);
    } else {
      if (!this.entityId) {
        this.router.navigate([Constant.routeRootUrl]);
      } else {
        // Get the tax returns files from the Database.
        this.initializeTaxReturnInfo(ResourceType.Company);
      }
    }
  }

  /**
   * Method is to upload file to temp folder in S3 bucket when file is dropped 
   * @param droppedFiles NgxFileDropEntry array
   */
  dropped(droppedFiles: NgxFileDropEntry[]) {
    
    if (droppedFiles.length > this.years.length || ((this.appService.taxes.length + droppedFiles.length) > this.entityAC.periods.length)) {
      this.toastrService.error(`${Constant.fileExceeded}${this.entityAC.periods.length}`);
      return;
    }
    for (const droppedFile of droppedFiles) {
      if (droppedFile.fileEntry.isFile && this.isFileAllowed(droppedFile.fileEntry.name)) {
        const fileEntry = droppedFile.fileEntry as FileSystemFileEntry;
        fileEntry.file((file: File) => {
          if(!this.isFileSizeAllowed(file.size)){
            this.toastrService.error(Constant.fileSizeExceeded);
            return;
          }
          this.fileData = file;
          this.fileName = file.name;
          const taxAC = new TaxAC();
          taxAC.entityTaxAccount = new EntityTaxAccountAC();
          taxAC.entityTaxAccount.document = new DocumentAC();
          taxAC.entityTaxAccount.document.name = this.fileName;
          this.appService.taxes.push(taxAC);

          this.appService.taxesData.push({ taxes: taxAC, isDbData: false, isFileUploading: true });
          this.appService.taxFileUploadingInProgress = true;
          this.fileUpload(taxAC);
          if (this.appService.taxesData.length === this.entityAC.periods.length){
            this.isFileUploadAllowed = false;
          }
        });
      } else {
        this.toastrService.error(Constant.fileExtensionNotFound);
      }
    }
  }

  /**
   * Method is to verify that file extension is PDF or not
   * @param fileName File name
   */
  isFileAllowed(fileName: string) {
    let isFileAllowed = false;
    const allowedFiles = ['.pdf', '.PDF'];
    const regex = /(?:\.([^.]+))?$/;
    const extension = regex.exec(fileName);
    if (undefined !== extension && null !== extension) {
      for (const ext of allowedFiles) {
        if (ext === extension[0]) {
          isFileAllowed = true;
        }
      }
    }
    return isFileAllowed;
  }

  isFileSizeAllowed(fileSize: number){
    const maxFileSize = 20971520 ;
    if(fileSize > maxFileSize){
      return false;
    }
    return true;
  }

  /**
   * Method is to remove the file from the list
   * @param i index of the file in list
   */
  fileRemove(i: number) {
    if (this.appService.taxes[i].entityTaxAccount.period){
      this.years = this.years.concat(this.appService.taxes[i].entityTaxAccount.period);
    }
    this.deletedTaxes.push(this.appService.taxes[i]);
    this.appService.taxes.splice(i, 1);
    this.appService.taxesData.splice(i, 1);
    if (this.appService.taxesData.length < this.entityAC.periods.length){
      this.isFileUploadAllowed = true;
    }
    this.changeDetectorRef.detectChanges();
  }

  /**
   * Method is to download the file
   * @param tax TaxAC object
   */
  fileDownload(taxData) {
    if (taxData.taxes.entityTaxAccount.document.id && taxData.taxes.entityTaxAccount.document.downloadPath === null) {
      this.globalService.getDocument(taxData.taxes.entityTaxAccount.document.id).subscribe(
        (preSignedUrl: string) => {
          if (preSignedUrl != null) {
            window.open(preSignedUrl, '_blank');
          }
        }
      );
    } else {
      window.open(taxData.taxes.entityTaxAccount.document.downloadPath, '_blank');
    }
  }

  fileUpload(taxAC: TaxAC) {
    //Generate upload presigned URL
    this.globalService.getUploadPreSignedURL(this.fileName).subscribe(
      (res: AwsSettings) => {
        this.appService.showTaxTimer = true;
        this.appService.uploadFile(this.fileData, res.uploadPreSignedUrl)
          .subscribe(() => {
              taxAC.entityTaxAccount.document.path = res.baseUrl;
              taxAC.entityTaxAccount.document.downloadPath = res.preSignedUrl;
            this.appService.taxesData.filter(x => res.baseUrl.includes(x.taxes.entityTaxAccount.document.path))[0].isFileUploading = false;
            if (this.appService.taxesData.every(x => x.isFileUploading === false) && !this.router.url.includes('taxes')) {
                this.addTaxes();
            }
            this.appService.taxFileUploadingInProgress = false;
            this.changeDetectorRef.detectChanges();
          });
      }
    );
  }
  
  /**Add tax files */
  addTaxes() {
    this.appService.taxFileUploadingInProgress = false;
    this.entityAC.taxes = this.appService.taxes;
    this.entityService.addTaxForm(this.entityId, this.entityAC).subscribe(
      () => {
        for (const taxes of this.appService.taxesData) {
          taxes.isDbData = true;
        }
        if (this.currentSection !== Constant.taxes) {
          this.appService.taxes = [];
          this.appService.taxesData = [];
        }
      }, (err: ProblemDetails) => {
        this.toastrService.error(err.detail);
        this.appService.updateLoader(false);
        this.loader = false;
      }
    );
  }

  /**
   * Method is to add tax return year in the uploaded file list
   * @param period Tax return year
   * @param i index of file
   */
  addYearOfDocument(period: string, i: number) {
    this.appService.taxes[i].entityTaxAccount.period = period;
    let tempYears = this.entityAC.periods;
    for (const tax of this.appService.taxes) {
      if (tempYears.includes(tax.entityTaxAccount.period)) {
        tempYears = tempYears.filter(x => x !== tax.entityTaxAccount.period);
      }
    }
    this.years = tempYears;
    this.changeDetectorRef.detectChanges();
  }

  /**
   * Method is to save the uploaded tax return files for the entity
   */
  async saveTaxReturnInfo() {
    this.loader = true;
    if (this.isDocumentsAlreadySaved() && this.deletedTaxes.length === 0) {
      this.updateSectionAndNavigate();
    } else {
      if(!await this.checkValidationForUploadedFiles()){
        this.appService.updateLoader(false);
        this.loader = false;
        return;
      } else {
        if (!this.appService.taxFileUploadingInProgress) {
          this.addTaxes();
        }
        this.updateSectionAndNavigate();
      }
    }
  }

  /**Update section if current section taxes else navigate to additional document. */
  updateSectionAndNavigate() {
    if (this.currentSection === Constant.taxes) {
      this.updateSectionName();
    } else {
      this.router.navigate([Constant.additionalDocumentsRedirectUrl]);
    }
  }

  /**
   * Method is used for validating whether all the required files are uploaded or not
   */
  async checkValidationForUploadedFiles() {
    const totalTaxFormRequireToUploadCount = this.years.length;
    const uploadedTaxFormCount = this.appService.taxes.filter(x => this.years.filter(z => z === x.entityTaxAccount.period)).length;
    const minimumNumberOfYearlyTaxForm = (await this.appService.getAppSettings()).filter(x => x.fieldName === 'TaxConfig:MinimumNumberOfYearlyTaxForm')[0].value;
    
    if (minimumNumberOfYearlyTaxForm === 'All' && uploadedTaxFormCount !== totalTaxFormRequireToUploadCount) {
      this.toastrService.error(Constant.allTaxFormValidationError);
      return false;
    } else if (minimumNumberOfYearlyTaxForm === 'AnyOne' && uploadedTaxFormCount === 0) {
      this.toastrService.error(Constant.atleastOneTaxFormValidationError);
      return false;
    } else {
      // Validation for checking does uploaded document attached with tax year
      const documentWithEmptyPeriodCount = this.appService.taxes.filter(x => x.entityTaxAccount.period === undefined).length;
      if (documentWithEmptyPeriodCount > 0) {
        this.toastrService.error(Constant.documentPeriodEmpty);
        return false;
      }
      return true;
    }
  }

  /**
   * Method is to check whether new file is uploaded or not
   */
  isDocumentsAlreadySaved(){
    return (this.appService.taxes.findIndex(tax=> tax.id === undefined || tax.id === null) === -1);
  }

  /**
  * Method is used to fetch the previously uploaded the tax forms of the entity
  */ 
  initializeTaxReturnInfo(type: ResourceType) {
    if (type === ResourceType.Company) {
      this.entityService.getTaxListByEntityId(this.entityId).subscribe(
        (entityACResponse: EntityAC) => {
          if (this.currentSection === Constant.taxes && entityACResponse.taxes.length !== 0) {
            this.entityAC = new EntityAC();
            this.entityAC.periods = entityACResponse.periods;
            this.taxesRetrievalDate = entityACResponse.taxes[0].creationDateTime;
            this.years = entityACResponse.periods;
            this.entityData = entityACResponse;
            this.hasTaxesAlready = true;
          } else {
            this.entityAC = entityACResponse;
            this.hasTaxesAlready = false;
            this.fillTaxes();
          }
        }, (err: ProblemDetails) => {
          this.handleErrorInTaxSubscription(err);
        }
      );
    } else {
      this.applicationService.getTaxListByApplicationId(this.applicationId).subscribe(
        (entityACResponse: EntityAC) => {
          this.entityAC = entityACResponse;
          this.fillTaxes();
        }, (err: ProblemDetails) => {
          this.handleErrorInTaxSubscription(err);
        }
      );
    }
    
  }

  // method to handle error in tax subscription
  handleErrorInTaxSubscription(err: ProblemDetails) {
    if (err.status === Constant.badRequest) {
      this.toastrService.error(err.detail);
      this.loader = false;
    }
  }

  /** Fill up fetched taxes information */
  fillTaxes() {
    this.years = this.entityAC.periods;
    if (!this.appService.taxFileUploadingInProgress) {
      this.appService.taxesData = [];
    }
    if (this.entityAC.taxes.length > 0 && (this.appService.taxesData.length === 0)){
      for (const tax of this.entityAC.taxes) {
        this.appService.taxesData.push({ taxes: tax, isDbData: true, isFileUploading: false });
        this.removeYearFromDropdown(tax.entityTaxAccount.period);
      }
      this.appService.taxes = this.entityAC.taxes;
      if (this.appService.taxesData.length === this.entityAC.periods.length) {
        this.isFileUploadAllowed = false;
      }
    } else {
      for (const taxesData of this.appService.taxesData) {
        this.removeYearFromDropdown(taxesData.taxes.entityTaxAccount.period);
      }
    }
    if (this.appService.taxesData.length === this.entityAC.periods.length) {
      this.isFileUploadAllowed = false;
    }
    this.loader = false;
  }

  // Handler of interstial page response
  existingTaxesAction(reuseExistingTaxes: boolean) {
    this.hasTaxesAlready = false;
    if (!reuseExistingTaxes) {
      this.loader = false;
    } else {
      this.entityAC = this.entityData;
      this.fillTaxes();
    }
  }

  /**
   * Method is used to update the section name
   */
  async updateSectionName() {
    const loanApplicationId = await this.appService.getCurrentLoanApplicationId();
    this.applicationService.updateCurrentSectionName(loanApplicationId, this.currentSection).subscribe(
      async (updatedSectionName: string) => {
        if (updatedSectionName !== null) {
          await this.appService.updateCurrentSectionName(updatedSectionName);
          await this.appService.updateProgressbar(Constant.additionalDocumentsProgressBar);
          this.router.navigate([Constant.additionalDocumentsRedirectUrl]);
        }
      }, err => {
        this.loader = false;
        this.appService.updateLoader(false);
      }
    );
  }

  /**
   * Method is to remove the selected year from the drop down of year
   * @param period Year of uploaded document
   */
  removeYearFromDropdown(period: string){
    const targetYearIndex = this.years.indexOf(period);
    if(targetYearIndex !== -1){
      this.years = this.years.filter(x => x !== period);
    }
  }
}
