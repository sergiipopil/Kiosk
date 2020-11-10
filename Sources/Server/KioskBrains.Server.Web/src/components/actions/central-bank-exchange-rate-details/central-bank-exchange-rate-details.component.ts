import { Component } from '@angular/core';
import { HttpService } from 'services/http.service';
import { CommonDetailsGetRequest } from 'models/base/common-details-get-request';
import { NavigationService } from 'services/navigation.service';
import { BaseDetailsComponent } from 'components/base-details/base-details.component';
import { PortalCentralBankExchangeRateDetailsGetResponse } from 'models/api/api';

@Component({
  selector: 'central-bank-exchange-rate-details',
  templateUrl: 'central-bank-exchange-rate-details.component.html'
})
export class CentralBankExchangeRateDetailsComponent extends BaseDetailsComponent<PortalCentralBankExchangeRateDetailsGetResponse, CommonDetailsGetRequest> {
  constructor(public http: HttpService, public navService: NavigationService) {
    super(http, navService);
    this.actionName = 'portal-central-bank-exchange-rate-details';
  }
};
