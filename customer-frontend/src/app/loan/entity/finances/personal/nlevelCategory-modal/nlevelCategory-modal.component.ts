import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Constant } from '../../../../../shared/constant';
import {
  AddressAC, PersonalFinanceAttributeAC, PersonalFinanceAttributeFieldType,
  PersonalFinanceCategoryAC, PersonalFinanceOrderedAttributeAC
} from '../../../../../utils/serviceNew';
import { AppService } from '../../../../../services/app.service';
import { ToastrService } from 'ngx-toastr';
import { noop, Observable, Observer, of } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { SmartyStreetsService } from '../../../company-info/smartyStreets.service';
import { TypeaheadMatch } from 'ngx-bootstrap/typeahead';
import { FieldSetList } from '../fieldSetList.modal';

@Component({
  selector: 'app-nlevelpersonalfinance-modal',
  templateUrl: './nlevelCategory-modal.component.html',
})
export class NLevelCategoryModalComponent implements OnInit {
  @Input() selectedCategory: PersonalFinanceCategoryAC;
  @Output() submitEvent = new EventEmitter<FieldSetList>();
  @Input() currentUserId: string;
  @Input() currency: string;
  @Input() isLocked: boolean;
  @Input() parentCategory: PersonalFinanceCategoryAC;
  @Output() fillParentEvent = new EventEmitter<PersonalFinanceCategoryAC>();
  addressSuggestions: Array<Observable<[]>>;
  submit: string;
  selected: string;
  selectorName: string;
  nextModal: string;
  addAnotherField: string;
  isFirstOpen: Array<boolean>;
  customClass = 'personal-finances-accordion';
  attributeFieldType: typeof PersonalFinanceAttributeFieldType = PersonalFinanceAttributeFieldType;
  showFillParentMessage = false;
  //bankRegex = /^\d{12}$/;
  childRendering = false;
  constructor(private readonly appService: AppService,
    private readonly toastrService: ToastrService,
    private readonly smartyStreetsService: SmartyStreetsService) { }

  ngOnInit(): void {

    this.isFirstOpen = new Array<boolean>();
    this.isFirstOpen.push(true);
    this.initializeModalControls();
    this.submit = Constant.submit;
    this.handleChildModalRendering();

    if (this.selectedCategory.attributes && this.selectedCategory.attributes.length>0) {
      this.selectedCategory.attributes[0].childAttributeSets.sort((a, b) => a.order - b.order);
      for (const attributes of this.selectedCategory.attributes[0].childAttributeSets) {
        attributes.childAttributes.sort((a, b) => a.order - b.order);
      }
      this.initializeSuperParent();
      this.initializeArray(this.selectedCategory.attributes[0].childAttributeSets);
      const isSuperQuestionUnfilled = (this.selectedCategory.attributes[0].fieldType === this.attributeFieldType.Boolean && !this.selectedCategory.attributes[0].booleanAnswer)
        || (this.selectedCategory.attributes[0].fieldType === this.attributeFieldType.SingleCheckbox && this.selectedCategory.attributes[0].booleanAnswer);

    }
    
    
  }

