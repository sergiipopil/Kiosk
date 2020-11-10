import { Component, Input, Output, ChangeDetectorRef, AfterViewInit, ViewChild, Renderer } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { NavigationService } from 'services/navigation.service';
import { LoginStatus } from 'models/login/login-status';
import { LoginService } from 'services/login.service';

@Component({
    selector: 'login-status',
    templateUrl: 'login-status.component.html',
    styleUrls: ['login-status.scss']
})
export class LoginStatusComponent  {
    public loginStatus: LoginStatus = new LoginStatus();

    constructor(public loginService: LoginService, public router: Router) {
        this.loginStatus = this.loginService.loginStatus;
        this.loginService.loginState.subscribe(val => this.updateStatus(val));
    }

    updateStatus(status: LoginStatus) {
        this.loginStatus = status;
    }

    logout() {
        this.loginService.doLogout(this.router.routerState.snapshot.url);
    }
}