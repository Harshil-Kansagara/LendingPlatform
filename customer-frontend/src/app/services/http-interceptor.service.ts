import { Location } from '@angular/common';
import { Injectable, Injector } from '@angular/core';
import { HttpInterceptor, HttpBackend, HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HttpRequest } from '@angular/common/http';
import { HttpHandler } from '@angular/common/http';
import { HttpEvent } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable, from, noop } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Constant } from '../shared/constant';
import { ToastrService } from 'ngx-toastr';
import { AppService } from './app.service';

@Injectable()
export class HttpInterceptorService implements HttpInterceptor {
    constructor(private injector: Injector) {

    }
    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        // intercept if there is bearer token in local storage
        let toastr = this.injector.get(ToastrService);
        let appService = this.injector.get(AppService);
        if (localStorage.getItem('access_token') != null) {

            return from(this.handleAccess(request, next, toastr, appService));
        }
        else {
            // Ignoring interceptor
            return this.handlErrors(next, request, toastr, appService);
        }
    }

    private async handleAccess(request: HttpRequest<any>, next: HttpHandler, toastrService: ToastrService, appService: AppService):
        Promise<HttpEvent<any>> {

      if (request.url.indexOf(Constant.amazonAWS) >= 0) {
        return this.handlErrors(next, request, toastrService, appService).toPromise();
      } else {
        let changedRequest = request;
        // HttpHeader object immutable - copy values
        const headerSettings: { [name: string]: string | string[]; } = {};

        for (const key of request.headers.keys()) {
          headerSettings[key] = request.headers.getAll(key);
        }

        let token = localStorage.getItem("access_token");
        if (token) {
          headerSettings['Authorization'] = 'Bearer ' + token;
        }
        if (!request.url.includes('document')) {
          headerSettings['Content-Type'] = 'application/json';
        }
        const newHeader = new HttpHeaders(headerSettings);

        changedRequest = request.clone({
          headers: newHeader
        });

        return this.handlErrors(next, changedRequest, toastrService, appService).toPromise();
      }

        
    }

    private handlErrors(next: HttpHandler, changedRequest: HttpRequest<any>, toastrService: ToastrService, appService: AppService) {
        return next.handle(changedRequest).pipe(
            tap(event =>
                noop
                , exception => {
                    if (exception instanceof HttpErrorResponse) {
                        if (exception.status <= 500) {
                            switch (exception.status) {

                                case Constant.unauthorized:
                                    toastrService.error(Constant.tokenExpired);
                                    appService.logoff();
                                    break;

                                case Constant.notFound:
                                    toastrService.error(Constant.noRecordFound);
                                    break;

                                case Constant.timeout:
                                    toastrService.error(Constant.timedOut);
                                    break;

                                case Constant.forbidden:
                                    toastrService.error(Constant.forbiddenServerRefused);
                                    break;

                                case Constant.internalServerError:
                                    toastrService.error(Constant.serverError);
                                    break;
                            }
                        }
                        else {
                            toastrService.error(Constant.serverError);
                        }
                    }
                })
        );
    }
}
