import { Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { AppService } from '../../services/app.service';
import { Constant } from '../../shared/constant';
import {ApplicationService, LoanApplicationStatusType } from '../../utils/serviceNew';

@Component({
  selector: 'app-loan-status',
  templateUrl: './loan-status.component.html',
  styleUrls: ['./loan-status.component.scss']
})
export class LoanStatusComponent implements OnInit {
  constructor(private readonly appService: AppService, private readonly applicationService: ApplicationService) {
    this.appService.updateRoute(Constant.loanConsentRedirectUrl);
    this.appService.updateLoader(false);
  }
  loanConsentRedirectUrl = Constant.loanConsentRedirectUrl;
  // Loan issues data
  issueList = [];
  loanStatus: string = LoanApplicationStatusType[LoanApplicationStatusType.Referral.toString()];
  isCollapsed = true;
  
  
  proceedToBankDetailsUrl: string = Constant.bankDetailsRedirectUrl;
  ifloanProcess = Constant.ifloanProcess;
  ifloanSuccess = Constant.ifloanSuccess;
  ifloanFailed = Constant.ifloanFailed;
  ifloanEvaluation = Constant.ifloanEvaluation;
  supportTitle = Constant.supportTitle;
  supportMail = Constant.supportMail;
  supportPhone = Constant.supportPhone;
  ifloanEvaluationSubline = Constant.ifloanEvaluationSubline;
  async ngOnInit() {
    const timedSubscription = timer(0, Constant.resetTimer).pipe(
      switchMap(_ => this.appService.getCurrentLoanApplicationStatus())
    ).subscribe(async res => {
      if (res !== LoanApplicationStatusType.Draft && res !== LoanApplicationStatusType.Unlocked) {
        timedSubscription.unsubscribe();
        this.applicationService.evaluateLoanAndGetLoanStatus(await this.appService.getCurrentLoanApplicationId())
          .subscribe(loan => {
            this.loanStatus = LoanApplicationStatusType[loan.status.toString()];
            this.issueList = JSON.parse(loan.evaluationComments);
            this.appService.setCurrentLoanApplicationStatus(loan.status);
          });
      }
    });
  }
}