  handleChildModalRendering() {
    this.childRendering = false;
    if (this.parentCategory) {
      if (this.parentCategory.attributes && this.parentCategory.attributes.length > 0
        && (this.parentCategory.attributes[0].childAttributeSets.some(z => z.childAttributes.some(a => a.answer)
          || this.parentCategory.attributes[0].answer === 'true'))) {
        this.showFillParentMessage = false;
        const parentAttribute = this.parentCategory.parentAttribute.text;
        const copiedParentCategory = this.appService.deepCopy(this.parentCategory);
        const superAttribute = copiedParentCategory.attributes[0].childAttributeSets[0].childAttributes.filter(x => x.text === parentAttribute)[0];
        this.selectedCategory.attributes = new Array<PersonalFinanceAttributeAC>();
        this.selectedCategory.attributes.push(new PersonalFinanceAttributeAC());
        this.selectedCategory.attributes[0].answer = superAttribute.answer;
        this.selectedCategory.attributes[0].booleanAnswer = superAttribute.booleanAnswer;
        this.selectedCategory.attributes[0].text = superAttribute.text;
        this.selectedCategory.attributes[0].order = superAttribute.order;
        this.selectedCategory.attributes[0].fieldType = superAttribute.fieldType;
        this.selectedCategory.attributes[0].childAttributeSets = new Array<PersonalFinanceOrderedAttributeAC>();

        for (let attribute of copiedParentCategory.attributes[0].childAttributeSets) {
          for (let item of attribute.childAttributes.filter(x => x.childAttributeSets && x.childAttributeSets.length > 0)) {
            if (item.text === parentAttribute) {
              const orderedAttribute = new PersonalFinanceOrderedAttributeAC();
              orderedAttribute.childAttributes = item.childAttributeSets[0].childAttributes;
              orderedAttribute.order = attribute.order;
              this.selectedCategory.attributes[0].childAttributeSets.push(orderedAttribute);
            }
            
          }
        }
        
        this.childRendering = true;
        
      } else {
        this.showFillParentMessage = true;
      }
    } else {
      this.showFillParentMessage = false;
    }
  }

  initializeSuperParent() {
    if (this.selectedCategory.attributes && !this.selectedCategory.attributes[0].answer || this.selectedCategory.attributes[0].answer === 'false') {
      this.selectedCategory.attributes[0].booleanAnswer = false;
    } else {
      this.selectedCategory.attributes[0].booleanAnswer = true;
    }
  }

  initializeArray(childAttributeSets: Array<PersonalFinanceOrderedAttributeAC>) {
    childAttributeSets.forEach(attribute => {
      attribute.childAttributes.forEach(y => {
        if (y.fieldType === PersonalFinanceAttributeFieldType.Boolean) {
          if (!y.answer || y.answer === 'false') {
            y.booleanAnswer = false;
            y.answer = 'false';
            y.childAttributeSets[0].childAttributes.forEach(z => z.answer = '');
          } else {
            y.answer = 'true';
            y.booleanAnswer = true;
          }
          y.childAttributeSets[0].childAttributes.sort((a, b) => a.order - b.order);

        } else if (y.fieldType === PersonalFinanceAttributeFieldType.Address) {
          y.address = new AddressAC();
          y.address.addressSuggestion = new Array<Observable<[]>>();
          y.address.addressSuggestion.push(this.prepareAddressObservable(y));
          if (y.answer) {
            y.address.streetLine = this.getStreetLine(y);
            y.address.city = this.getCity(y);
            y.address.stateAbbreviation = this.getState(y);
            y.address.zipCode = this.getZipCode(y);
          }

        }

        if (y.childAttributeSets && y.childAttributeSets.length>0 && y.childAttributeSets[0].childAttributes && y.childAttributeSets[0].childAttributes.length > 0) {
          this.initializeArray(y.childAttributeSets);
        }

      })
    });
  }

  // Add more question set
  addMoreFields() {
    const latestCount = this.selectedCategory.attributes[0].childAttributeSets.length;
    const fieldSet = this.selectedCategory.attributes[0].childAttributeSets[latestCount - 1];
    const newFieldSet = this.appService.deepCopy(fieldSet);
    this.sanitizeAttribute(newFieldSet.childAttributes);
    this.selectedCategory.attributes[0].childAttributeSets.push(newFieldSet);
    const latestFieldSet = this.selectedCategory.attributes[0].childAttributeSets[latestCount];
    latestFieldSet.order = this.selectedCategory.attributes[0].childAttributeSets.length;

    for (let i = 0; i < latestCount; i++) {
      this.isFirstOpen[i] = false;
    }
    this.isFirstOpen.push(true);
  }

