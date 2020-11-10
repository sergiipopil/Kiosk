import { Input, Output, EventEmitter, OnDestroy, AfterViewInit } from '@angular/core';
import { HttpService } from '../../services/http.service';
import { NotificationRecord } from 'models/base/notification-record';
import { UrlHelper } from 'helpers/url-helper';
import { NavigationService } from 'services/navigation.service';
import { Subscription, Observable, Subject } from 'rxjs';

import * as equal from 'deep-equal';

export class BaseViewModelComponent<TViewModel, TRequest> implements AfterViewInit, OnDestroy {
  @Input()
  public viewModel: TViewModel;
  @Output()
  public viewModelChange = new EventEmitter<TViewModel>();
  @Input()
  public requestModel: TRequest;

  public actionName: string;
  protected listener: EventListenerObject = this.decodeUrlAndLoad.bind(this);
  public isLoading: boolean = false;

  @Input()
  protected updateUrl: boolean = true;
  @Input()
  protected prepareRequest: Function = () => {};
  @Input()
  protected clearRequest: Function = () => {};

  public routeListener: Subscription;
  private previousState: { componentName: string, queryParams: any };

  constructor(public http: HttpService, public navService: NavigationService) {
  }

  back() {
    window.removeEventListener('popstate', this.listener);
    if (this.routeListener) {
      this.routeListener.unsubscribe();
    }
    this.navService.back();
  }

  doRequest(doReplace: boolean = false) {
    this.prepareRequest();
    this.updateRequest(this.requestModel, doReplace);
  }

  updateRequest(request: TRequest, doReplace: boolean) {
    if (this.updateUrl) {
      let queryString = this.constructRequest('', request);
      if (!doReplace && window.history.pushState && queryString) {
        window.history.pushState({ selfCall: true }, 'Title', window.location.pathname + '?' + queryString);
      } else {
        window.history.replaceState({ selfCall: true }, 'Title', window.location.pathname + '?' + queryString);
      }
    }
    this.doHttpRequest();
  }

  doHttpRequest() {
    if (!this.isLoading) {
      this.isLoading = true;
      let request = JSON.stringify(this.requestModel, (key, value) => value != undefined ? value : undefined);
      this.http.get(this.actionName + '?request=' + encodeURIComponent(request)).subscribe(val => {
          if (val)
            this.viewModel = val.data;
          this.isLoading = false;
          this.viewModelChange.emit(val.data);
        },
        error => {
          this.isLoading = false;
        });
    }
  }

  public sendGetRequest(request: TRequest): Observable<TViewModel> {
    var subject = new Subject<TViewModel>();
    this.isLoading = true;
    let requestJson = JSON.stringify(request, (key, value) => value != undefined ? value : undefined);
    this.http.get(this.actionName + '?request=' + encodeURIComponent(requestJson))
      .subscribe(
        val => {
          this.isLoading = false;
          if (val) {
            subject.next(val.data);
          }
          subject.complete();
        },
        error => {
          this.isLoading = false;
          subject.error(error);
        });
    return subject;
  }

  constructRequest(baseObjStr: string, request: any): string {
    let result = '';
    for (let property in request) {
      if (request[property] != undefined && request[property] != '' && request.hasOwnProperty(property)) {
        if (Object(request[property]) === request[property] &&
          !(request[property] instanceof Date)) {
          result += '&' + this.constructRequest((baseObjStr ? baseObjStr + '.' : '') + property + '.', request[property]);
        } else {
          result += (result.length > 1 ? '&' : '') + baseObjStr + property + '=' + this.formatProperty(request[property]);
        }
      }
    }
    return result;
  }

  formatProperty(prop: any): string {
    if (prop instanceof Date) {
      return prop.toISOString();
    }
    return prop.toString();
  }

  ngAfterViewInit() {
    // this is needed to prevent angular from throwing error about changing model
    setTimeout(() => {
        if (this.updateUrl) {
          this.previousState = { componentName: this.getCurrentComponentName(), queryParams: {} };
          window.removeEventListener('popstate', this.listener);
          window.addEventListener('popstate', this.listener);
          if (!this.routeListener) {
            this.routeListener = this.navService.activatedRoot.queryParams.subscribe(
              val => {
                if (this.shouldUpdate(val)) {
                  this.decodeUrlAndLoad({ selfCall: true });
                }
              }
            );
            this.decodeUrlAndLoad({ selfCall: true });
          }
        } else {
          this.prepareRequest();
          this.doHttpRequest();
        }
      },
      0);

  }

  ngOnDestroy() {
    if (this.routeListener) {
      this.routeListener.unsubscribe();
    }
    window.removeEventListener('popstate', this.listener);
  }

  decodeUrlAndLoad(event: any = null) {
    let urlParams = this.getUrlParams();
    let model: any = UrlHelper.getRequestModelFromParams(urlParams);
    if (event && event.selfCall) {
      this.requestModel = Object.assign({}, this.requestModel, model);
      this.doHttpRequest();
    } else {
      // back or forward button is clicked
      if (this.routeListener) {
        this.routeListener.unsubscribe();
      }

      if (this.shouldUpdate()) {
        this.clearRequest();
        this.requestModel = Object.assign({}, this.requestModel, model);
        this.doHttpRequest();
      }
    }

  }

  getUrlParams() {
    let match,
      pl = /\+/g, // Regex for replacing addition symbol with a space
      search = /([^&=]+)=?([^&]*)/g,
      decode = (s) => decodeURIComponent(s.replace(pl, ' ')),
      query = window.location.search.substring(1);

    let urlParams = {};
    while (match = search.exec(query))
      urlParams[decode(match[1])] = decode(match[2]);
    return urlParams;
  }

  shouldUpdate(queryParams: any = null): boolean {
    // check if only query params changed while leaving the component untouched, angular wont re-render component
    let currentComponentName = this.getCurrentComponentName();
    let shouldUpdate;
    if (!queryParams) {
      shouldUpdate = !this.previousState || this.previousState.componentName == currentComponentName;
    } else {
      shouldUpdate = !this.previousState || this.previousState.componentName == currentComponentName && !equal(queryParams && this.previousState.queryParams);
      this.previousState.queryParams = queryParams;
    }
    this.previousState = this.previousState || { queryParams: {}, componentName: '' };
    this.previousState.componentName = currentComponentName;
    return shouldUpdate;
  }

  getCurrentComponentName(): string {
    return location.pathname.match(/([^\/]*)\/*$/)[1];
  }

  // NOTIFICATIONS
  public notifications: NotificationRecord[] = [];

  protected clearNotifications() {
    this.notifications = [];
  }

}
