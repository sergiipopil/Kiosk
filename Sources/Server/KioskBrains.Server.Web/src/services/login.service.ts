import { Injectable } from '@angular/core';
import { ReplaySubject, Subject  } from 'rxjs';
import { LocalStorageService } from 'angular-2-local-storage';
import { LoginResponse } from 'models/login/login-response';
import { LoginRequest } from 'models/login/login-request';
import { LoginStatus } from 'models/login/login-status';
import { ModalService } from './modal.service';
import { HttpService } from './http.service';
import { LoginComponent } from 'components/login/login/login.component';
import { UnauthorizedEventName } from 'models/broadcast-events';
import { BroadcastService } from './broadcast.service';


@Injectable()
export class LoginService {
    private loginSubject = new Subject<LoginStatus>();
    public loginState = this.loginSubject.asObservable();
    public loginStatus: LoginStatus;
    private timeout: number = 60000;
    private loginTimeout: number = 3000000;
    private logoutTimeoutId: number;

    constructor(public localStrg: LocalStorageService,
        public modalService: ModalService,
        public httpService: HttpService,
        public broadcastService: BroadcastService) {
        this.loginStatus = {
            isLoggedIn: !!this.localStrg.get("token"),
            name: this.localStrg.get<string>("userName"),
            error: null
        };
        this.broadcastService.getEvents([UnauthorizedEventName]).subscribe(x => this.doLogout(x.args.returnUrl));
        this.checkLogin();
        this.runLoginCheck();
    }

    private setLoginStatus(isLoggedIn: boolean, name: string, error: string) {
        this.loginStatus.isLoggedIn = isLoggedIn;
        this.loginStatus.name = name;
        this.loginStatus.error = error;
    }

    public doLogin(loginRequest: LoginRequest) {
        this.httpService.post('portal-login', { ...loginRequest }).subscribe(val => {
            let res: LoginResponse = val.data;
            if (!val.isError && res.token) {
                this.localStrg.set("token", res.token);
                this.localStrg.set("userName", res.user.fullName);
                this.setLoginStatus(true, res.user.fullName, null);
                this.loginSubject.next(this.loginStatus);
                this.modalService.removeModal();
                this.runLogoutCheck();
            } else {
                this.setLoginStatus(false, "", 'Login or password is incorrect');
                this.loginSubject.next(this.loginStatus);
            }
        });    
    }


    public doLogout(returnUrl: string) {
        this.localStrg.remove("token");
        this.localStrg.remove("userName");
        this.loginStatus = { isLoggedIn: false, name: "", error: null };
        this.setLoginStatus(false, "", null);
        this.loginSubject.next(this.loginStatus);
        this.requestLogin(returnUrl);
    }

    public requestLogin(returnUrl: string) {
        this.modalService.addModal({ component: LoginComponent, modalConfig: { hideCloseButton: true, isTransparent: false }, params: { returnUrl: returnUrl } });
    }

    public runLoginCheck()
    {
        var callback = jQuery['throttle'](this.timeout, false, this.checkLogin.bind(this));
        window.addEventListener('click', callback, true);
        window.addEventListener('focus', callback, true);
        window.addEventListener('keypress', callback, true);
        window.addEventListener('mousemove', callback, true);
    }

    runLogoutCheck() {
        window.clearTimeout(this.logoutTimeoutId);
        this.logoutTimeoutId = window.setTimeout(() => this.doLogout(""), this.loginTimeout);
    }

    public checkLogin() {
        window.clearTimeout(this.logoutTimeoutId);
        if (this.loginStatus.isLoggedIn)
        {
            this.httpService.get("ping?request={}");
            this.runLogoutCheck();
        }
    }
}