  sanitizeAttribute(childAttributes: Array<PersonalFinanceAttributeAC>) {
    childAttributes.forEach(x => {
      x.answer = '';
      if (x.fieldType === PersonalFinanceAttributeFieldType.Boolean || x.fieldType === PersonalFinanceAttributeFieldType.SingleCheckbox) {
        x.answer = 'false';
        x.booleanAnswer = false;
      } else if (x.fieldType === PersonalFinanceAttributeFieldType.Address) {
        x.address = new AddressAC();
        x.address.streetLine = '';
        x.address.addressSuggestion = new Array<Observable<[]>>();
        x.address.addressSuggestion.push(this.prepareAddressObservable(x));
      }

      if (x.childAttributeSets && x.childAttributeSets.length>0 &&  x.childAttributeSets[0].childAttributes && x.childAttributeSets[0].childAttributes.length > 0) {
        x.childAttributeSets[0].order = this.selectedCategory.attributes[0].childAttributeSets.length + 1;
        this.sanitizeAttribute(x.childAttributeSets[0].childAttributes);
      }
    });
  }

  // Delete question set
  deleteFields(order: number) {
    this.selectedCategory.attributes[0].childAttributeSets = this.selectedCategory.attributes[0].childAttributeSets.filter(x => x.order !== order);
    for (const attributes of this.selectedCategory.attributes[0].childAttributeSets) {
      if (attributes.order > order) {
        attributes.order = attributes.order - 1;
        this.assignOrderToChildrensChildren(attributes.childAttributes, attributes.order);
      }
    }
    
    this.isFirstOpen.splice(order - 1, 1);
  }
  assignOrderToChildrensChildren(childAttributes: Array<PersonalFinanceAttributeAC>, order: number) {
    childAttributes.forEach(attribute => {
      if (attribute.childAttributeSets && attribute.childAttributeSets.length > 0) {
        attribute.childAttributeSets[0].order = order;
        if (attribute.childAttributeSets[0].childAttributes && attribute.childAttributeSets[0].childAttributes.length > 0) {
          this.assignOrderToChildrensChildren(attribute.childAttributeSets[0].childAttributes, order);
        }
      }
      
    })
  }
  // Save button click handler
  save(navigateToNextTab: boolean) {
    
    if (this.selectedCategory.attributes[0].booleanAnswer) {
      this.selectedCategory.attributes[0].answer = 'true';
    } else {
      this.selectedCategory.attributes[0].answer = 'false';
    }

    const hasAnswersInChild = this.selectedCategory.attributes[0].childAttributeSets
      .some(x => x.childAttributes.filter(z => z.fieldType !== PersonalFinanceAttributeFieldType.Boolean
      && z.fieldType !== PersonalFinanceAttributeFieldType.SingleCheckbox).some(y => y.answer));

    const doHaveChild = (this.selectedCategory.attributes[0].fieldType === this.attributeFieldType.Boolean && this.selectedCategory.attributes[0].booleanAnswer) ||
      (this.selectedCategory.attributes[0].fieldType === this.attributeFieldType.SingleCheckbox && !this.selectedCategory.attributes[0].booleanAnswer);
    // If the super question's answer is that they have child questions to answer but there isn't any child answer, throw validation

    

    if (!hasAnswersInChild && doHaveChild) {
      this.toastrService.error(Constant.fillAllRequiredFieldsError);
    } else {
      let allValidFields = false;
      for (const attributeSet of this.selectedCategory.attributes[0].childAttributeSets) {
        allValidFields = this.validateFields(attributeSet.childAttributes);
      }

      if (allValidFields) {
        this.handleSuperParentChange();
        const savedCategory = new FieldSetList();
        savedCategory.navigateToNextCategory = navigateToNextTab;
        savedCategory.savedCategory = this.selectedCategory;
        this.submitEvent.emit(savedCategory);
      }
      
    }
  }

