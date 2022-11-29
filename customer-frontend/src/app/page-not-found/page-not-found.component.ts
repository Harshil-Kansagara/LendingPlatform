import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
    selector: 'app-not-found',
    templateUrl: './page-not-found.component.html',
    styleUrls: ['./page-not-found.component.scss']
})
export class PageNotFoundComponent {
  image404 = 'assets/images/404_Error_Page.svg';

  constructor(private readonly router: Router) { }

  redirectToHome() {
    this.router.navigate(['']);
  }
}
