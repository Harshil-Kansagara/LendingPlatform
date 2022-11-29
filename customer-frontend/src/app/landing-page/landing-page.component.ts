import { Component, OnDestroy, ElementRef, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { CarouselConfig } from 'ngx-bootstrap/carousel';
import { Constant } from '../shared/constant';
import { AppService } from '../services/app.service';
import { ToastrService } from 'ngx-toastr';
import {
  GlobalService, EntityService, EntityAC, ApplicationBasicDetailAC, FilterAC,
  CompanyAC, LoanApplicationStatusType, ApplicationService, SectionAC
} from '../utils/serviceNew';
import { Router, ActivatedRoute } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Subscription } from 'rxjs';
import { OwlOptions } from 'ngx-owl-carousel-o';
import { BsModalRef, ModalDirective, BsModalService } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.scss'],
  providers: [
    { provide: CarouselConfig, useValue: { noPause: true, showIndicators: false } }
  ]
})

export class LandingPageComponent implements OnInit, OnDestroy {


 
  constructor(private readonly globalService: GlobalService,
    private readonly appService: AppService,
    private readonly entityService: EntityService,
    private readonly applicationService: ApplicationService,
    private readonly route: ActivatedRoute,
    private readonly toastrService: ToastrService,
    private readonly router: Router,
    private readonly oidcSecurityService: OidcSecurityService,
    private readonly modalService: BsModalService) { }

  modalRef: BsModalRef;
  @ViewChild(ModalDirective, { static: false }) modal: ModalDirective;
  @ViewChild('template') template: TemplateRef<ElementRef>;
  // Loan process steps
  processes = [
    { title: Constant.request, description: Constant.requestInfo },
    { title: Constant.approval, description: Constant.approvalInfo },
    { title: Constant.money, description: Constant.moneyInfo }
  ];
  //Loan process short info
  processSummary = Constant.processSummary;
  // Loanslider(Owl slider) options
  loanSliderOptions: OwlOptions = {
    loop: false,
    mouseDrag: false,
    touchDrag: false,
    pullDrag: false,
    dots: false,
    navSpeed: 700,
    navText: ['<i class="zmdi zmdi-arrow-left"></i>', '<i class="zmdi zmdi-arrow-right"></i>'],
    responsive: {
      0: {
        items: 1
      }
    },
    nav: true
  };
  statusList = [];
  isSectionFound;
  isSectionAlreadyFound;
  isLoanAlreadyInProgress = false;
  loanAlreadyInProgress;
  personalSection = Constant.personal;
  lendingFeatures = Constant.lendingFeatures;
  loanStatusDraftTitle = Constant.loanStatusDraftTitle;
  loanStatusApprovedTitle = Constant.loanStatusApprovedTitle;
  loanStatusRejectedTitle = Constant.loanStatusRejectedTitle;
  loanStatusLockedTitle = Constant.loanStatusLockedTitle;
  evaluationResultMessage = Constant.ifloanEvaluationLandingPage;
  information = Constant.information;
  infoDesk = Constant.infoDesk;
  continueApply = Constant.continueApply;
  applyNewLoan = Constant.applyNewLoan;
  isLoggedIn = false;
  calculatorRedirectUrl = Constant.calculatorRedirectUrl;
  currentUserId: string;
  companyList: EntityAC[] = [];
  selectedCompany: string;
  loanApplicationList: ApplicationBasicDetailAC[] = [];
  loader = true;
  loanSections = [];
  isRendered = false;
  subsVar: Subscription;
  loanApplicationStatusEnum = LoanApplicationStatusType;
  

