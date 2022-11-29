import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FileSystemFileEntry, NgxFileDropEntry } from 'ngx-file-drop';
import { ToastrService } from 'ngx-toastr';
import { AppService } from '../../services/app.service';
import { Constant } from '../../shared/constant';
import {
  AdditionalDocumentAC, AdditionalDocumentTypeAC, AwsSettings, DocumentAC,
  GlobalService, LoanApplicationStatusType, ProblemDetails, ResourceType,
  EntityService, ApplicationService, EntityAC
} from '../../utils/serviceNew';

@Component({
  selector: 'app-additional-documents',
  templateUrl: './additional-documents.component.html',
  styleUrls: ['./additional-documents.component.scss']
})

export class AdditionalDocumentsComponent implements OnInit {

  constructor(private readonly globalService: GlobalService,
    private readonly entityService: EntityService,
    private readonly applicationService: ApplicationService,
    private readonly router: Router,
    private readonly toastrService: ToastrService,
    readonly appService: AppService,
    private readonly changeDetectorRef: ChangeDetectorRef) {
    this.appService.updateLoader(false);
  }

  uploadAdditionalDocument = Constant.uploadAdditionalDocument;
  businessRelatedDocument = Constant.businessRelatedDocument;
  shareHolderDocument = Constant.shareHolderDocument;

  businessDocumentTypeList;
  shareholderDocumentTypeList;
  fileData;
  fileName;
  deletedAdditionalDocuments: AdditionalDocumentAC[] = [];
  entityAC: EntityAC;
  
  resourceType: typeof ResourceType = ResourceType;

  currentSection: string;
  entityId: string;
  applicationId: string;
  isViewOnlyMode = false;
  hasDocumentsAlready = false;

  nextRoute = Constant.loanConsentRedirectUrl;
  backRoute = Constant.taxesReturnsRedirectUrl;
  allowedExtensions = Constant.allowedFileTypesForAdditionalDocuments;
  allowedCommaSeparatedFileExtensions = Constant.allowedCommaSeparatedFileExtensionsForAdditionalDocuments;
  pdfIcon = '../../assets/images/pdf.svg';
  xlsIcon = '../../assets/images/xls.svg';
  pngIcon = '../../assets/images/png.svg';
  jpegIcon = '../../assets/images/jpg.svg';
  csvIcon = '../../assets/images/csv.svg';
  docxIcon = '../../assets/images/doc.svg';
  loader = true;

  //File types
  pdf = Constant.pdf;
  xls = Constant.xls;
  png = Constant.png;
  jpeg = Constant.jpeg;
  csv = Constant.csv;
  docx = Constant.docx;

  async ngOnInit() {
    if (!this.appService.additionalDocumentUploadingInProgress && this.appService.showAdditionalDocumentTimer) {
      this.appService.showAdditionalDocumentTimer = false;
    }

    this.currentSection = await this.appService.getCurrentSectionName();
    this.isViewOnlyMode = await this.appService.isViewOnlyMode();
    const currentStatus = LoanApplicationStatusType[await this.appService.getCurrentLoanApplicationStatus()];
    this.entityId = await this.appService.getCurrentCompanyId();
    this.applicationId = await this.appService.getCurrentLoanApplicationId();

    //Get additional documents' types (seeded data)
    this.globalService.getAdditionalDocumentTypes().subscribe(
      documentTypes => {
        this.businessDocumentTypeList = documentTypes.filter(x => x.documentTypeFor === ResourceType.Company);
        this.shareholderDocumentTypeList = documentTypes.filter(x => x.documentTypeFor === ResourceType.User);

        //If current status of loan is draft then fetch documents via application id else fetch via entity id
        if (currentStatus && LoanApplicationStatusType[currentStatus] !== LoanApplicationStatusType.Draft) {
          this.initializeAdditionalDocumentsList(ResourceType.Loan);
        } else {
          if (!this.entityId) {
            this.router.navigate([Constant.routeRootUrl]);
          } else {
            this.initializeAdditionalDocumentsList(ResourceType.Company);
          }
        }
      },
      (err: ProblemDetails) => {
        this.handleErrorInSubscription(err);
      });
  }

