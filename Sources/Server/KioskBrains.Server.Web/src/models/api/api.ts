import { BaseSearchRequest } from 'models/base/base-search-request';
import { BaseSearchResponse } from 'models/base/base-search-response';
import { ListOptionInt, ListOptionString } from 'models/base/list-option-int';
import { CommonDetailsGetResponse } from 'models/base/common-details-get-response';
import { CommonDetailsGetRequest } from 'models/base/common-details-get-request';
import { EmptyForm } from 'models/base/empty';


// ------------------------------------
// COMMON (WAF)
// ------------------------------------
export class FileInfo {
  fileName: string;
  fileKey: string;
}


// ------------------------------------
// COMMON (APP)
// ------------------------------------
export enum RemoteControlRequestStatusEnum {
  New = 1,
  Sent = 2,
  Completed = 3,
  Error = 4,
}


// ------------------------------------
// CENTRAL BANK EXCHANGE RATES
// ------------------------------------
export class PortalCentralBankExchangeRateSearchForm {
  localCurrencyCode: string;
  foreignCurrencyCode: string;
}

export class PortalCentralBankExchangeRateSearchGetRequest extends BaseSearchRequest<PortalCentralBankExchangeRateSearchForm> {
}

export class PortalCentralBankExchangeRateSearchRecord {
  id: number;
  localCurrencyCode: string;
  foreignCurrencyCode: string;
  rate?: number;
  defaultOrder?: number;
}

export class PortalCentralBankExchangeRateSearchGetResponse extends BaseSearchResponse<PortalCentralBankExchangeRateSearchRecord> {
  currencies: ListOptionString[];
  // todo: remove after new authorization model
  isNewAllowed: boolean;
}

export class PortalCentralBankExchangeRateDetailsForm {
  id?: number;
  localCurrencyCode: string;
  foreignCurrencyCode: string;
  defaultOrder?: number;
}

export class PortalCentralBankExchangeRateDetailsGetResponse extends CommonDetailsGetResponse<PortalCentralBankExchangeRateDetailsForm> {
  currencies: ListOptionString[];
}

export class PortalCentralBankExchangeRateUpdateLinkedSearchGetRequest extends BaseSearchRequest<EmptyForm> {
  centralBankExchangeRateId: number;
}

export class PortalCentralBankExchangeRateUpdateLinkedSearchRecord {
  id: number;
  centralBankExchangeRateId: number;
  startTime: Date;
}

export class PortalCentralBankExchangeRateUpdateLinkedSearchGetResponse extends BaseSearchResponse<PortalCentralBankExchangeRateUpdateLinkedSearchRecord> {
}

export class PortalCentralBankExchangeRateUpdateDetailsGetRequest extends CommonDetailsGetRequest {
  centralBankExchangeRateId: number;
}

export class PortalCentralBankExchangeRateUpdateDetailsForm {
  id?: number;
  centralBankExchangeRateId: number;
  startDate: Date;
  startTime: string;
  rate?: number;
}

export class PortalCentralBankExchangeRateUpdateDetailsGetResponse extends CommonDetailsGetResponse<PortalCentralBankExchangeRateUpdateDetailsForm> {
}


// ------------------------------------
// KIOSK STATES
// ------------------------------------
export enum ComponentStatusCodeEnum {
  Undefined = 0,
  Ok = 1,
  Warning = 2,
  Error = 3,
  Disabled = 5,
}

export class ComponentStatus {
  code: ComponentStatusCodeEnum;
  message: string;
}

export class ComponentMonitorableState {
  componentName: string;
  status: ComponentStatus;
  specificMonitorableStateJson: string;
}

export class PortalKioskStateSearchForm {
}

export class PortalKioskStateSearchGetRequest extends BaseSearchRequest<PortalKioskStateSearchForm> {
}

export class PortalKioskStateSearchRecord {
  id: number;
  addressLine1: string;
  addressLine2: string;
  city: string;
  country: string;
  assignedKioskVersion: string;
  kioskStateVersion: string;
  lastPingedMinutesAgo?: number;
  kioskStateJson: string;
  componentsStatuses: ComponentMonitorableState[];
}

export class PortalKioskStateSearchGetResponse extends BaseSearchResponse<PortalKioskStateSearchRecord> {
}

export class PortalKioskStateDetailsForm {
  id: number;
  addressLine1: string;
  addressLine2: string;
  city: string;
  country: string;
  assignedKioskVersion: string;
  kioskStateVersion: string;
  lastPingedMinutesAgo?: number;
  kioskStateJson: string;
  componentsStatuses: ComponentMonitorableState[];
}

export class PortalKioskStateDetailsGetResponse extends CommonDetailsGetResponse<PortalKioskStateDetailsForm> {
  kiosks: ListOptionInt[];
  statusDisplayName: string;
  statusMessage: string;
  componentRoles: ListOptionString[];
  operationNames: ListOptionString[];
}

// ------------------------------------
// LOG RECORDS
// ------------------------------------
export enum LogTypeEnum {
  Trace = 100,
  Info = 200,
  Warning = 300,
  Error = 400,
}

export enum LogContextEnum {
  Application = 2,
  Workflow = 3,
  Device = 5,
  File = 6,
  Component = 7,
  KioskAutoUpdater = 9,
  Communication = 10,
  Configuration = 11,
  Ui = 12,
}

export class PortalLogRecordSearchForm {
}

export class PortalLogRecordSearchGetRequest extends BaseSearchRequest<PortalLogRecordSearchForm> {
}

export class PortalLogRecordSearchRecord {
  id: number;
  kioskId: number;
  kioskVersion: string;
  localTime: string;
  type: LogTypeEnum;
  typeDisplayName: string;
  context: LogContextEnum;
  contextDisplayName: string;
  message: string;
  additionalDataJson: string;
}

export class PortalLogRecordSearchGetResponse extends BaseSearchResponse<PortalLogRecordSearchRecord> {
}
