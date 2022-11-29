import { Injectable } from '@angular/core';
import { CanActivate, Router, CanActivateChild } from '@angular/router';
import { AppService } from './app.service';
import { Observable } from 'rxjs';
import { Constant } from '../shared/constant';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate, CanActivateChild {
  constructor(private readonly appService: AppService,
    private readonly router: Router) { }

  canActivate(): Observable<boolean> {
    return this.appService.isAuthenticated().pipe(
      map((res: boolean) => {

        // If user is authenticated then only check the credit score is okay or not.
        if (res) {
          this.appService.getIsCreditOkay().then(async (isAllowed: boolean) => {
            if (isAllowed !== null) {

              const currentLoanInViewOnlyMode = await this.appService.isViewOnlyMode();

              //If user is redirected to consent page to give consent then doesn't require credit score to be okay.
              if (isAllowed || currentLoanInViewOnlyMode) {
                return true;
              } else {
                this.router.navigate([Constant.creditProfileRedirectUrl]);
              }
            }
            return res;
          });
        }
        return res;
      }));
  }

  canActivateChild(): Observable<boolean> {
    return this.appService.isAuthenticated();
  }
}
