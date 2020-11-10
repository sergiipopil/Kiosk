import { RouterModule, Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { LocalStorageService } from 'angular-2-local-storage';
import { ModalService } from 'services/modal.service';
import { LoginComponent } from 'components/login/login/login.component';

@Injectable()
export class AuthGuard implements CanActivate {
    constructor(private router: Router, public localStrgService: LocalStorageService, public modalService: ModalService) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        if (this.localStrgService.get("token")) {
            return true;
        }
        this.modalService.addModal({ component: LoginComponent, modalConfig: { isTranparent: false, hideCloseButton: true }, params: { returnUrl: state.url, queryParams: route.queryParams } });
        return false;
    }
}