  // Method that validates if all the on-screen questions are filled up or not
  validateFields(childAttributes: Array<PersonalFinanceAttributeAC>): boolean {
    let isFieldValid = true;
    for (const x of childAttributes){
      if (!x.answer) {
        this.toastrService.error(`Missing value for: ${x.text}`);
        isFieldValid = false;
        break;
      }
      if (x.fieldType === this.attributeFieldType.Boolean && x.answer === 'true') {
        if (x.childAttributeSets && x.childAttributeSets.length > 0 && x.childAttributeSets[0].childAttributes && x.childAttributeSets[0].childAttributes.length > 0) {
          isFieldValid = this.validateFields(x.childAttributeSets[0].childAttributes);
          if (!isFieldValid) {
            return false;
          }
        }
      }
      
    }
    return isFieldValid;
  }

  handleSuperParentChange() {
    // if the super question answer says that they don't have any info child questions - then remove child questions' answers/added fields etc
    if ((this.selectedCategory.attributes[0].fieldType === this.attributeFieldType.Boolean && !this.selectedCategory.attributes[0].booleanAnswer) ||
      (this.selectedCategory.attributes[0].fieldType === this.attributeFieldType.SingleCheckbox && this.selectedCategory.attributes[0].booleanAnswer)) {
      const childAttributesLength = this.selectedCategory.attributes[0].childAttributeSets.length;
      if (childAttributesLength > 1) {
        this.selectedCategory.attributes[0].childAttributeSets = this.selectedCategory.attributes[0].childAttributeSets.filter(x => x.order === 1);
      }
      this.selectedCategory.attributes[0].childAttributeSets.forEach(y => this.sanitizeAttribute(y.childAttributes));
    }
  }

  onSelect(event: TypeaheadMatch, childAttribute: PersonalFinanceAttributeAC) {
    const address = new AddressAC();
    address.streetLine = event.item.streetLine;
    address.city = event.item.city;
    address.stateAbbreviation = event.item.state;
    address.addressSuggestion = childAttribute.address.addressSuggestion;
    childAttribute.answer = JSON.stringify(address);
    childAttribute.address = address;
    
  }

  getStreetLine(childAttribute: PersonalFinanceAttributeAC) {

    if (childAttribute && childAttribute.answer) {
      const address = JSON.parse(childAttribute.answer) as AddressAC;
      return [address.primaryNumber, address.streetLine, address.streetSuffix, address.secondaryNumber, address.secondaryDesignator].join(' ');
    }
    return '';
  }

  getCity(childAttribute: PersonalFinanceAttributeAC) {
    if (childAttribute && childAttribute.answer) {
      return (JSON.parse(childAttribute.answer) as AddressAC).city;
    }
    return '';
  }

  getState(childAttribute: PersonalFinanceAttributeAC) {
    if (childAttribute && childAttribute.answer) {
      return (JSON.parse(childAttribute.answer) as AddressAC).stateAbbreviation;
    }
    return '';
  }

  getZipCode(childAttribute: PersonalFinanceAttributeAC) {
    if (childAttribute && childAttribute.answer) {
      return (JSON.parse(childAttribute.answer) as AddressAC).zipCode;
    }
    return '';
  }

  hasSubBooleanChanged(childAttribute: PersonalFinanceAttributeAC, index: number) {
    if (childAttribute.booleanAnswer) {
      childAttribute.answer = 'true';
    } else {
      childAttribute.answer = 'false';
      childAttribute.childAttributeSets.forEach(attribute => attribute.childAttributes.forEach(x => x.answer = ''));
    }

  }

  prepareAddressObservable(childAttribute: PersonalFinanceAttributeAC) {
    return new Observable((observer: Observer<string>) => {
      observer.next(childAttribute.address.streetLine);
    }).pipe(
      switchMap((token: string) => this.fetchSmartyStreetsSuggestions(token))
    );
  }

  identifierName(attributeSet: PersonalFinanceOrderedAttributeAC) {
    if (attributeSet.childAttributes) {
      attributeSet.childAttributes.sort((a, b) => a.order - b.order);
      const identifierEntries = attributeSet.childAttributes[0];
      if (identifierEntries) {
        return identifierEntries.answer;
      }
    }
    return null;
  }

