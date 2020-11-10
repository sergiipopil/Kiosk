import { Component } from '@angular/core';
import { HttpService } from 'services/http.service';
import { Field } from 'models/field';
import { BaseSearchComponent } from 'components/base-search/base-search.component';
import { NavigationService } from 'services/navigation.service';
import { PortalLogRecordSearchGetRequest, PortalLogRecordSearchGetResponse} from 'models/api/api';

@Component({
  selector: 'log-record-search',
  templateUrl: 'log-record-search.component.html'
})
export class LogRecordSearchComponent extends BaseSearchComponent<PortalLogRecordSearchGetResponse, PortalLogRecordSearchGetRequest> {
  fields: Field[];
  showAdvanced: boolean;

  constructor(public http: HttpService, public navService: NavigationService) {
    super(http, navService);
    this.requestModel = new PortalLogRecordSearchGetRequest();
    this.viewModel = new PortalLogRecordSearchGetResponse();
    this.fields = new Array<Field>();
    this.actionName = 'portal-log-record-search';
    this.fields.push({ key: 'Type', displayName: 'Type', sortable: true });
    this.fields.push({ key: 'KioskId', displayName: 'Kiosk ID', sortable: true });
    this.fields.push({ key: 'LocalTime', displayName: 'Local Time', sortable: true });
    this.fields.push({ key: 'Context', displayName: 'Context', sortable: true });
    this.fields.push({ key: 'Message', displayName: 'Message', sortable: true });
    this.fields.push({ key: 'AdditionalDataJson', displayName: 'Additional Data', sortable: false });
  }
}
