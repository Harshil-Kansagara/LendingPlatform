import { Component, Input, OnInit } from '@angular/core';
import { Constant } from '../constant';
import { Router } from '@angular/router';
import { AppService } from '../../services/app.service';
import { ApplicationService } from '../../utils/serviceNew';

@Component({
  selector: 'app-coming-soon',
  templateUrl: './coming-soon.component.html',
  styleUrls: ['./coming-soon.component.scss']
})
export class ComingSoonComponent implements OnInit {

  comingSoonTitile = Constant.comingSoonTitile;
  currentSectionName: string;
  @Input() btnRoute: string;
  @Input() hideBtn: boolean;
  constructor(private readonly router: Router,
    private readonly appService: AppService,
    private readonly applicationService: ApplicationService) { }

  ngOnInit(): void {
    
  }

  async redirect() {
    this.currentSectionName = await this.appService.getCurrentSectionName();
    if (this.currentSectionName === Constant.personal || this.currentSectionName === Constant.additionalDocuments) {
      this.applicationService.updateCurrentSectionName(await this.appService.getCurrentLoanApplicationId(), this.currentSectionName).subscribe(
        async (updatedSectionName: string) => {
          if (updatedSectionName != null) {
            await this.appService.updateCurrentSectionName(updatedSectionName);
            this.router.navigate([this.btnRoute]);
          }
        }
      );
    } else {
      this.router.navigate([this.btnRoute]);
    }

    
  }
}
