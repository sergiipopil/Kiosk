import { Injectable } from '@angular/core';
import { Router, ActivatedRoute, ActivatedRouteSnapshot, ResolveEnd } from '@angular/router';
import { Subject } from 'rxjs/Subject';
import { ModalService } from './modal.service';
import { Observable } from 'rxjs';

@Injectable()
export class NavigationService {
    constructor(private modalService: ModalService, private router: Router) {
    }

    public get activatedRoot(): ActivatedRoute
    {
        return this.router.routerState.root;
    }

    public get activatedRouteData(): Observable<any> {
        return this.router.routerState.root.children[0].data;
    }

    navigate(url: string, params: {}) {
        url = url || '/dashboard';
        this.router.navigateByUrl(url, { queryParams: params, preserveQueryParams: true });
    }


    navigateWithModal(url: string, params: {}) {
        url = url || '/dashboard';
        url = this.addQueryParam(url, '_t', 'modal');
        this.router.navigateByUrl(url, { queryParams: params });
    }

    navigateNewScreen(url: string, params: object = {}) {
        url = url || '/dashboard';
        url = this.addQueryParam(url, '_t', 'screen');
        this.router.navigateByUrl(url);
    }

    addQueryParam(url: string, key: string, value: string) {
        if (!url || url.indexOf('&' + key) < 0 && url.indexOf('?' + key) < 0) {
            return url && url.indexOf('?') > -1 ? `${url}&${key}=${value}` : `${url}?${key}=${value}`;
        }
        return url;
    }

    back() 
    {
        if (this.modalService.IsCurrentWindowModal)
        {
            this.modalService.removeModal(false);
        } else 
        {
            window.history.back();
        }
    }
}