  /**
   * Method is to upload file to temp folder in S3 bucket when file is dropped 
   * @param droppedFiles NgxFileDropEntry array
   * @param type Resource type
   */
  dropped(droppedFiles: NgxFileDropEntry[], type: ResourceType) {

    for (const droppedFile of droppedFiles) {
      if (droppedFile.fileEntry.isFile && this.isFileAllowed(droppedFile.fileEntry.name)) {

        const fileEntry = droppedFile.fileEntry as FileSystemFileEntry;
        fileEntry.file((file: File) => {
          //Check if given file size is allowed or not
          if (!this.isFileSizeAllowed(file.size)) {
            this.toastrService.error(Constant.fileSizeExceeded);
            return;
          }

          this.fileData = file;
          this.fileName = file.name;
          const additionalDocumentAC = new AdditionalDocumentAC();
          additionalDocumentAC.document = new DocumentAC();
          additionalDocumentAC.document.name = this.fileName;

          //Get the file extension to set the file type
          const fileType = file.name.split('.')[1];

          //Based on the types, fill data in separate array
          if (type === ResourceType.Company) {
            this.appService.businessRelatedDocumentsData.push({ document: additionalDocumentAC, isDbData: false, isFileUploading: true, fileType });
          } else {
            this.appService.shareholderRelatedDocumentsData.push({ document: additionalDocumentAC, isDbData: false, isFileUploading: true, fileType });
          }
          this.appService.additionalDocumentUploadingInProgress = true;
          this.fileUpload(additionalDocumentAC, type);
        });
      } else {
        this.toastrService.error(Constant.fileNotSupportedInAdditionalDocuments);
      }
    }
  }

  /**
   * Method is to verify that file extension is allowed or not
   * @param fileName File name
   */
  isFileAllowed(fileName: string) {
    let isFileAllowed = false;
    const allowedFileExtensions = ['.pdf', '.xls', '.png', '.jpeg', '.csv', '.docx'];
    const regex = /(?:\.([^.]+))?$/;
    const extension = regex.exec(fileName);
    if (extension !== undefined && extension !== null && allowedFileExtensions.includes(extension[0])) {
      isFileAllowed = true;
    }
    return isFileAllowed;
  }

  /**
   * Method to verify that file size is in allowed range
   * @param fileSize File size
   */
  isFileSizeAllowed(fileSize: number) {
    const maxFileSize = 20971520;
    if (fileSize > maxFileSize) {
      return false;
    }
    return true;
  }

  /**
   * Method to download the file
   * @param additionalDocument AdditionalDocumentAC object
   */
  fileDownload(additionalDocument: AdditionalDocumentAC) {
    //If download path is null then get it else open that file on new window to download
    if (additionalDocument.document.id && additionalDocument.document.downloadPath === null) {
      this.globalService.getDocument(additionalDocument.document.id).subscribe(
        (preSignedUrl: string) => {
          if (preSignedUrl != null) {
            window.open(preSignedUrl, '_blank');
          }
        }
      );
    } else {
      window.open(additionalDocument.document.downloadPath, '_blank');
    }
  }

  /**
   * Method to upload the file
   * @param additionalDocument AdditionalDocumentAC object
   * @param type Resource type
   */
  fileUpload(additionalDocument: AdditionalDocumentAC, type: ResourceType) {
    //Generate upload presigned URL
    this.globalService.getUploadPreSignedURL(this.fileName).subscribe(
      (res: AwsSettings) => {
        this.appService.showAdditionalDocumentTimer = true;
        this.appService.uploadFile(this.fileData, res.uploadPreSignedUrl).subscribe(
          () => {
            additionalDocument.document.path = res.baseUrl;
            additionalDocument.document.downloadPath = res.preSignedUrl;

            if (type === ResourceType.Company) {
              this.appService.businessRelatedDocumentsData.filter(x => res.baseUrl.includes(x.document.document.path))[0].isFileUploading = false;
              if (this.appService.businessRelatedDocumentsData.every(x => x.isFileUploading === false) && !this.router.url.includes('additional-document')) {
                this.addAdditionalDocuments();
              }
            } else {
              this.appService.shareholderRelatedDocumentsData.filter(x => res.baseUrl.includes(x.document.document.path))[0].isFileUploading = false;
              if (this.appService.shareholderRelatedDocumentsData.every(x => x.isFileUploading === false) && !this.router.url.includes('additional-document')) {
                this.addAdditionalDocuments();
              }
            }
            this.appService.additionalDocumentUploadingInProgress = false;
            this.changeDetectorRef.detectChanges();
          });

      }
    );
  }

