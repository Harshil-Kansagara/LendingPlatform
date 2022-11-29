import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppService } from '../services/app.service';
import { FilterAC, EntityService, GlobalService, EntityAC, LoanApplicationStatusType } from '../utils/serviceNew';
import { Constant } from '../shared/constant';

@Component({
  selector: 'app-loan',
  templateUrl: './loan.component.html',
  styleUrls: ['./loan.component.scss']
})
export class LoanComponent implements OnInit {

  isLoggedIn = false;
  currentUserId: string;
  isRendered = false;

  constructor(private readonly route: ActivatedRoute,
    private readonly appService: AppService,
    private readonly entityService: EntityService,
    private readonly router: Router,
    private readonly globalService: GlobalService) { }

  ngOnInit(): void {
    this.globalService.getConfigurations().subscribe(
      res => {
        this.appService.setConfigurations(res);
        const token = localStorage.getItem('access_token');
        this.route.queryParams.subscribe(params => {
          const code = params['code'];

          // When there is no redirection from identity server
          if (!code) {
            this.appService.isAuthenticated().subscribe(async isAuthorized => {
              if (isAuthorized && token) {
                await this.appService.removeAllDataFromLocalForage();
                this.isLoggedIn = true;

                const userInLocalForage = await this.appService.getCurrentUserDetailsNew();
                //If current user details is not saved in localForage then fetch it and save it.
                if (!userInLocalForage || (userInLocalForage && !userInLocalForage.id)) {

                  const filterAC = new FilterAC();
                  filterAC.field = 'type';
                  filterAC.operator = '=';
                  filterAC.value = 'people';
                  const filters = new Array<FilterAC>();
                  filters.push(filterAC);

                  this.entityService.getEntityList(null, null, null, JSON.stringify(filters), null, null).subscribe(
                    async result => {
                      //Check if the user has credit score details, otherwise
                      const user = await this.appService.getCurrentUserDetailsNew();
                      if ((result[0].user && result[0].user.selfDeclaredCreditScore === null) && (user && user.user && user.user.selfDeclaredCreditScore !== null)) {
                        result[0].user.hasAnyJudgementsSelfDeclared = user.user.hasAnyJudgementsSelfDeclared;
                        result[0].user.hasBankruptcySelfDeclared = user.user.hasBankruptcySelfDeclared;
                        result[0].user.selfDeclaredCreditScore = user.user.selfDeclaredCreditScore;
                      }

                      //Set the current user details and name.
                      await this.appService.setCurrentUserDetailsNew(result[0]);
                      this.appService.setCurrentUserName(`${result[0].user.firstName} ${result[0].user.lastName}`);
                      this.appService.setCurrentUserEmail(result[0].user.email);
                      this.currentUserId = result[0].id;
                      this.isHavingLoanOrNot();
                    },
                    err => {
                    });
                } else {
                  this.isHavingLoanOrNot();
                }
              } else {
                this.isRendered = true;
              }
            });
          }
        });
      },
      err => {
      });
  }

  /**
   * Method to check whethe the user has any loan applied.
   * If yes then redirect to that loan's last section.
   * If not then redirect to loan needs section for new loan.
   * */
  async isHavingLoanOrNot() {
    const currentUser = await this.appService.getCurrentUserDetailsNew();
    if (currentUser) {
      this.currentUserId = currentUser.id;
      this.isRendered = true;

      //Set the eligibility based on credit profile.
      this.entityService.addUserCreditProfile(currentUser).subscribe(
        async res => {
          
          this.checkLoanListAndRedirect(currentUser, res);
        },
        err => {
        }
      );
    }
  }

  /**
   * Method will fetch the loan list and redirect the current user accordingly.
   */
  async checkLoanListAndRedirect(currentUser: EntityAC, isCreditOkay:boolean) {

    this.entityService.getLoanApplicationListByEntityId(this.currentUserId).subscribe(
      async res => {
        const loanApplicationList = res.filter(x => x.status === LoanApplicationStatusType.Draft);

        //If any loan is present then redirect to that
        if (loanApplicationList.length !== 0) {
          loanApplicationList.sort((a, b) => (a.lastUpdatedOn <= b.lastUpdatedOn) ? -1 : 1 );
          if(loanApplicationList[0].mappedEntityId !== Constant.guidEmptyString){
            const filterAC = new FilterAC();
            filterAC.field = 'type';
            filterAC.operator = '=';
            filterAC.value = 'company';
            const filters = new Array<FilterAC>();
            filters.push(filterAC);
      
            this.entityService.getEntityList(null, null, null, JSON.stringify(filters), null, null).subscribe(
              entity => {
                const mappedEntity = entity.find(x => x.id === loanApplicationList[0].mappedEntityId);
                if(mappedEntity){
                  this.appService.setCurrentCompanyId(mappedEntity.id);
                }
              }, err => {}
            );
          }
          this.appService.openLoanApplication(loanApplicationList[0], isCreditOkay);
          
          //otherwise check for the credit score
        } else if (currentUser.user.selfDeclaredCreditScore === null) {
          await this.appService.setIsCreditOkay(false);
          this.router.navigate([Constant.creditProfileRedirectUrl]);

          //and then to check if call is for calculator
        } else if (await this.appService.getOpenCalculator()) {
          this.router.navigate([Constant.calculatorRedirectUrl]);

          //and at last redirect to loan needs for new loan
        } else {
          await this.appService.setViewOnlyMode(false);
          this.router.navigate([Constant.loanNeedsRedirectUrl]);
        }
      },
      err => {
      });
  }
}

