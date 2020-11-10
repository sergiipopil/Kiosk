import { EventEmitter } from '@angular/core';
import { BaseViewModelComponent } from 'components/base-viewmodel/base-viewmodel.component';
import { HttpService } from 'services/http.service';
import 'rxjs/add/operator/toPromise';
import 'rxjs/add/operator/map';
import { CommonDetailsGetRequest } from 'models/base/common-details-get-request';
import { CommonDetailsPostResponse } from 'models/base/common-details-post-response';
import { CommonDetailsGetResponse } from 'models/base/common-details-get-response';
import { NavigationService } from 'services/navigation.service';
import * as moment from 'moment';

export class BaseDetailsComponent<T extends CommonDetailsGetResponse<any>, K extends CommonDetailsGetRequest> extends BaseViewModelComponent<T, K>  {

  protected onProcessSaveSuccess: EventEmitter<CommonDetailsPostResponse> = new EventEmitter<CommonDetailsPostResponse>();

  constructor(public http: HttpService, public navService: NavigationService) {
    super(http, navService);
    this.clearRequest = this.clearRequestFunc;
  }

  public async processSaveAsync(options?: any): Promise<CommonDetailsPostResponse> {
    this.isLoading = true;

    let response = await this.http.post(this.actionName,
      Object.assign({}, { form: this.viewModel.form }, options))
      .toPromise();

    let apiResponse: any = response;
    if (apiResponse && apiResponse.meta && apiResponse.meta.code === 200) {
      this.setRecordUpdatedNotification();
      this.onProcessSaveSuccess.emit(Object.assign({}, this.viewModel.form, apiResponse.data));
      this.isLoading = false;
      return apiResponse.data as CommonDetailsPostResponse;
    } else {
      this.isLoading = false;
      return null;
    }
  }

  public async save(options?: any) {
    let response = await this.processSaveAsync(options);
    if (response) {
      this.requestModel = {
        id: response.id
      } as K;
      this.doRequest(true);
    };
  }

  public async saveAndExit() {
    let response = await this.processSaveAsync();
    if (response) {
      this.back();
    }
  }

  public exit() {
    this.back();
  }

  clearRequestFunc() {
    this.requestModel = {} as K;
  }

  protected setRecordUpdatedNotification() {
    this.clearNotifications();
    this.notifications.push({
      message: `Record saved [${moment().format("L LTS")}].`
    });
  }
};