  /**
   * Method to remove the file from the list
   * @param fileIndex Index of the document which is to be removed
   * @param additionalDocument Document object to remove
   */
  fileRemove(fileIndex: number, type: ResourceType) {
    //Based on the resource type remove the file from relevant list
    if (type === ResourceType.Company) {
      //Additional document to remove
      this.deletedAdditionalDocuments.push(this.appService.businessRelatedDocumentsData[fileIndex].document);
      this.appService.businessRelatedDocumentsData.splice(fileIndex, 1);
    } else {
      //Additional document to remove
      this.deletedAdditionalDocuments.push(this.appService.shareholderRelatedDocumentsData[fileIndex].document);
      this.appService.shareholderRelatedDocumentsData.splice(fileIndex, 1);
    }
    this.changeDetectorRef.detectChanges();
  }

  /**
   * Method to set document type for the uploaded file
   * @param event Event of ngSelect
   * @param additionalDocument AdditionalDocumentAC object
   */
  selectDocumentType(event: AdditionalDocumentTypeAC, type: ResourceType, fileIndex: number) {
    if (type === ResourceType.Company && this.appService.businessRelatedDocumentsData[fileIndex]) {
      this.appService.businessRelatedDocumentsData[fileIndex].document.documentType = event;
    } else if (type === ResourceType.User && this.appService.shareholderRelatedDocumentsData[fileIndex]) {
      this.appService.shareholderRelatedDocumentsData[fileIndex].document.documentType = event;
    }
  }

  /**
   * Method to fetch and initialize document list for the existing data, if any
   * @param type Type of resource
   */
  initializeAdditionalDocumentsList(type: ResourceType) {
    if (type === ResourceType.Company) {
      this.entityService.getAdditionalDocumentsForEntity(this.entityId).subscribe(
        entityAC => {
          this.entityAC = entityAC;
          if (entityAC.additionalDocuments && entityAC.additionalDocuments.length !== 0) {
            this.fillAdditionalDocumentList();
          } else {
            this.loader = false;
          }
        }, (err: ProblemDetails) => {
          this.handleErrorInSubscription(err);
        }
      );
    } else {
      this.applicationService.getAdditionalDocumentsForApplication(this.applicationId).subscribe(
        entityAC => {
          this.entityAC = entityAC;
          this.fillAdditionalDocumentList();
        }, (err: ProblemDetails) => {
          this.handleErrorInSubscription(err);
        }
      );
    }
  }

  /**
   * Method to handle error in subscription
   * @param err Error
   */
  handleErrorInSubscription(err: ProblemDetails) {
    if (err.status === Constant.badRequest) {
      this.toastrService.error(err.detail);
      this.loader = false;
    }
  }

  /**
   * Fill up fetched additional document data
   * */
  fillAdditionalDocumentList() {
    if (!this.appService.additionalDocumentUploadingInProgress) {
      this.appService.businessRelatedDocumentsData = [];
      this.appService.shareholderRelatedDocumentsData = [];
    }

    //If object has additional documents then only fill them in the list
    if (this.entityAC.additionalDocuments && this.entityAC.additionalDocuments.length > 0 &&
      this.appService.businessRelatedDocumentsData.length === 0 && this.appService.shareholderRelatedDocumentsData.length === 0) {
      for (const additionalDocument of this.entityAC.additionalDocuments.filter(x => x.documentType.documentTypeFor === ResourceType.Company)) {
        //Get the file extension to set the file type
        const fileType = additionalDocument.document.name.split('.')[1];
        this.appService.businessRelatedDocumentsData.push({ document: additionalDocument, isDbData: true, isFileUploading: false, fileType });
      }
      for (const additionalDocument of this.entityAC.additionalDocuments.filter(x => x.documentType.documentTypeFor === ResourceType.User)) {
        //Get the file extension to set the file type
        const fileType = additionalDocument.document.name.split('.')[1];
        this.appService.shareholderRelatedDocumentsData.push({ document: additionalDocument, isDbData: true, isFileUploading: false, fileType });
      }
    }
    this.loader = false;
  }