  async ngOnInit() {

    this.appService.showTaxTimer = false;
    this.appService.taxesData = [];
    this.appService.taxes = [];
    this.appService.showAdditionalDocumentTimer = false;
    this.appService.businessRelatedDocumentsData = [];
    this.appService.shareholderRelatedDocumentsData = [];
    this.appService.removeLoanProductDetail();
    // Fetch and save all the configurations in localForage.
    this.globalService.getConfigurations().subscribe(
      configurations => {
        this.appService.setConfigurations(configurations);
        const token = localStorage.getItem('access_token');
        this.route.queryParams.subscribe(params => {
          const code = params['code'];

          // When there is no redirection from identity server
          if (!code) {
            this.appService.isAuthenticated().subscribe(async isAuthorized => {
              if (isAuthorized && token) {
                
                this.isLoggedIn = true;

                if (!await this.appService.getCurrentUserDetailsNew()) {

                  const filterAC = new FilterAC();
                  filterAC.field = 'type';
                  filterAC.operator = '=';
                  filterAC.value = 'people';
                  const filters = new Array<FilterAC>();
                  filters.push(filterAC);

                  this.entityService.getEntityList(null, null, null, JSON.stringify(filters), null, null).subscribe(
                    list => {
                      this.appService.setCurrentUserName(`${list[0].user.firstName} ${list[0].user.lastName}`);
                      this.currentUserId = list[0].id;
                      this.getAllCompaniesOfUser();
                    },);
                } else {
                  this.getAllCompaniesOfUser();
                }
              } else {
                await this.appService.resetCurrentLocalForage();
                this.isRendered = true;
              }
            });
          }
        });
      });
   
  }

  /**
   * Method to fetch all the linked entities of user.
   * */
  async getAllCompaniesOfUser() {
    const currentUserDetails = await this.appService.getCurrentUserDetailsNew();
    if (currentUserDetails !== null) {
      // Details to add self in company list dropdown
      const temp = new EntityAC();
      temp.id = currentUserDetails.id;
      this.currentUserId = temp.id;
      temp.company = new CompanyAC();
      temp.company.name = Constant.self;

      const filterAC = new FilterAC();
      filterAC.field = 'type';
      filterAC.operator = '=';
      filterAC.value = 'company';
      const filters = new Array<FilterAC>();
      filters.push(filterAC);

      this.entityService.getEntityList(null, null, null, JSON.stringify(filters), null, null).subscribe(
        res => {
          this.setList(res, temp);
        });
    }
  }

  /**
   * Method to set the entity list along with adding SELF in the list.
   * @param entityList List of entities
   * @param temp current user as an entity
   */
  setList(entityList: EntityAC[], temp: EntityAC) {
    
    if (entityList.length !== 0 && temp !== null) {
      this.companyList = entityList;
      this.appService.setCurrentCompanyDetailsNew(this.companyList[0]);
      this.appService.setCurrentCompanyId(this.companyList[0].id);
      this.selectedCompany = this.companyList[0].company.name;
      this.companyList.push(temp);
    } else if (entityList.length === 0 && temp !== null) {
      this.appService.setCurrentCompanyDetailsNew(temp);
      this.appService.setCurrentCompanyId(temp.id);
      this.selectedCompany = temp.company.name;
      this.companyList.push(temp);
    } else {
      this.toastrService.error(Constant.someThingWentWrong);
    }

    if (this.companyList.length !== 0) {
      this.entityService.getLoanApplicationListByEntityId(this.companyList[0].id).subscribe(
        res => {
          this.loanApplicationList = res.length !== 0 ? res : [];
          if (this.loanApplicationList.length !== 0) {
            this.appService.updateEntityList(this.companyList);
            this.setStatusArray(this.loanApplicationList);
          } else {
            this.handleErrorInSetListMethod();
          }
          this.appService.updateSelectedEntity({ id: null, name: null });
          this.subsVar=this.appService.selectedEntityEvent.subscribe(val => this.selectCompany(val));
        },
        () => {
          this.handleErrorInSetListMethod();
        });
    } else {
      this.handleErrorInSetListMethod();
    }
  }

  /**
   * Method to execute the code for error and else part in setList method.
   * */
  handleErrorInSetListMethod() {
    this.appService.updateEntityList(this.companyList);
    this.loader = false;
    this.isRendered = true;
  }

