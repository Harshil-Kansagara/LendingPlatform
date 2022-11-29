import { Component, OnInit, Input, SimpleChanges } from '@angular/core';
import { AppService } from '../../../../../services/app.service';
import { FileParameter, CompanyInfoService } from '../../../../../utils/service';
import { ToastrService } from 'ngx-toastr';
import { Constant } from '../../../../../shared/constant';
import { Router } from '@angular/router';

@Component({
  selector: '[app-upload-file]',
  templateUrl: './upload-file.component.html'
})
export class UploadFileComponent implements OnInit {

  constructor(private readonly appService: AppService,
    private readonly companyInfoService: CompanyInfoService,
    private readonly toastrService: ToastrService,private readonly router:Router) { }

  /* File Upload Methods */
  @Input() taxArray;
  @Input() showDeleteButton;
  @Input() title;
  @Input() statementId = 1;
  @Input() showUploadButton = true;
  @Input() noFileslabel = false;
  file: FileParameter = {} as FileParameter;
  uploadFileLoaders: Array<boolean>;

  /**
   *  Angular on change event. It hits when input changes.
   * @param changes it contain what input changes happen.  
   */
  ngOnChanges(changes: SimpleChanges) {
    if (changes.taxArray && !this.uploadFileLoaders) {
      this.uploadFileLoaders = new Array<boolean>(this.taxArray[0].entityFinanceYearlyMappings.length);
    }
  }
  /**
   * Method to upload files on aws.
   * @param event Files to upload.
   * @param entityFinanceYearMapping EntityFinanceYearlyMappingAC object.
   */
  async uploadFile(event, entityFinanceYearMapping, index: number) {
    this.uploadFileLoaders[index] = true;
    this.file.data = event.target.files[0];
    this.file.fileName = event.target.files[0].name;
    entityFinanceYearMapping.documentName = this.file.fileName;
    await this.companyInfoService.uploadDocument(this.file).subscribe(async res => {
      this.uploadFileLoaders[index] = false;
      entityFinanceYearMapping.uploadedDocumentPath = res.baseUrl;
      entityFinanceYearMapping.documentPathToDownload = res.preSignedUrl;
    },
      err => {
        // Remove the current uploaded file details.
        this.clear(entityFinanceYearMapping);
        this.uploadFileLoaders[index] = false;
        this.toastrService.error(err.response);
      });
  }

  /**
   * Method to clear input of file section.
   * @param entityFinanceYearMapping EntityFinanceYearlyMappingAC object.
   */
  clear(entityFinanceYearMapping) {
    entityFinanceYearMapping.documentName = null;
    entityFinanceYearMapping.uploadedDocumentPath = null;
    entityFinanceYearMapping.documentPathToDownload = null;
  }

  pdfIcon = '../../assets/images/pdf-icon.svg';
  forAllFormatIcon = '../../assets/images/forallformat.png';
  ngOnInit(): void {

  }

  /**
   * Method to download files.
   * @param entityFinanceYearMapping EntityFinanceYearlyMappingAC object.
   */
  async downloadFile(entityFinanceYearMapping) {
    if (!this.router.url.includes(Constant.loanConsentRedirectUrl)) {
      this.appService.updateLoader(true);
    }
    if (entityFinanceYearMapping.uploadedDocumentId && entityFinanceYearMapping.documentPathToDownload === null) {
      this.companyInfoService.getDocument(entityFinanceYearMapping.uploadedDocumentId).subscribe(async (res) => {
        entityFinanceYearMapping.documentPathToDownload = res;
        this.appService.updateLoader(false);
        window.open(entityFinanceYearMapping.documentPathToDownload, '_blank');
      }, err => {
        this.appService.updateLoader(false);
        this.toastrService.error(err.response);
      });
    } else {
      window.open(entityFinanceYearMapping.documentPathToDownload, '_blank');
    }
  }
}
