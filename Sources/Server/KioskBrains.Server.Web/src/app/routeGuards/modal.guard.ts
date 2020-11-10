import { RouterModule, Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { ModalService } from 'services/modal.service';
import { ComponentHistoryEntry } from 'models/component-history-entry';
import { UrlHelper } from 'helpers/url-helper';

@Injectable()
export class ModalGuard implements CanActivate {
    constructor(private router: Router, private modalService: ModalService) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        if (route.queryParams['_t'] == 'modal') {
            let model = UrlHelper.getRequestModelFromParams(route.queryParams);
            this.modalService.addModal({ component: route.component, params: { requestModel: model, updateUrl: false } });
            return false;
        }
        else if (this.modalService.IsCurrentWindowModal && !route.queryParams['_t']) {
            let model = UrlHelper.getRequestModelFromParams(route.queryParams);
            this.modalService.replaceModal({ component: route.component, params: { requestModel: model, updateUrl: false } });
            return false;
        } else {
            this.modalService.removeModal(true);
            this.modalService.IsCurrentWindowModal = false;
        }
     
        return true;
    }
}