  subIdentifierName(attributeSet: PersonalFinanceOrderedAttributeAC) {
    if (attributeSet.childAttributes) {
      attributeSet.childAttributes.sort((a, b) => a.order - b.order);
      const identifierEntries = attributeSet.childAttributes[1];
      if (identifierEntries && identifierEntries.fieldType === PersonalFinanceAttributeFieldType.BankAccountNumber) {
        return identifierEntries.answer;
      }
    }
    return null;
  }

  fetchSmartyStreetsSuggestions(token: string) {
    if (token) {
      return this.smartyStreetsService.fetchSuggestions(token)
        .pipe(
          map(data => data && data.result || []),
          tap(() => noop, err => {
            this.toastrService.error(Constant.someThingWentWrong);
            return of([]);
          })
        );
    } else {
      return of([]);
    }
  }

  initializeModalControls() {
    if (this.selectedCategory.name === Constant.checking) {
      this.nextModal = Constant.nextSavings;
      this.addAnotherField = Constant.addAnotherAccount;
      this.selectorName = 'Account';
    } else if (this.selectedCategory.name === Constant.savings) {
      this.nextModal = Constant.nextBrokerage;
      this.addAnotherField = Constant.addAnotherAccount;
      this.selectorName = 'Account';
    } else if (this.selectedCategory.name === Constant.brokerage) {
      this.nextModal = Constant.nextRetirement;
      this.addAnotherField = Constant.addAnotherAccount;
      this.selectorName = 'Account';
    } else if (this.selectedCategory.name === Constant.retirement) {
      this.nextModal = Constant.nextLifeInsurance;
      this.addAnotherField = Constant.addAnotherAccount;
      this.selectorName = 'Account';
    } else if (this.selectedCategory.name === Constant.lifeInsurance) {
      this.nextModal = Constant.nextReceivables;
      this.addAnotherField = Constant.addAnotherInsurance;
      this.selectorName = 'Life Insurance';
    } else if (this.selectedCategory.name === Constant.creditCards) {
      this.nextModal = Constant.nextInstallmentLoans;
      this.addAnotherField = Constant.AddAnotherCreditCard;
      this.selectorName = 'Credit Card';
    } else if (this.selectedCategory.name === Constant.installmentLoans) {
      this.nextModal = Constant.nextMortgage;
      this.addAnotherField = Constant.addAnotherInstalmentLoan;
      this.selectorName = 'Instalment Loan';
    } else if (this.selectedCategory.name === Constant.incomeInformation) {
      this.selectorName = 'Income Information';
    } else if (this.selectedCategory.name === Constant.mortgageLoans) {
      this.selectorName = 'Mortgage Loan';
      this.addAnotherField = Constant.addAnotherMortgageLoan;
      this.nextModal = Constant.nextOtherLoan;
    } else if (this.selectedCategory.name === Constant.autoMobile) {
      this.selectorName = 'Automobile';
      this.addAnotherField = Constant.addAnotherAutomobile;
      this.nextModal = Constant.nextPersonalProperty;
    } else if (this.selectedCategory.name === Constant.realEstate) {
      this.nextModal = Constant.nextAutoMobile;
      this.addAnotherField = Constant.addAnotherProperty;
      this.selectorName = 'Property';
    } else if (this.selectedCategory.name === Constant.receivables) {
      this.nextModal = Constant.nextRealEstate;
      this.addAnotherField = Constant.addAnotherReceivable;
      this.selectorName = 'Receivable';
    } else if (this.selectedCategory.name === Constant.otherLoans) {
      this.nextModal = Constant.nextUnpaidTaxes;
      this.addAnotherField = Constant.addAnotherLoan;
      this.selectorName = 'Loan';
    } else if (this.selectedCategory.name === Constant.unpaidTaxes) {
      this.addAnotherField = Constant.addAnotherTax;
      this.selectorName = 'Obligation';
    } else if (this.selectedCategory.name === Constant.personalProperty) {
      this.addAnotherField = Constant.addAnotherProperty;
      this.selectorName = 'Property';
    }
  }

  jumpToParentCategory(parentCategory: PersonalFinanceCategoryAC) {
    this.fillParentEvent.emit(parentCategory);
  }
}
