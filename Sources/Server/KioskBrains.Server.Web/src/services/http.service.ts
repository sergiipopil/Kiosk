import { Headers, Http, RequestOptionsArgs } from '@angular/http';
import { LocalStorageService } from 'angular-2-local-storage';
import { Injectable } from '@angular/core';
import { ReplaySubject, AsyncSubject } from 'rxjs';
import { BroadcastService } from './broadcast.service';
import { HttpErrorOccuredEventName } from 'models/broadcast-events';

@Injectable()
export class HttpService {
  private baseUrl: string;
  private token: string;
  private AUTH_HEADER: string = 'Authorization';

  constructor(public http: Http, public localStrgService: LocalStorageService, public broadcastService: BroadcastService) {
    this.baseUrl = "/api/";
  }

  getBaseUrl(): string {
    return this.baseUrl;
  }

  setBaseUrl(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  get(url: string, options?: RequestOptionsArgs, raw?: boolean): ReplaySubject<any> {
    let observer = new ReplaySubject(1);
    options = options || {};
    options.headers = this.createAuthorizationHeader();
    console.time("request");
    this.http.get(this.baseUrl + url, options).subscribe(s => {
      this.setAuthToken(s);

      if (raw) {
        observer.next(s);
      }
      else {
        let val = s.json();
        observer.next(val);
      }
      observer.complete();
      console.timeEnd("request");
    }, error => {
      this.handleError(error);
      observer.error(error);
      console.timeEnd("request");
    });
    return observer;
  }

  post(url: string, body: any, options?: RequestOptionsArgs): AsyncSubject<any> {
    let observer = new AsyncSubject();
    options = options || {};
    options.headers = this.createAuthorizationHeader();
    console.time("request");
    this.http.post(this.baseUrl + url, body, options).subscribe(s => {
      this.setAuthToken(s);
      let val = s.json();
      observer.next(val);
      observer.complete();
      console.timeEnd("request");
    }, error => {
      this.handleError(error);
      observer.complete();
      console.timeEnd("request");
    });
    return observer;
  }

  delete(url: string, options?: RequestOptionsArgs): AsyncSubject<any> {
    let observer = new AsyncSubject();
    options = options || {};
    options.headers = this.createAuthorizationHeader();
    console.time("request, url: " + url);
    this.http.delete(this.baseUrl + url, options).subscribe(s => {
      this.setAuthToken(s);
      let val = s.json();
      observer.next(val);
      observer.complete();
      console.timeEnd("request, url: " + url);
    }, error => {
      this.handleError(error);
      observer.next(null);
      observer.complete();
      console.timeEnd("request, url: " + url);
    });
    return observer;
  }

  setAuthToken(rs: { headers: Headers }) {
    let headers = rs.headers;
    let token = headers.get(this.AUTH_HEADER);
    if (token) {
      this.localStrgService.set("token", token);
    }
  }

  createAuthorizationHeader(): Headers {
    let headers = new Headers();
    let token = this.localStrgService.get<string>("token");
    if (token) {
      headers.append(this.AUTH_HEADER, 'Bearer ' + token);
    }
    return headers;
  }
  private handleError(error: any) {
    try {
      let body = error.json();
      this.broadcastService.next({ name: HttpErrorOccuredEventName, args: { ...body.meta } });
    }
    catch (e) {
      let errorMessage = error.text();
      let code = error.status;
      this.broadcastService.next({ name: HttpErrorOccuredEventName, args: { code, errorMessage } });
    }

  }

}
