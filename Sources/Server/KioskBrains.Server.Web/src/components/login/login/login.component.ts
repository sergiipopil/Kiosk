import { Component, Input } from '@angular/core';
import { NavigationService } from 'services/navigation.service';
import { LoginStatus } from 'models/login/login-status';
import { LoginService } from 'services/login.service';

@Component({
    selector: 'login',
    templateUrl: 'login.component.html',
    styleUrls: ['login.component.scss']
})
export class LoginComponent  {
    public userName: string;
    public password: string;
    public error: string;

    @Input() public returnUrl: string;

    constructor(public loginService: LoginService, public navService: NavigationService) {
        this.loginService.loginState.subscribe(val => this.updateStatus(val));
    }

    updateStatus(status: LoginStatus) {
        if (status.isLoggedIn) {
            this.navService.navigateNewScreen(this.returnUrl);
        }
        else if (status.error) {
            this.error = status.error;
        }
    }

    doLogin() {
        this.error = null;
        this.loginService.doLogin({ userName: this.userName, password: this.password, returnUrl: this.returnUrl });
    }
}