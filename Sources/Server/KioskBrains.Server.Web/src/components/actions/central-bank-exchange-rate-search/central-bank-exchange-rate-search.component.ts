import { Component } from '@angular/core';
import { HttpService } from 'services/http.service';
import { Field } from 'models/field';
import { BaseSearchComponent } from 'components/base-search/base-search.component';
import { NavigationService } from 'services/navigation.service';
import { PortalCentralBankExchangeRateSearchGetRequest, PortalCentralBankExchangeRateSearchGetResponse } from 'models/api/api';

@Component({
  selector: 'central-bank-exchange-rate-search',
  templateUrl: 'central-bank-exchange-rate-search.component.html'
})
export class CentralBankExchangeRateSearchComponent extends BaseSearchComponent<PortalCentralBankExchangeRateSearchGetResponse, PortalCentralBankExchangeRateSearchGetRequest> {
  fields: Field[];
  showAdvanced: boolean;

  constructor(public http: HttpService, public navService: NavigationService) {
    super(http, navService);
    this.requestModel = new PortalCentralBankExchangeRateSearchGetRequest();
    this.viewModel = new PortalCentralBankExchangeRateSearchGetResponse();
    this.fields = new Array<Field>();
    this.actionName = 'portal-central-bank-exchange-rate-search';
    this.fields.push({ key: 'Details', displayName: 'Details', sortable: false });
    this.fields.push({ key: 'LocalCurrencyCode', displayName: 'Local Currency', sortable: false });
    this.fields.push({ key: 'ForeignCurrencyCode', displayName: 'Foreign Currency', sortable: false });
    this.fields.push({ key: 'Rate', displayName: 'Base Rate', sortable: false });
    this.fields.push({ key: 'DefaultOrder', displayName: 'Default Order', sortable: false });
  }
}
