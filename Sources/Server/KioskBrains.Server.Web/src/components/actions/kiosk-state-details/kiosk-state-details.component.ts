import { Component } from '@angular/core';
import { HttpService } from 'services/http.service';
import { CommonDetailsGetRequest } from 'models/base/common-details-get-request';
import { NavigationService } from 'services/navigation.service';
import { BaseDetailsComponent } from 'components/base-details/base-details.component';
import { PortalKioskStateDetailsGetResponse } from 'models/api/api';
import { KioskStatusDecorator } from '../kiosk-state-search/kiosk-status-decorator';

@Component({
  selector: 'kiosk-state-details',
  templateUrl: './kiosk-state-details.component.html',
  styleUrls: ['./kiosk-state-details.component.scss']
})
export class KioskStateDetailsComponent extends BaseDetailsComponent<PortalKioskStateDetailsGetResponse, CommonDetailsGetRequest> {
  kioskState: KioskStatusDecorator;

  constructor(public http: HttpService, public navService: NavigationService) {
    super(http, navService);
    this.actionName = 'portal-kiosk-state-details';

    this.viewModelChange.subscribe(x => this.initKioskStateDecorator());
  }

  initKioskStateDecorator() {
    this.kioskState = new KioskStatusDecorator(this.viewModel.form);
  }
};
