import { Component, OnInit, Input } from '@angular/core';
import { HttpService } from 'services/http.service';
import { Field } from 'models/field';
import { BaseSearchComponent } from 'components/base-search/base-search.component';
import { NavigationService } from 'services/navigation.service';
import { BroadcastService } from 'services/broadcast.service';
import { PortalCentralBankExchangeRateUpdateLinkedSearchGetRequest, PortalCentralBankExchangeRateUpdateLinkedSearchGetResponse } from 'models/api/api';
import { CentralBankExchangeRateUpdateChangedEventName } from 'models/broadcast-events';
import { SearchRequestMetadata } from 'models/base/search-request-metadata';

@Component({
  selector: 'central-bank-exchange-rate-update-linked-search',
  templateUrl: './central-bank-exchange-rate-update-linked-search.component.html'
})
export class CentralBankExchangeRateUpdateLinkedSearchComponent extends BaseSearchComponent<PortalCentralBankExchangeRateUpdateLinkedSearchGetResponse, PortalCentralBankExchangeRateUpdateLinkedSearchGetRequest> {
  fields: Field[];

  constructor(public http: HttpService, public navService: NavigationService, private broadcastService: BroadcastService) {
    super(http, navService);

    this.requestModel = new PortalCentralBankExchangeRateUpdateLinkedSearchGetRequest();
    this.viewModel = new PortalCentralBankExchangeRateUpdateLinkedSearchGetResponse();

    this.actionName = 'portal-central-bank-exchange-rate-update-linked-search';
    this.fields = new Array<Field>();
    this.fields.push({ key: 'Details', displayName: 'Details', sortable: false });
    this.fields.push({ key: 'StartTime', displayName: 'Start Time', sortable: false });
    this.fields.push({ key: 'Rate', displayName: 'Rate', sortable: false });

    broadcastService.getEvents([CentralBankExchangeRateUpdateChangedEventName]).subscribe(x => this.doRequest());
  }

  prepareRequest: Function = () => {
    this.setDefaultSorting();
  };

  setDefaultSorting() {
    if (!this.requestModel.metadata) {
      this.requestModel.metadata = new SearchRequestMetadata();
    }

    if (!this.requestModel.metadata.orderBy) {
      this.requestModel.metadata.orderBy = 'StartTime';
      this.requestModel.metadata.orderDirection = 1;
    }
  }

}
