import { Component } from '@angular/core';
import { HttpService } from 'services/http.service';
import { Field } from 'models/field';
import { BaseSearchComponent } from 'components/base-search/base-search.component';
import { NavigationService } from 'services/navigation.service';
import { PortalKioskStateSearchGetRequest, PortalKioskStateSearchGetResponse, ComponentStatusCodeEnum } from 'models/api/api';
import { KioskStatusDecorator } from './kiosk-status-decorator';

@Component({
  selector: 'kiosk-state-search',
  templateUrl: 'kiosk-state-search.component.html',
  styleUrls: ['kiosk-state-search.component.scss']
})
export class KioskStateSearchComponent extends BaseSearchComponent<PortalKioskStateSearchGetResponse, PortalKioskStateSearchGetRequest> {
  fields: Field[];
  showAdvanced: boolean;
  kioskStates: KioskStatusDecorator[];

  constructor(public http: HttpService, public navService: NavigationService) {
    super(http, navService);
    this.requestModel = new PortalKioskStateSearchGetRequest();
    this.viewModel = new PortalKioskStateSearchGetResponse();
    this.fields = new Array<Field>();
    this.actionName = 'portal-kiosk-state-search';
    this.fields.push({ key: 'Id', displayName: 'ID', sortable: false });
    this.fields.push({ key: 'Address', displayName: 'Address', sortable: false });
    this.fields.push({ key: 'State', displayName: 'State', sortable: false });
    this.fields.push({ key: 'Cash', displayName: 'Cash', sortable: false });
    this.fields.push({ key: 'Details', displayName: 'Details', sortable: false });

    // update line collections on GET responses
    this.viewModelChange.subscribe(x => this.initKioskStateDecoratorCollection());
  }

  initKioskStateDecoratorCollection() {
    this.kioskStates = [];
    for (let record of this.viewModel.records) {
      const decorator = new KioskStatusDecorator(record);
      this.kioskStates.push(decorator);
    }
  }

  getComponentStatusName(status: ComponentStatusCodeEnum): string {
    return status == ComponentStatusCodeEnum.Ok
      ? 'Ok'
      : status == ComponentStatusCodeEnum.Warning
        ? 'Warning'
        : status == ComponentStatusCodeEnum.Error
          ? 'Error'
          : status == ComponentStatusCodeEnum.Disabled
            ? 'Disabled'
            : status == ComponentStatusCodeEnum.Undefined
              ? 'Undefined'
              : null;
  }

  isOkComponentStatus(status: ComponentStatusCodeEnum) : boolean {
    return status == ComponentStatusCodeEnum.Ok;
  }

}