  /**
   * Set section when user is sholder and at personal finances section.
   * @param section
   * @param sectionName
   */
  setSectionsForShareholder(section: SectionAC, sectionName: string) {
    if ((section.name === sectionName || section.name === this.isSectionFound || section.name === Constant.loanConsent)) {
      this.statusList.push({ name: section.name, status: Constant.processing });
    } else {
      this.statusList.push({ name: section.name, status: Constant.approved });
    }
  }

  /**
   * Set all sections status for loan.
   * @param section
   * @param sectionName
   * @param isFound
   */
  setSections(section: SectionAC, sectionName: string) {
    if ((section.name === sectionName || section.name === this.isSectionFound)) {
      this.statusList.push({ name: section.name, status: Constant.processing });
      this.isSectionAlreadyFound = true;
    } else if (!this.isSectionAlreadyFound) {
      this.statusList.push({ name: section.name, status: Constant.approved });
    } else {
      this.statusList.push({ name: section.name, status: Constant.pending });
    }
  }

  /**
   * Set the status for all loans in list.
   * @param list List of loan
   */
  async setStatusArray(list: ApplicationBasicDetailAC[]) {
    this.loanSections = [];
    for (const i of list) {
      this.statusList = [];
      this.isSectionAlreadyFound = false;
      let temp = await this.appService.getSectionConfigurations();
      temp = temp.filter(x => x.isEnabled);
      temp = temp.filter(x => x.name !== Constant.loanStatus && x.name !== Constant.bankDetails);
      temp.sort(this.appService.sortByAnyIntegerField);
      //If selected section is any child then point to the parent section
      const parents = temp.filter(x => x.childSection && x.childSection.length > 0);
      this.isSectionFound = this.checkIfLoanIsAtSubSection(parents, i);
      if (i.createdByUserId !== this.currentUserId && i.sectionName === this.personalSection) {
        for (const j of temp) {
          this.setSectionsForShareholder(j, i.sectionName);
        }
      } else {
        for (const j of temp) {
          this.setSections(j, i.sectionName);
        }
      }
      this.loanSections.push(this.statusList);
    }
    this.loader = false;
    this.isRendered = true;
  }

  // Method that checks if loan is at subsection
  checkIfLoanIsAtSubSection(parents: SectionAC[], i: ApplicationBasicDetailAC):string {
    if (parents && parents.length > 0) {
      for (const parent of parents) {
        for (const child of parent.childSection) {
          if (i.sectionName === child.name) {
            return parent.name;
          }
        }
      }
    }
    return null;
  }

  /**
   * Method will be called on company change in dropdown ans set all the loan of that company.
   * @param event Event of ngSelect
   */
  selectCompany(event: { id: string, name: string }): void {
    if (event && event.id) {
      
      const companyId = event.id;
      this.loader = true;
      this.entityService.getLoanApplicationListByEntityId(companyId).subscribe(
        async res => {
          if (companyId === this.currentUserId) {
            await this.appService.removeCurrentCompanyDetails();
            await this.appService.removeCurrentCompanyId();
          } else {
            await this.appService.setCurrentCompanyDetailsNew(this.companyList.filter(x => x.id === companyId)[0]);
            await this.appService.setCurrentCompanyId(this.companyList.filter(x => x.id === companyId)[0].id);
          }
          this.loanApplicationList = companyId === this.currentUserId ? res.filter(x => x.mappedEntityId === Constant.guidEmptyString) : res;
          this.setStatusArray(this.loanApplicationList);
        });
    }
  }

  /**
   * Continue loan application.
   * */
  async continueLoanAsync() {
    this.modalRef.hide();
    const currentUser = (await this.appService.getCurrentUserDetailsNew()).user;
    const companyId = await this.appService.getCurrentCompanyId();
    const companyDetails = await this.appService.getCurrentCompanyDetailsNew();
    if (this.isLoanAlreadyInProgress) {
      await this.appService.removeCurrentCompanyId();
      await this.appService.removeCurrentCompanyDetails();
    } else {
      await this.appService.resetCurrentLocalForage();
      await this.appService.setCurrentCompanyId(companyId);
      await this.appService.setCurrentCompanyDetailsNew(companyDetails);
    }
      if (currentUser.selfDeclaredCreditScore === null) {
        this.router.navigate([Constant.creditProfileRedirectUrl]);
        //If no loan present and credit profile values are set then redirect to loan needs section to start new loan.
      } else {
        await this.appService.setViewOnlyMode(false);
        this.router.navigate([Constant.loanNeedsRedirectUrl]);
      }
    
  }

