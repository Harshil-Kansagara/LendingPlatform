import { Component, OnInit, OnDestroy, TemplateRef, ElementRef, Pipe, PipeTransform, ViewChildren, QueryList } from '@angular/core';
import { FormBuilder, FormArray, Validators, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { AppService } from '../../../services/app.service';
import {
  UserAC, CompanyAC, AddressAC, EntityService, GlobalService, CompanyStructureAC,
  NAICSIndustryType, BusinessAgeAC, CompanySizeAC, IndustryExperienceAC, EntityAC, IndustryTypeAC,
  EntityRelationMappingAC, ApplicationService, AppSettingAC, LoanApplicationStatusType, ProblemDetails
} from '../../../utils/serviceNew';
import { ToastrService } from 'ngx-toastr';
import { TypeaheadMatch } from 'ngx-bootstrap/typeahead/typeahead-match.class';
import { Observable, Subscription, noop, of, Observer } from 'rxjs';
import { Constant } from '../../../shared/constant';
import { SmartyStreetsService } from './smartyStreets.service';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { CompanyStructureOption } from './models/companyStructure.model';
import { switchMap, map, tap } from 'rxjs/operators';
import USStateListJson from '../../../../assets/USStateList.json';

@Component({
  selector: 'app-company-info',
  templateUrl: './company-info.component.html',
  styleUrls: ['./company-info.component.scss']
})
export class CompanyInfoComponent implements OnInit, OnDestroy {
  companyStructureQuestion = Constant.companyStructureQuestion;
  USStateList = USStateListJson;
  partnerErrorMessage: string = null;
  mojorityPercentage: number = null;
  isEditPartnerIndex: number;
  addressSuggestions: Observable<[]>;
  entity: EntityAC = new EntityAC;
  companyStructureOptions: CompanyStructureOption[] = [];
  industryGroupList: NAICSIndustryType[] = [];
  businessAgeList: BusinessAgeAC[] = [];
  companySizeList: CompanySizeAC[] = [];
  industryExperienceList: IndustryExperienceAC[] = [];
  loanProductDetailsRedirectUrl = Constant.loanProductDetailsRedirectUrl;
  companyInfoTitle = Constant.companyInfoTitle;
  addressApiResponse;
  addressSearchString: string;
  isValidAddress = false;
  modalRef: BsModalRef;
  newPartner = [];
  role: string = Constant.proprietorshipOptionName;
  companyId: string;
  applicationId: string;
  addressId: string;
  tempForm: FormGroup;
  count: number;
  //Validation Messages
  requiredField = Constant.requiredField;
  invalidEIN = Constant.invalidEIN;
  invalidZipCode = Constant.invalidzipCode;
  invalidEmail = Constant.invalidEmail;
  invalidPhone = Constant.invalidPhone;
  duplicateEmail = Constant.duplicateEmail;
  duplicatePhone = Constant.duplicatePhone;
  months = Constant.months;
  companyStructure = Constant.companyStructure;
  allQuestions = Constant.companyAllQuestions;
  isFiscalYear = Constant.companyIsFiscalYear;
  addPartnerDeatils = Constant.companyAddPartnerDeatils;
  invalidTotalPercentage = Constant.invalidTotalPercentage;
  invalidPartnershipSingleUserPercentage = Constant.invalidPartnershipSingleUserPercentage;
  zeroSharePercentageError = Constant.zeroSharePercentageError;
  isInvalidSharePercentage = false;
  isInvalidPartnershipSingleUserPercentage = false;
  isTotalHundredSharePercentage = false;
  totalShare = 0;
  maskSSN = true;
  ssnCustomPattern = {
    X: { pattern: new RegExp('\\d'), symbol: 'X' },
    0: { pattern: new RegExp('\\d') }
  };
  isSharePercentageZero = false;

  @ViewChildren('companyViewOnly') viewMode: QueryList<ElementRef>;

  constructor(
    private readonly modalService: BsModalService,
    private readonly toastrService: ToastrService,
    private readonly fb: FormBuilder,
    private readonly appService: AppService,
    private readonly router: Router,
    private readonly entityService: EntityService,
    private readonly applicationService: ApplicationService,
    private readonly globalService: GlobalService,
    private readonly smartyStreetsService: SmartyStreetsService
  ) {
    this.appService.updateLoader(false);
    this.loader = true;
    this.appService.updateRoute(Constant.loanProductDetailsRedirectUrl);
    this.appService.updateNextRoute(Constant.financesRedirectUrl);

    // Coder as per new UX starts here
    this.companyStructure.forEach(question => {
      this.entityCompanyForm.addControl(question.name, fb.control(Constant.proprietorshipOptionName, Validators.required));
    });

  }

  /**
   * Methods for adding and removing partner
   * @param template
   * @param i index number
   */
  config = {
    ignoreBackdropClick: true
  };
  openModal(template: TemplateRef<ElementRef>, i) {
    this.partnerErrorMessage = null;
    this.modalRef = this.modalService.show(template, this.config);
    if (this.shareholder.controls && this.shareholder.controls.length > 0) {
      this.totalShare = 0;
      for (const share of this.shareholder.controls) {
        this.totalShare = this.totalShare + (+share.get('shares').value);
      }
    }
    if (i !== null) {
      this.totalShare = this.totalShare - this.shareholder.at(i).get('shares').value;
      this.partnerDetailsForm.patchValue({
        id: this.shareholder.at(i).get('id').value,
        firstname: this.shareholder.at(i).get('firstname').value,
        lastname: this.shareholder.at(i).get('lastname').value,
        email: this.shareholder.at(i).get('email').value,
        phone: this.shareholder.at(i).get('phone').value,
        dob: this.shareholder.at(i).get('dob').value,
        shares: this.shareholder.at(i).get('shares').value,
        ssn: this.shareholder.at(i).get('ssn').value,
        residencyStatus: this.shareholder.at(i).get('residencyStatus').value,
        entityRelationshipMappingId: this.shareholder.at(i).get('entityRelationshipMappingId').value
      });
    }
    this.isEditPartnerIndex = i;
  }

  /**method to hide modal pop up */
  hideModal() {
    this.modalRef.hide();
    this.totalShare = 0;
    const hundred = 100;
    for (const share of this.shareholder.controls) {
      this.totalShare = this.totalShare + (+share.get('shares').value);
    }
    if (this.totalShare === hundred) {
      this.isTotalHundredSharePercentage = true;
    }
    this.isSharePercentageZero = false;
  }

  /**Method is call when partner is added */
  onPartnerAdd() {
    this.partnerDetailsForm.markAllAsTouched();
    if (this.partnerDetailsForm.get('shares').value === 0) {
      this.isSharePercentageZero = true;
      this.partnerDetailsForm.get('shares').setErrors({ 'invalid': true });
    } else {
      if (this.partnerDetailsForm.valid) {
        if (this.isEditPartnerIndex !== null) {
          this.shareholder.at(this.isEditPartnerIndex).patchValue(this.partnerDetailsForm.value);
          this.modalRef.hide();
        } else {
          this.shareholder.push(this.fb.group(this.partnerDetailsForm.value));
        }
        this.modalRef.hide();
      }
      this.isSharePercentageZero = false;
    }
    
  }

  /**
   * Remove shareholder and its linking to company
   * @param i index number of shareholder
   */
  remove(i: number): void {
    if (this.shareholder.at(i).get('entityRelationshipMappingId').value !== null && this.companyId !== null) {
      this.entity.relationMapping = new EntityRelationMappingAC();
      this.entity.relationMapping.primaryEntityId = this.companyId;
      this.entity.id = this.shareholder.at(i).get('id').value;
      this.entityService.removeLinkEntity(this.shareholder.at(i).get('id').value, this.entity).subscribe(
        data => {
        }
      );
    }
    this.shareholder.removeAt(i);
    this.isTotalHundredSharePercentage = false;
  }
  // Method to add Css class in form
  getClass(gridColumn, optionType) {
    return {
      'col-md-12': gridColumn === 'full' || optionType === 'selectWithSearchIndustryGroup',
      'col-md-6 mt-40': gridColumn === undefined,
      'd-md-flex justify-content-md-between mt-20 align-items-center question-spacing': optionType === 'businessSelectList' || optionType === 'industryExperienceSelectList' || optionType === 'companySelectList'
    };
  }
  // Method for Company structure
  changeCompanyStructure(val) {
    this.role = val;
    //let indexToDetele:number[] = [];
    if (this.shareholder.controls[0].get('shares').value >= this.mojorityPercentage
      && (this.entityCompanyForm.get('structure').value === this.companyStructureOptions.filter(x => x.optionName !== Constant.partnershipOptionName)[0].id
      || this.entityCompanyForm.get('structure').value === this.companyStructureOptions.filter(x => x.optionName !== Constant.proprietorshipOptionName)[0].id)) {
      for (let i = (this.shareholder.controls.length - 1); i >= 0; i--) {
        if (i !== 0) {
          this.remove(i);
        }
      }
    }
  }

  // Form To add Partner Details
  partnerDetailsForm = this.fb.group({
    id: [null],
    firstname: [null],
    lastname: [null],
    email: ['', {
      validators: [
        Validators.required,
        Validators.email],
      updateOn: Constant.blur
    }],
    phone: [null],
    dob: [null],
    shares: [null, [
      Validators.required,
      Validators.pattern(Constant.onlyIntegerOrDecimalPattern)
    ]],
    ssn: [null],
    residencyStatus: [null],
    entityRelationshipMappingId: [null]
  });
  /*variable for differentiating questions categories*/

  loader = true;
  shareHolderUsers: UserAC[] = [];
  shareuser: UserAC;
  companyAC: CompanyAC = new CompanyAC();
  address: AddressAC;
  subsVar: Subscription;

  /*Loan Application form Controls*/
  entityCompanyForm = this.fb.group({
    structure: ['', [
      Validators.required
    ]],
    companyEIN: ['', [
      Validators.required
    ]],
    name: ['', [
      Validators.required
    ]],
    streetLine: ['', { validators: [Validators.required], updateOn: 'change' }],
    city: ['', { validators: [Validators.required], updateOn: 'change' }],
    stateAbbreviation: ['', { validators: [Validators.required], updateOn: 'change' }],
    businessAge: [null, [
      Validators.required
    ]],
    companySize: [null, [
      Validators.required
    ]],
    shareholder: this.fb.array([]),
    industryExperience: [null, [
      Validators.required
    ]],
    registeredState: [null, [
      Validators.required
    ]],
    zipCode: [null, [
      Validators.pattern(Constant.zipCodePattern)
    ]],
    industryGroup: [null, [
      Validators.required
    ]],
    fiscalYear: ['yes', { validators: [Validators.required], updateOn: 'change' }],
    fiscalMonth: [null, [
      Validators.required
    ]],
  },
    { updateOn: Constant.blur });



  /*Methods for dynamically adding the form controls for shareholders*/
  get shareholder() {
    return this.entityCompanyForm.get('shareholder') as FormArray;
  }

  async ngOnInit(): Promise<void> {
    
    this.modalService.onHide.subscribe(
      () => {
        this.partnerDetailsForm.reset({ id: null, residencyStatus: null, shares: null });
      }
    );
    this.partnerDetailsForm.get('shares').valueChanges.subscribe(
      sharePercentage => {
        if (sharePercentage !== null) {
          const hundred = 100;
          const total = this.totalShare + (+sharePercentage);
          this.isInvalidSharePercentage = false;
          this.isInvalidPartnershipSingleUserPercentage = false;
          this.isTotalHundredSharePercentage = false;
          this.isSharePercentageZero = false;
          if (total > hundred) {
            this.partnerDetailsForm.get('shares').setErrors({ 'invalid': true });
            this.isInvalidSharePercentage = true;
          } else if (sharePercentage === hundred
            && this.entityCompanyForm.get('structure').value === this.companyStructureOptions.filter(x => x.optionName === Constant.partnershipOptionName)[0].id) {
            this.isInvalidPartnershipSingleUserPercentage = true;
            this.partnerDetailsForm.get('shares').setErrors({ 'invalid': true });
          } else if (total === hundred) {
            this.isTotalHundredSharePercentage = true;
          } else { }
        }
      }
    );
    this.companyId = await this.appService.getCurrentCompanyId();
    const isLinked = await this.appService.getCompanyLinked();
    if (this.companyId === null || isLinked === null) {
      this.appService.setCompanyLinked(false);
    }
    this.applicationId = await this.appService.getCurrentLoanApplicationId();
    const appSettings: AppSettingAC[] = await this.appService.getAppSettings();
    this.mojorityPercentage = +appSettings.filter(x => x.fieldName === Constant.mojorityPercentage)[0].value;
    this.getCompanyOptions();

    //SmartyStreet autocomplete api
    this.addressSuggestions = new Observable((observer: Observer<string>) => {
      observer.next(this.entityCompanyForm.get('streetLine').value);
    }).pipe(
      switchMap((token: string) => {
        this.isValidAddress = false;
        if (token) {
          return this.smartyStreetsService.fetchSuggestions(token)
            .pipe(
              map(data => {
                this.addressApiResponse = data.result;
                return data && data.result || [];
              }),
              tap(() => noop, err => {
                this.toastrService.error(Constant.someThingWentWrong);
              })
            );
        } else {
          return of([]);
        }
      })
    );
  }

  ngOnDestroy() {
    if (this.subsVar) {
      this.subsVar.unsubscribe();
    }
  }
  onSelect(event: TypeaheadMatch): void {
    this.entityCompanyForm.get('streetLine').setValue(event.item.streetLine);
    this.entityCompanyForm.get('city').setValue(event.item.city);
    this.entityCompanyForm.get('stateAbbreviation').setValue(event.item.state);
  }

  /**Fetch all company options and set current user */
  async getCompanyOptions() {

    this.globalService.getCompanyStructureList().subscribe(
      (companyStructureList: CompanyStructureAC[]) => {
        for (const companyStructure of companyStructureList) {
          const structure = new CompanyStructureOption;
          structure.name = 'companystructure';
          if (companyStructure.structure === Constant.proprietorship) {
            structure.id = companyStructure.id;
            structure.imageUrl = Constant.proprietorshipImagePath;
            structure.optionName = Constant.proprietorshipOptionName;
            structure.title = companyStructure.structure;
            this.entityCompanyForm.get('structure').setValue(companyStructure.id);
          } else if (companyStructure.structure === Constant.partnership) {
            structure.id = companyStructure.id;
            structure.imageUrl = Constant.partnershipImagePath;
            structure.optionName = Constant.partnershipOptionName;
            structure.title = companyStructure.structure;
          } else if (companyStructure.structure === Constant.limitedLiabilityCompany) {
            structure.id = companyStructure.id;
            structure.imageUrl = Constant.llcImagePath;
            structure.optionName = Constant.llcOptionName;
            structure.title = companyStructure.structure;
          } else if (companyStructure.structure === Constant.sCorporation) {
            structure.id = companyStructure.id;
            structure.imageUrl = Constant.scorporationImagePath;
            structure.optionName = Constant.sCorporationOptionName;
            structure.title = companyStructure.structure;
          } else if (companyStructure.structure === Constant.cCorporation) {
            structure.id = companyStructure.id;
            structure.imageUrl = Constant.ccorporationImagePath;
            structure.optionName = Constant.cCorporationOptionName;
            structure.title = companyStructure.structure;
          } else {
            this.toastrService.error(Constant.someThingWentWrong);
          }
          this.companyStructureOptions.push(structure);
        }
      }
    );
    this.globalService.getIndustryGroupList().subscribe(
      (industryGroupList: NAICSIndustryType[]) => {
        this.industryGroupList = industryGroupList;
        for (const industry of industryGroupList) {
          industry.industryType = `${industry.industryCode} ${industry.industryType}`;
        }
      }
    );

    this.globalService.getBusinessAgeRangeList().subscribe(
      (businessAgeList: BusinessAgeAC[]) => {
        businessAgeList.sort(this.appService.sortByAnyIntegerField);
        this.businessAgeList = businessAgeList;
      }
    );

    this.globalService.getCompanySizeList().subscribe(
      (companySizeList: CompanySizeAC[]) => {
        companySizeList.sort(this.appService.sortByAnyIntegerField);
        this.companySizeList = companySizeList.filter(x => x.isEnabled === true);
      }
    );

    this.globalService.getIndustryExperienceList().subscribe(
      (industryExperienceList: IndustryExperienceAC[]) => {
        industryExperienceList.sort(this.appService.sortByAnyIntegerField);
        this.industryExperienceList = industryExperienceList.filter(x => x.isEnabled === true);
      }
    );

    this.entityService.getEntityList(null, null, null, '[{\'Field\':\'type\', \'Operator\':\'=\', \'Value\':\'people\'}]', null, null).subscribe(
      async (entityCurrentUser: EntityAC[]) => {
        this.shareholder.push(
          this.fb.group({
            id: [entityCurrentUser[0].id, [Validators.required]],
            firstname: [entityCurrentUser[0].user.firstName, [Validators.required]],
            lastname: [entityCurrentUser[0].user.lastName, [Validators.required]],
            email: [entityCurrentUser[0].user.email, [Validators.required, Validators.email]],
            phone: [entityCurrentUser[0].user.phone, [Validators.required, Validators.pattern(Constant.phoneNumberPattern)]],
            dob: [entityCurrentUser[0].user.dob],
            ssn: [entityCurrentUser[0].user.ssn],
            shares: [null, [Validators.required]],
            residencyStatus: [entityCurrentUser[0].user.residencyStatus],
            entityRelationshipMappingId: [null]
          })
        );
        if (this.companyId === entityCurrentUser[0].id) {
          this.companyId = null;
          this.appService.setCompanyLinked(false);
        }
        const lockedApplication = await this.appService.getLockedApplicationJsonAsObject();
        if (lockedApplication && await this.appService.getCurrentLoanApplicationStatus() !== LoanApplicationStatusType.Draft) {
          this.patchFormValue(lockedApplication.borrowingEntities[0]);

        } else if (this.companyId !== null) {
          this.entityService.getEntity(this.companyId).subscribe(
            (entity: EntityAC) => {
              this.addressId = entity.address.id;
              this.companyId = entity.id;
              this.patchFormValue(entity);
              if (entity.company.createdByUserId !== this.shareholder.at(0).get('id').value) {
                this.viewMode.forEach(el => el.nativeElement.classList.add('company-view-only'));
                this.appService.changeCompanyMode(false);
              } else {
                this.appService.changeCompanyMode(true);
              }
            }
          );
        } else {
          this.loader = false;
        }
      }
    );
  }

  /**While if form value is not changed */
  async formValueNotChanged(){
    const currentSection = await this.appService.getCurrentSectionName();
      const companyId = await this.appService.getCurrentCompanyId();
      if (currentSection === Constant.companyInfo) {
        this.applicationService.linkApplicationWithEntity(this.applicationId, companyId).subscribe(
          async () => {
            this.appService.setCurrentCompanyId(companyId);
            await this.updateSectionAsync();
          },
          () => {
            this.loader = false;
          }
        );
      } else {
        this.router.navigate([Constant.financesRedirectUrl]);
      }
  }

  /**While submitting if there is any change in form */
  async formValueChanged() {
    if (this.checkValidForm()) {
      this.loader = true;
      this.partnerErrorMessage=null;

      this.entity = new EntityAC({
        id: null,
        type: 0,
        relationMapping: null,
        company: new CompanyAC({
          name: this.entityCompanyForm.get('name').value,
          cin: this.entityCompanyForm.get('companyEIN').value,
          companyFiscalYearStartMonth: null,
          companyRegisteredState: null,
          businessAge: new BusinessAgeAC({
            id: this.entityCompanyForm.get('businessAge').value,
            age: null,
            order: null
          }),
          companySize: new CompanySizeAC({
            id: this.entityCompanyForm.get('companySize').value,
            size: null,
            order: null,
            isEnabled: true
          }),
          companyStructure: new CompanyStructureAC({
            id: this.entityCompanyForm.get('structure').value,
            structure: null,
            order: null
          }),
          industryExperience: new IndustryExperienceAC({
            id: this.entityCompanyForm.get('industryExperience').value,
            experience: null,
            order: null,
            isEnabled: true
          }),
          industryType: new IndustryTypeAC({
            id: this.entityCompanyForm.get('industryGroup').value,
            industryCode: null,
            industryType: null
          }),
          createdByUserId: null
        }),
        address: new AddressAC({
          id: null,
          city: this.entityCompanyForm.get('city').value,
          streetLine: this.entityCompanyForm.get('streetLine').value,
          stateAbbreviation: this.entityCompanyForm.get('stateAbbreviation').value,
          integratedServiceConfigurationId: null
        })
      });

      if (this.entityCompanyForm.get('structure').value !== this.companyStructureOptions.filter(x => x.optionName === Constant.proprietorshipOptionName)[0].id) {
        this.entity.company.companyRegisteredState = this.entityCompanyForm.get('registeredState').value;
      }
      if (this.entityCompanyForm.get('structure').value === this.companyStructureOptions.filter(x => x.optionName === Constant.cCorporationOptionName)[0].id
        && this.entityCompanyForm.get('fiscalYear').value === 'no') {
        this.entity.company.companyFiscalYearStartMonth = this.entityCompanyForm.get('fiscalMonth').value;
      }
      if (this.companyId === null) {
        this.entityService.addEntity('company', this.entity).subscribe(
          (addedEntity: EntityAC) => {
            this.companyId = addedEntity.id;
            this.addressId = addedEntity.address.id;
            this.appService.setCurrentCompanyId(addedEntity.id);
            this.saveShareHolders(addedEntity.id);
          },
          (err: ProblemDetails) => {
            this.toastrService.error(err.detail);
            this.loader = false;
          }
        );

      } else {
        this.entity.id = this.companyId;
        this.entity.address.id = this.addressId;
        this.entityService.updateEntity(this.companyId, this.entity).subscribe(
          async (updatedEntity: EntityAC) => {
            this.saveShareHolders(updatedEntity.id);
          },
          (err: ProblemDetails) => {
            this.toastrService.error(err.detail);
            this.loader = false;
          }

        );
      }
    }
   
  }
  /**Submits and add or updatte whole company form*/
  async onSubmit() {
    this.entityCompanyForm.markAllAsTouched();
    if (JSON.stringify(this.tempForm) === JSON.stringify(this.entityCompanyForm.value)) {
      await this.formValueNotChanged();
    } else {
      await this.formValueChanged();
    }
  }

  /**
   * Patch company form
   * @param entity
   */
  patchFormValue(entity: EntityAC) {
    this.role = this.companyStructureOptions.filter(x => x.id === entity.company.companyStructure.id)[0].optionName;
    
    this.entityCompanyForm.patchValue({
      structure: entity.company.companyStructure.id,
      companyEIN: entity.company.cin,
      name: entity.company.name,
      streetLine: entity.address.streetLine,
      city: entity.address.city,
      stateAbbreviation: entity.address.stateAbbreviation,
      zipCode: entity.address.zipCode,
      industryGroup: entity.company.industryType.id,
      businessAge: entity.company.businessAge.id,
      companySize: entity.company.companySize.id,
      industryExperience: entity.company.industryExperience.id,
      fiscalMonth: entity.company.companyFiscalYearStartMonth,
      registeredState: entity.company.companyRegisteredState,
      fiscalYear: entity.company.companyFiscalYearStartMonth === null ? 'yes' : 'no'
    });
    let total = 0;
    for (const shareholder of entity.linkedEntities) {
      total = shareholder.relationMapping.sharePercentage + total;
      if (this.shareholder.at(0).get('email').value !== shareholder.user.email && entity.linkedEntities.length !== 1) {
        this.shareholder.push(
          this.fb.group({
            id: [shareholder.id],
            firstname: [shareholder.user.firstName, [Validators.required]],
            lastname: [shareholder.user.lastName, [Validators.required]],
            email: [shareholder.user.email, [Validators.required, Validators.email]],
            phone: [shareholder.user.phone, [Validators.required, Validators.pattern(Constant.phoneNumberPattern)]],
            dob: [shareholder.user.dob],
            shares: [shareholder.relationMapping.sharePercentage, [Validators.required, Validators.pattern(Constant.onlyIntegerOrDecimalPattern)]],
            ssn: [shareholder.user.ssn],
            residencyStatus: [shareholder.user.residencyStatus],
            entityRelationshipMappingId: [shareholder.relationMapping.id]
          })
        );
      } else {
        this.shareholder.at(0).get('id').setValue(shareholder.id);
        this.shareholder.at(0).get('firstname').setValue(shareholder.user.firstName);
        this.shareholder.at(0).get('lastname').setValue(shareholder.user.lastName);
        this.shareholder.at(0).get('email').setValue(shareholder.user.email);
        this.shareholder.at(0).get('phone').setValue(shareholder.user.phone);
        this.shareholder.at(0).get('dob').setValue(shareholder.user.dob);
        this.shareholder.at(0).get('ssn').setValue(shareholder.user.ssn);
        this.shareholder.at(0).get('residencyStatus').setValue(shareholder.user.residencyStatus);
        this.shareholder.at(0).get('shares').setValue(shareholder.relationMapping.sharePercentage);
        this.shareholder.at(0).get('entityRelationshipMappingId').setValue(shareholder.relationMapping.id);
      }

    }
    if (entity.company.companyStructure.id === this.companyStructureOptions.filter(x => x.optionName === Constant.proprietorshipOptionName)[0].id) {
      this.shareholder.at(0).get('shares').setValue(null);
    }
    const hundred = 100;
    if (total === hundred && this.role !== Constant.proprietorshipOptionName) {
      this.isTotalHundredSharePercentage = true;
    }
    
    this.tempForm = this.entityCompanyForm.value;
    this.loader = false;
  }

  /**Check all Validation in company form */
  checkValidForm() {
    const formFieldNames = ['structure', 'companyEIN', 'name', 'streetLine', 'city', 'stateAbbreviation', 'businessAge', 'companySize', 'industryExperience', 'industryGroup'];

    for(const element of formFieldNames){
      if(this.entityCompanyForm.get(element).invalid){
        return false;
      }
    }

    if (this.entityCompanyForm.get('structure').value === this.companyStructureOptions.filter(x => x.optionName === Constant.proprietorshipOptionName)[0].id) {
      return true;
    }
    
    if (this.entityCompanyForm.get('registeredState').invalid) {
      return false;
    }
    if (this.entityCompanyForm.get('structure').value === this.companyStructureOptions.filter(x => x.optionName === Constant.cCorporationOptionName)[0].id
      && this.entityCompanyForm.get('fiscalMonth').invalid && this.entityCompanyForm.get('fiscalYear').value === 'no') {
      return false;
    }

  const corps=[Constant.sCorporationOptionName,Constant.llcOptionName,Constant.cCorporationOptionName];
    if (corps.some(x=>x === this.companyStructureOptions.filter(z => z.id === this.entityCompanyForm.get('structure').value)[0].optionName)
    && this.shareholder.controls.length === 1) {
    this.partnerErrorMessage = Constant.singleLowPercentage;
    return this.shareholder.controls[0].get('shares').value >= this.mojorityPercentage;
    }
    return this.checkPartnerFormDetails();
  }

  /**Check partners form details entered correct */
  private checkPartnerFormDetails() {
    const emailList: string[] = [];
    //const phoneList: string[] = [];
    let totalsharepercentage = 0;
    for (const shareholder of this.shareholder.controls) {
      emailList.push(shareholder.get('email').value);
      //phoneList.push(shareholder.get('phone').value);
      totalsharepercentage = totalsharepercentage + (+shareholder.get('shares').value);
    }
    const hundred = 100;
    if (this.shareholder.controls.some(x => x.get('shares').value === null)) {
      this.partnerErrorMessage = Constant.addSharePercentageMessage;
      return false;
    }

    if (!this.validateLength(emailList, 'email')) {
      return false;
    }
    return this.validatePercentage(totalsharepercentage, hundred);
  }

  /**
* Check partners share percentage added are valid
* @param totalsharepercentage
* @param hundredPercentage
*/
  private validatePercentage(totalsharepercentage: number, hundredPercentage: number) {
    if (totalsharepercentage > hundredPercentage || totalsharepercentage < hundredPercentage) {
      this.partnerErrorMessage = Constant.invalidTotalPercentage;
      return false;
    }
    return true;
  }

  /**
   * Check provide list does not have duplication for email and phone list
   * @param listToValidateOn
   * @param entity
   */
  private validateLength(listToValidateOn:Array<string>, entity:string){
    if ((new Set(listToValidateOn)).size !== listToValidateOn.length) {
      this.partnerErrorMessage= Constant.duplicateEmail;
            return false;
    }
    return true;
  }



  /**
   * Method to add or update all the shareholders
   * @param companyId company id
   */
  async saveShareHolders(companyId: string) {
    if (this.entityCompanyForm.get('structure').value === this.companyStructureOptions.filter(x => x.optionName === Constant.proprietorshipOptionName)[0].id) {
      const isLinked = await this.appService.getCompanyLinked();
      if (isLinked === false) {
        this.applicationService.linkApplicationWithEntity(this.applicationId, companyId).subscribe(
          async () => {
            this.appService.setCurrentCompanyId(companyId);
            await this.updateSectionAsync();
          },
          () => {
            this.loader = false;
          }
        );
      } else {
        this.appService.setCurrentCompanyId(companyId);
        this.router.navigate([Constant.financesRedirectUrl]);
      }
    } else {
      this.count = 0;
      for (const shareholder of this.shareholder.controls) {
        this.entity = new EntityAC({
          id: shareholder.get('id').value,
          type: 0,
          relationMapping: new EntityRelationMappingAC({
            id: shareholder.get('entityRelationshipMappingId').value,
            primaryEntityId: companyId,
            relation: null,
            sharePercentage: shareholder.get('shares').value
          }),
          user: new UserAC({
            email: shareholder.get('email').value,
            firstName: shareholder.get('firstname').value,
            middleName: null,
            lastName: shareholder.get('lastname').value,
            phone: shareholder.get('phone').value,
            dob: shareholder.get('dob').value,
            ssn: shareholder.get('ssn').value,
            selfDeclaredCreditScore: null,
            residencyStatus: shareholder.get('residencyStatus').value,
            hasAnyJudgementsSelfDeclared: null,
            hasBankruptcySelfDeclared: null
          })
        });
        if (this.entityCompanyForm.get('structure').value === this.companyStructureOptions.filter(x => x.optionName === Constant.proprietorshipOptionName)[0].id) {
          this.entity.relationMapping = null;
          this.entity.user.ssn = this.entityCompanyForm.get('companyEIN').value;
        }
        if (this.entity.id === null) {
          this.entityService.addEntity('people', this.entity).subscribe(
            async (entity: EntityAC) => {
              await this.addOrUpdateEntityAsync(entity, companyId);
            },
            (err: ProblemDetails) => {
              this.toastrService.error(err.detail);
              this.loader = false;
            }
          );
        } else {
          this.entityService.updateEntity(this.entity.id, this.entity).subscribe(
            async (entity: EntityAC) => {
              await this.addOrUpdateEntityAsync(entity, companyId);
            },
            (err: ProblemDetails) => {
              this.toastrService.error(err.detail);
              this.loader = false;
            }
          );
        }
      }
    }
  }

  /**
   * Method to add or update entity people
   * @param entity Data return from add or update entity API
   * @param companyId current added company Id
   */
  async addOrUpdateEntityAsync(entity: EntityAC,companyId:string) {
    if (entity.relationMapping) {
      this.shareholder.at(this.count).get('entityRelationshipMappingId').setValue(entity.relationMapping.id);
    }

    this.shareholder.at(this.count).get('id').setValue(entity.id);
    this.count++;
    if (this.shareholder.controls.length === this.count) {
      const isLinked = await this.appService.getCompanyLinked();
      if (isLinked === false) {
        this.applicationService.linkApplicationWithEntity(this.applicationId, companyId).subscribe(
          async () => {
            this.appService.setCurrentCompanyId(companyId);
            await this.updateSectionAsync();
          },
          () => {
            this.loader = false;
          }
        );
      } else {
        this.appService.setCurrentCompanyId(companyId);
        this.router.navigate([Constant.financesRedirectUrl]);
      }
    }
  }

  /**SSN focus change */
  ssnFocusFunction() {
    this.maskSSN = !this.maskSSN;
  }

  /**Method to update current section*/
  async updateSectionAsync() {
    const currentSection = await this.appService.getCurrentSectionName();
    this.applicationService.updateCurrentSectionName(this.applicationId, currentSection).subscribe(
      async (updatedSectionName: string) => {
        if (updatedSectionName !== null) {
          await this.appService.updateCurrentSectionName(updatedSectionName);
          await this.appService.updateProgressbar(Constant.financesProgressBar);
          await this.appService.setCompanyLinked(true);
          this.router.navigate([Constant.financesRedirectUrl]);
        }
      }
    );
  }
}

