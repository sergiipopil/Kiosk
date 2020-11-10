import { Component } from '@angular/core';
import { HttpService } from 'services/http.service';
import { CommonDetailsGetRequest } from 'models/base/common-details-get-request';
import { NavigationService } from 'services/navigation.service';
import { BroadcastService } from 'services/broadcast.service';
import { ModalService } from "services/modal.service";
import { ConfirmMessageComponent } from "components/actions/confirm-message/confirm-message.component";
import { BaseDetailsComponent } from 'components/base-details/base-details.component';
import { PortalCentralBankExchangeRateUpdateDetailsGetRequest, PortalCentralBankExchangeRateUpdateDetailsGetResponse } from 'models/api/api';
import { CentralBankExchangeRateUpdateChangedEventName } from 'models/broadcast-events';

@Component({
  selector: 'central-bank-exchange-rate-update-details',
  templateUrl: './central-bank-exchange-rate-update-details.component.html'
})
export class CentralBankExchangeRateUpdateDetailsComponent extends BaseDetailsComponent<PortalCentralBankExchangeRateUpdateDetailsGetResponse, PortalCentralBankExchangeRateUpdateDetailsGetRequest> {

  timeRegEx = /^([[0-1]?\d|2[0-3]):[0-5][0-9]:[0-5][0-9]$/;

  constructor(public http: HttpService, public navService: NavigationService, private _broadcastService: BroadcastService, public modalService: ModalService) {
    super(http, navService);
    this.actionName = 'portal-central-bank-exchange-rate-update-details';
    this.onProcessSaveSuccess.subscribe(() => {
      this._broadcastService.next({ name: CentralBankExchangeRateUpdateChangedEventName });
    });
  }

  delete() {
    this.showConfirmationPopup(() => this.doDelete(this.viewModel.form.id));
  }

  doDelete(id: number) {
    this.isLoading = true;
    this.http.delete('portal-central-bank-exchange-rate-update-details?request=' + encodeURIComponent(JSON.stringify({ id }))).subscribe(val => {
      this.isLoading = false;
      if (val) {
        this._broadcastService.next({ name: CentralBankExchangeRateUpdateChangedEventName });
        this.exit();
      }
    });
  }

  showConfirmationPopup(yes: Function) {
    this.modalService.addModal({
      component: ConfirmMessageComponent,
      modalConfig: undefined,
      params: {
        header: "Warning",
        message: "Are you sure you want to delete the rate update?",
        yes: () => {
          this.modalService.removeModal();
          if (yes) {
            yes();
          }
        },
        no: () => {
          this.modalService.removeModal();
        }
      }
    });
  }
};
