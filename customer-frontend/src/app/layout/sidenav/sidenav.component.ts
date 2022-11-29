import { Component, OnInit } from '@angular/core';
import { AppService } from '../../services/app.service';
import { Router, Event, NavigationEnd, NavigationStart } from '@angular/router';
import { Constant } from '../../shared/constant';
import { ToastrService } from 'ngx-toastr';
import { EntityAC, SectionAC } from '../../utils/serviceNew';

@Component({
  selector: 'app-sidenav',
  templateUrl: './sidenav.component.html',
  styleUrls: ['./sidenav.component.scss']
})
export class SidenavComponent implements OnInit {
  overflow = false;
  logoImage: string;
  constructor(readonly appService: AppService,
    private readonly router: Router,
    private readonly toastrService: ToastrService) {
    this.appService.overflow.subscribe(val => this.overflow = val);
  }
  // Method to show/hide sidenav
  toggleSidenav(){
    this.overflow = false;
  }
 
  navLinks = [];

  //Section names
  loanNeeds: string = Constant.loanNeeds;
  loanNeedsConstant = Constant.loanNeedsProgressBar;
  progress;
  isCollapsed: boolean;
  isViewOnlyMode: boolean;
  isCompanyEditable: boolean;
  companyInformation = Constant.companyInfo;
  finances = Constant.finances;

  currentUser = new EntityAC();
  currentUserName: string;
  currentUserEmail: string;
  loanApplicationNumber: string;
  isCurrentLoanSectionFound: boolean;
  async ngOnInit() {
    const selectedTheme = await this.appService.getSelectedTheme();
    if (selectedTheme) {
      this.logoImage = selectedTheme.logoUrl;
    }
    this.setSections();
    this.currentUser = await this.appService.getCurrentUserDetailsNew();

    if (!this.currentUser || (this.currentUser && !this.currentUser.id)) {
      this.appService.currentUserName.subscribe(val => this.currentUserName = val);
      this.appService.currentUserEmail.subscribe(val => this.currentUserEmail = val);
    } else {
      this.currentUserName = (`${this.currentUser.user.firstName} ${this.currentUser.user.lastName}`);
      this.currentUserEmail = this.currentUser.user.email;
    }

    this.loanApplicationNumber = await this.appService.getCurrentLoanApplicationNumber();

    this.appService.companyEditMode.subscribe(val => this.isCompanyEditable = val);
    const currentCompany = await this.appService.getCurrentCompanyDetailsNew();
    if (currentCompany) {
      if (currentCompany.company?.createdByUserId === this.currentUser.id) {
        this.isCompanyEditable = true;
        this.appService.changeCompanyMode(true);
      } else {
        this.isCompanyEditable = false;
        this.appService.changeCompanyMode(false);
      }
    }

    this.appService.currentMode.subscribe(val => this.isViewOnlyMode = val);
    this.isViewOnlyMode = await this.appService.isViewOnlyMode();
    this.appService.changeMode(this.isViewOnlyMode);

    /* Method To keep financial menu open for statement & taxes route*/
    this.router.events.subscribe(async (event: Event) => {
      this.loanApplicationNumber = await this.appService.getCurrentLoanApplicationNumber();

      if (event instanceof NavigationEnd) {
        this.progress = await this.appService.getCurrentProgress();
        this.setSections();
      }
      if (event instanceof NavigationStart) {
        /* To Update progressbar on each component */
        this.progress = await this.appService.getCurrentProgress();
      }
    });
    this.progress = await this.appService.getCurrentProgress();
  }

  /**
   * Method to set the sections in navlink array.
   * */
  async setSections() {
    const sections = await this.appService.getSectionConfigurations();
    
    //Add credit profile section in the section list.
    const creditProfileSection = new SectionAC();
    creditProfileSection.id = '';
    creditProfileSection.name = 'Credit Profile';
    creditProfileSection.order = 0;
    sections.push(creditProfileSection);

    sections.sort(this.appService.sortByAnyIntegerField);
    
      
    // Find current loan section
    let currentLoanSection = await this.appService.getCurrentSectionName();
    currentLoanSection = currentLoanSection !== null ? currentLoanSection : this.loanNeeds;

    // Find section on which route is navigated (clicked)
    const currentSectionNavigatedTo = Constant.loanRouteSectionMapping.filter(x => this.router.url.includes(x.Route));

    // Fill the navlink array
    if (this.navLinks.length === 0) {
      let isCurrentSectionFound = currentSectionNavigatedTo.some(x=>x.IsChild);
      for (const newSection of sections) {
        const redirectURL = Constant.loanRouteSectionMapping.filter(x => x.Section === newSection.name);
        if (redirectURL.length > 0) {
          isCurrentSectionFound = this.fillNavLinkArray(currentLoanSection, redirectURL, newSection, isCurrentSectionFound);
        }

      }
    }

    // Find sub sections
    const parentSections = this.navLinks.filter(x => x.childSection && x.childSection.length > 0);
    const childSections = new Array();
    for (const parent of parentSections) {
      for (const child of parent.childSection) {
        child.parentName = parent.name;
        childSections.push(child);
      }
    }

    // Prepare nav link array for section and subsection for proper showing of active/edit-mode as per loan
    if (sections.length !== 0 && currentSectionNavigatedTo.length > 0) {
      this.isCurrentLoanSectionFound = false;

      this.prepareNavbarForParentSection(currentSectionNavigatedTo, currentLoanSection, childSections);

      this.prepareNavbarForSubSections(parentSections, currentSectionNavigatedTo, currentLoanSection);
      
    }
    
  }