  /**
   * Open modal according to whether already any loan is in progress.
   * @param isLoanAlreadyInProgress
   * @param template
   */
  openModal(isAllowedToCreateNewLoan: boolean, template: TemplateRef<ElementRef>) {
    if (isAllowedToCreateNewLoan) {
      this.isLoanAlreadyInProgress = false;
      this.modalRef = this.modalService.show(template, Object.assign({}, { class: 'modal-dialog-centered modal-sm' }));
    } else {
      this.isLoanAlreadyInProgress = true;
      this.loanAlreadyInProgress = `Another loan for selected company ${this.selectedCompany} is already in progress by another user.`;
      this.modalRef = this.modalService.show(template, Object.assign({}, { class: 'modal-dialog-centered modal-vsm' }));
    }
  }

  /**
   * Method to apply for a new loan.
   * @param template
   * */
  async applyLoanAsync(template: TemplateRef<ElementRef>) {
    const token = localStorage.getItem('access_token');
    this.appService.isAuthenticated().subscribe(async isAuthorized => {
      //If user is not authenticated then redirect to login page.
      if (isAuthorized && token) {

        //If any loan is present then redirect to the latest section of it.
        const loanList = this.loanApplicationList.filter(x => x.status === LoanApplicationStatusType.Draft);
        if (loanList.length !== 0) {
          loanList.sort((a, b) => (a.lastUpdatedOn <= b.lastUpdatedOn) ? -1 : 1);
          this.openLoanApplication(loanList[0]);
          //If credit profile values are not set then redirect to credit profile page.
        }else {
          const companyId = await this.appService.getCurrentCompanyId();
          if (companyId && companyId !== this.currentUserId) {
            this.entityService.checkEntityAllowToStartNewApplication(companyId).subscribe(
              isAllowedToCreateNewLoan => {
                this.openModal(isAllowedToCreateNewLoan, template);
              });
          } else {
            this.isLoanAlreadyInProgress = false;
            this.modalRef = this.modalService.show(template, Object.assign({}, { class: 'modal-dialog-centered modal-sm' }));
          }
        }
      } else {
        this.oidcSecurityService.authorize();
      }
    });
  }

  /**
   * Method called when any application will be opened from landing page.
   * @param application
   */
  openLoanApplication(application: ApplicationBasicDetailAC) {
    
    if (LoanApplicationStatusType[application.status] !== LoanApplicationStatusType[LoanApplicationStatusType.Draft]) {
      this.applicationService.getLoanApplicationDetailsById(application.id).subscribe(
        async res => {
          await this.appService.setLockedApplicationAsJsonString(res);
          this.appService.openLoanApplication(application,true);
        });
    } else {
      this.appService.openLoanApplication(application,true);
    }
  }

  /**
   * Method to redirect the user to calculator page.
   * */
  async openCalculator() {
    //Set that the call is being made to open calculator page.
    await this.appService.setOpenCalculator(true);

    //If user is authentic then check that the credit profile value is set or not.
    //According to that redirect it to calculator page or credit profile page
    const currentUser = await this.appService.getCurrentUserDetailsNew();
    if (currentUser === null || currentUser.user.selfDeclaredCreditScore === null) {
      this.router.navigate([Constant.creditProfileRedirectUrl]);
    } else {
      this.router.navigate([Constant.calculatorRedirectUrl]);
    }
  }

  ngOnDestroy() {
    if (this.subsVar) {
      this.subsVar.unsubscribe();
    }
  }
}