  /**
   * Method is to save the uploaded tax return files for the entity
   */
  async saveAdditionalDocuments() {
    this.loader = true;

    //If all documents are saved already then no call is to be made for addition
    if (this.areAllDocumentsAlreadySaved() && this.deletedAdditionalDocuments.length === 0) {
      this.updateSectionAndNavigate();
    } else {
      //If all the uploaded files are valid then only save them
      if (!this.checkRequiredValidationsForUploadedFiles()) {
        this.appService.updateLoader(false);
        this.loader = false;
        return;
      } else {
        if (!this.appService.additionalDocumentUploadingInProgress) {
          this.addAdditionalDocuments();
        }
        this.updateSectionAndNavigate();
      }
    }
  }

  /** 
   * Add documents in DB
   * */
  addAdditionalDocuments() {
    this.entityAC.additionalDocuments = this.mapAllDocumentsToRequestObject();
    this.entityService.saveAdditionalDocumentsForEntity(this.entityId, this.entityAC).subscribe(
      async data => {
        if (this.appService.businessRelatedDocumentsData && this.appService.businessRelatedDocumentsData.length > 0) {
          for (const document of this.appService.businessRelatedDocumentsData) {
            document.isDbData = true;
          }
        }
        if (this.appService.shareholderRelatedDocumentsData && this.appService.shareholderRelatedDocumentsData.length > 0) {
          for (const document of this.appService.shareholderRelatedDocumentsData) {
            document.isDbData = true;
          }
        }
        if (this.currentSection !== Constant.additionalDocuments) {
          this.appService.businessRelatedDocumentsData = [];
          this.appService.shareholderRelatedDocumentsData = [];
        }
        this.appService.additionalDocumentUploadingInProgress = false;
      }, (err: ProblemDetails) => {
        this.toastrService.error(err.detail);
        this.appService.updateLoader(false);
        this.appService.additionalDocumentUploadingInProgress = false;
        this.loader = false;
      });
  }

  /**
   * Update section name if current section is Additional Documents else navigate to consent section.
   * */
  updateSectionAndNavigate() {
    if (this.currentSection === Constant.additionalDocuments) {
      this.updateSectionName();
    } else {
      this.router.navigate([Constant.loanConsentRedirectUrl]);
    }
  }

  /**
   * Method to map documents of both resource type to request object
   * */
  mapAllDocumentsToRequestObject(): AdditionalDocumentAC[] {
    const additionalDocuments = [];
    this.appService.shareholderRelatedDocumentsData.forEach(x => additionalDocuments.push(x.document));
    this.appService.businessRelatedDocumentsData.forEach(x => additionalDocuments.push(x.document));
    return additionalDocuments;
  }

  /**
   * Method to check whether all files are already saved or any new file has been uploaded
   * */
  areAllDocumentsAlreadySaved() {
    return ((this.appService.shareholderRelatedDocumentsData.findIndex(document => document.document.id === undefined || document.document.id === null) === -1)
      && (this.appService.businessRelatedDocumentsData.findIndex(document => document.document.id === undefined || document.document.id === null) === -1));
  }

  /**
   * Method to update the section
   */
  async updateSectionName() {
    const loanApplicationId = await this.appService.getCurrentLoanApplicationId();
    this.applicationService.updateCurrentSectionName(loanApplicationId, this.currentSection).subscribe(
      async (updatedSectionName: string) => {
        if (updatedSectionName !== null) {
          await this.appService.updateCurrentSectionName(updatedSectionName);
          await this.appService.updateProgressbar(Constant.loanConsentProgressBar);
          this.router.navigate([Constant.loanConsentRedirectUrl]);
        }
      }, err => {
        this.loader = false;
        this.appService.updateLoader(false);
      }
    );
  }

  /**
   * Method to check whether all the documents have been assigned the type or not
   */
  checkRequiredValidationsForUploadedFiles() {
    //Validation for the types of uploaded documents
    const documentsWithoutTypeAssigned = this.appService.shareholderRelatedDocumentsData.filter(x => x.document.documentType === undefined).length +
      this.appService.businessRelatedDocumentsData.filter(x => x.document.documentType === undefined).length;
    if (documentsWithoutTypeAssigned > 0) {
      this.toastrService.error(Constant.documentTypeNotSelected);
      return false;
    }
    return true;
  }
}
