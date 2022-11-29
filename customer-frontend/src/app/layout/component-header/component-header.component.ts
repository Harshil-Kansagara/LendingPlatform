import { Component, Input, OnInit } from '@angular/core';
import { Router,Event,NavigationEnd, NavigationStart } from '@angular/router';
import { AppService } from '../../services/app.service';
import { EventEmitterService } from '../../services/event-emitter.service';
import { ToastrService } from 'ngx-toastr';
import { Constant } from '../../shared/constant';
@Component({
  selector: 'app-component-header',
  templateUrl: './component-header.component.html',
  styleUrls: ['./component-header.component.scss']
})
export class ComponentHeaderComponent implements OnInit {
  logoImage: string;
  quickbookIcon = 'assets/images/quickbook-icon@2x.png';
  progress;
  overflow:boolean;
  @Input() title:string;
  @Input() backRoute;
  @Input() hideMenuBtn: boolean;
  @Input() secureLable = true;
  @Input() heading = true;
  @Input() connectedService: string;
  
  constructor(private readonly appService: AppService,
    private readonly router: Router,
    private readonly eventEmitterService: EventEmitterService,
    private readonly toastr: ToastrService) { }

  // Method to show/hide sidenav
  toggleSidenav(){
    this.appService.toggleSidenav(true);
  }
  // Method for previous route
  goBack(){
    this.router.navigate([this.backRoute]);
  }
  async ngOnInit() {
    const selectedTheme = await this.appService.getSelectedTheme();
    if (selectedTheme) {
      this.logoImage = selectedTheme.logoUrl;
    }

    
    /* Method To keep financial menu open for statement & taxes route*/
    this.router.events.subscribe(async (event: Event) => {
      

      if (event instanceof NavigationEnd) {

        this.progress = await this.appService.getCurrentProgress();
      }
      if (event instanceof NavigationStart) {
        /* To Update progressbar on each component */
        this.progress = await this.appService.getCurrentProgress();
      }
    });
    this.progress = await this.appService.getCurrentProgress();
  }

  // Method called on click of clear button in finances
  async clearFinances() {
    if (await this.appService.getLockedApplicationJsonAsObject()) {
      this.toastr.error(Constant.loanIsLocked);
    }else {
      if (!await this.appService.isFinanceInProgress()) {
        this.eventEmitterService.resetFinances();
      }else {
        this.toastr.error(Constant.financeInProgress);
      }
      
    }
  }
}