  // Prepare nav bar for parent sections
  prepareNavbarForParentSection(currentSectionNavigatedTo, currentLoanSection, childSections) {
    for (const currentNav of this.navLinks) {
      if (!this.isCurrentLoanSectionFound) {
        this.applyClassOnNavSection(currentNav, currentSectionNavigatedTo, currentLoanSection, childSections);
      } else {
        currentNav.editMode = false;
        currentNav.active = false;
      }
    }
  }

  // Prepare nav bar for sub sections
  async prepareNavbarForSubSections(parentSections,currentSectionNavigatedTo,currentLoanSection) {
    for (const parent of parentSections) {
      this.isCurrentLoanSectionFound = false;
      parent.childSection.sort(this.appService.sortByAnyIntegerField);
      for (const section of parent.childSection) {
        
        const redirectURL = Constant.loanRouteSectionMapping.filter(x => x.Section === section.name)[0];
        if (redirectURL) {
          section.redirectUrl = redirectURL.Route;
          if (!this.isCurrentLoanSectionFound) {
            this.applyClassOnNavSection(section, currentSectionNavigatedTo, currentLoanSection, section.childSection);
            if (!parent.editMode && !parent.active) {
              section.editMode = false;
              section.active = false;
            }
          }else {
            section.editMode = false;
            section.active = false;
          }

          if (!this.router.url.includes('statements') && await this.appService.isFinanceInProgress()) {
            parent.childSection.filter(x => x.redirectUrl === Constant.financesRedirectUrl)[0].data = 'processing';
          } else if (!this.router.url.includes('statements') && !await this.appService.isFinanceInProgress()
            && parent.childSection.filter(x => x.redirectUrl === Constant.financesRedirectUrl)[0]?.data === 'processing') {
            parent.childSection.filter(x => x.redirectUrl === Constant.financesRedirectUrl)[0].data = 'completed';
          } else if (this.router.url.includes('statements')) {
            const anyNavHavingProcessingData = parent.childSection.filter(x => x.data === 'processing' || x.data === 'completed');
            for (let navLink of anyNavHavingProcessingData) {
              navLink.data = null;
            }
          }

        }
      }
    }
  }

  // Apply edit mode or active class on section
  applyClassOnNavSection(currentNav,currentSectionNavigatedTo,currentLoanSection,childSections) {
    if (currentSectionNavigatedTo.some(x => x.Section === currentNav.name)) {
      currentNav.editMode = false;
      currentNav.active = true;
    } else{
      currentNav.editMode = true;
      currentNav.active = false;
    }
    if (currentNav.name === currentLoanSection ||
      (childSections && childSections.filter(x => x.parentName === currentNav.name).some(x => x.name === currentLoanSection))) {
      this.isCurrentLoanSectionFound = true;
    }
  }

  /**
   * Method to fill the nav links in array with its properties.
   * */
  fillNavLinkArray(currentLoanSection: string, redirectURL: { Route: string, Section: string }[],
    section: SectionAC, isCurrentSectionFound: boolean): boolean {
    if (section.name === currentLoanSection) {
      isCurrentSectionFound = true;
      this.navLinks.push({ name: section.name, editMode: false, active: true, childSection:section.childSection, redirectUrl: redirectURL[0].Route });
    } else if (!isCurrentSectionFound) {
      this.navLinks.push({ name: section.name, editMode: true, active: false, childSection:section.childSection, redirectUrl: redirectURL[0].Route });
    } else {
      this.navLinks.push({ name: section.name, editMode: false, active: false, childSection:section.childSection, redirectUrl: redirectURL[0].Route });
    }
    return isCurrentSectionFound;
  }

  

  /**
   * Methot to redirect user to landing page.
   * */
  goHome() {
    this.router.navigate(['']);
  }

  /**
   * Methot to log out the current user.
   * */
  logOff() {
    this.appService.logoff();
  }
}
