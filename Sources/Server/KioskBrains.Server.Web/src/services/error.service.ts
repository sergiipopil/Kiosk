import { Headers, Http, RequestOptionsArgs } from '@angular/http';
import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { LoaderService } from './loader.service';
import { ReplaySubject, AsyncSubject } from 'rxjs';
import { ModalService } from './modal.service';
import { ComponentHistoryEntry } from 'models/component-history-entry';
import { ErrorMessageComponent } from 'components/error-message/error-message.component';
import { HttpErrorOccuredEventName, UnauthorizedEventName } from 'models/broadcast-events';
import { BroadcastService } from './broadcast.service';

@Injectable()
export class ErrorService {
    constructor(public modalService: ModalService, public router: Router, public broadcastService: BroadcastService) {
        this.broadcastService.getEvents([HttpErrorOccuredEventName]).subscribe(x => this.onError(x.args));
    }

    onError(error: any) {
        if (error.code == 401) {
            this.showAuthError();
        } else {
            this.showError(error.errorMessage);
        }
    }

    public showError(error: any) {
        this.modalService.addModal({ component: ErrorMessageComponent, modalConfig: undefined, params: { message: error, ok: this.modalService.removeModal.bind(this.modalService), close: this.modalService.removeModal.bind(this.modalService)  } });
    }

    private showAuthError() {
        this.broadcastService.next({ name: UnauthorizedEventName, args: { returlUrl: this.router.url } });
    }
}
