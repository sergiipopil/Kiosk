import { HttpService } from 'services/http.service';
import { BaseViewModelComponent } from 'components/base-viewmodel/base-viewmodel.component';
import { Field } from 'models/field';
import { BaseSearchResponse } from 'models/base/base-search-response';
import { BaseSearchRequest } from 'models/base/base-search-request';
import { SearchRequestMetadata } from 'models/base/search-request-metadata';
import { NavigationService } from 'services/navigation.service';
import { ObjectHelper } from 'helpers/object-helper';
import * as XLSX from 'xlsx'

export class BaseSearchComponent<TViewModel extends BaseSearchResponse<any>, TRequest extends BaseSearchRequest<any>> extends BaseViewModelComponent<TViewModel, TRequest> {

  constructor(public http: HttpService, public navService: NavigationService) {
    super(http, navService);
    this.prepareRequest = this.prepareRequestFunc;
    this.clearRequest = this.clearRequestFunc;
  }

  doRequest(resetPage: boolean = true) {
    if (resetPage) {
      this.clearAdvancedSearch();
      this.resetPaging();
    }
    super.doRequest();
  }

  resetPaging() {
    this.requestModel.metadata.start = undefined;
    this.requestModel.metadata.orderBy = undefined;
    this.requestModel.metadata.orderDirection = undefined;
  }

  onSortingChange(field: Field) {
    if (field.sortable) {
      if (this.requestModel.metadata.orderBy == field.key) {
        this.requestModel.metadata.orderDirection = this.requestModel.metadata.orderDirection == 0 ? 1 : 0;
      } else {
        this.requestModel.metadata.orderBy = field.key;
        this.requestModel.metadata.orderDirection = 0;
      }
      super.doRequest();
    }
  }

  onStartChange(newStart: number) {
    this.requestModel.metadata.start = newStart;
    super.doRequest();
  }

  clearAdvancedSearch() {
    this.requestModel.isAdvancedSearch = undefined;
    this.requestModel.searchStruct = {};
  }

  doAdvancedSearch() {
    this.requestModel.isAdvancedSearch = true;
    this.requestModel.searchTerm = undefined;
    this.resetPaging();
    super.doRequest();
  }

  prepareRequestFunc() {
    if (!this.requestModel.metadata) {
      this.requestModel.metadata = new SearchRequestMetadata();
    }
  }

  ngOnChanges(changes: any) {
    if (changes.requestModel) {
      this.updateUrl = false;
      if (changes.requestModel.previousValue) {
        changes.requestModel.currentValue.metadata = changes.requestModel.previousValue.metadata;
        this.doRequest();
      }
    }
  }

  clearRequestFunc() {
    this.clearAdvancedSearch();
    this.requestModel.searchTerm = undefined;
    this.resetPaging();
  }

  public exportExcel(
    excelFileNameWithoutExtension: string,
    worksheetName: string,
    getHeadersFunc: () => string[],
    getDataRowForRecordFunc: (record: any) => any[]
  ) {
    if (!excelFileNameWithoutExtension || !getHeadersFunc() || !getDataRowForRecordFunc) {
      return;
    }

    let request = ObjectHelper.deepCopyViaJson(this.requestModel) as TRequest;
    request.metadata.start = 0;
    request.metadata.pageSize = 10000;
    this.sendGetRequest(request)
      .subscribe(data => {
        if (!data || data.records == null) {
          return;
        }
        // aoa - array of array
        var aoaData: string[][] = [getHeadersFunc()];
        for (var i = 0; i < data.records.length; i++) {
          let record = data.records[i];
          aoaData.push(getDataRowForRecordFunc(record));
        }

        const ws: XLSX.WorkSheet = XLSX.utils.aoa_to_sheet(aoaData);
        const wb: XLSX.WorkBook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, worksheetName);
        XLSX.writeFile(wb, excelFileNameWithoutExtension + '.xlsx');
      });
  }

}
