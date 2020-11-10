import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AppComponent } from './app.component';
import { NguiOverlayModule } from '@ngui/overlay';
import { AppMenuComponent } from 'app/app-menu/app-menu.component';
import { LoaderComponent } from 'components/loader/loader.component';
import { LoaderService } from 'services/loader.service';
import { AppRoutingModule } from 'app/app-routing.module';
import { LocalStorageModule } from 'angular-2-local-storage';
import { HttpService } from 'services/http.service';
import { DashboardComponent } from 'components/actions/dashboard/dashboard.component';
import { BaseFrameComponent } from 'components/base-frame/base-frame.component';
import { TableHeaderComponent } from 'components/table/table-header/table-header.component';
import { PagingComponent } from 'components/table/paging/paging.component';
import { HttpModule } from '@angular/http';
import { UnderConstructionComponent } from 'components/under-construction/under-construction.component';
import { SitemapService } from 'app/sitemap/sitemap.service';
import { AppBreadcrumbComponent } from 'app/app-breadcrumb/app-breadcrumb.component';
import { CommandPanelComponent } from 'components/layout/command-panel/command-panel.component';
import { CommandGroupComponent } from 'components/layout/command-group/command-group.component';
import { AdvancedSearchComponent } from 'components/advanced-search/advanced-search.component';
import { NotificationsComponent } from 'components/controls/notifications/notifications.component';
import { FormFieldGroupComponent } from 'components/layout/form-field-group/form-field-group.component';
import { LoginComponent } from 'components/login/login/login.component';
import { LoginStatusComponent } from 'components/login/login-status/login-status.component';
import { TabsComponent } from 'components/layout/tabs/tabs.component';
import { TabComponent } from 'components/layout/tabs/tab.component';
import { SearchBasicLayoutComponent } from 'components/layout/search-basic-layout/search-basic-layout.component';
import { GlobalSearchInputComponent } from 'components/controls/global-search-input/global-search-input.component';
import { OpenSidebarButtonComponent } from 'components/controls/open-sidebar-button/open-sidebar-button.component';
import { ModalOutletComponent } from 'components/modal-outlet/modal-outlet.component';
import { ModalService } from 'services/modal.service';
import { DynamicModalWrapper } from 'components/dynamic-modal-wrapper/dynamic-modal-wrapper.component';
import { NavigationService } from 'services/navigation.service';
import { LoginService } from 'services/login.service';
import { DateInputComponent } from 'components/controls/date-input/date-input.component';
import { SearchLinkedLayoutComponent } from 'components/layout/search-linked-layout/search-linked-layout.component';
import { SearchReportLayoutComponent } from 'components/layout/search-report-layout/search-report-layout.component';
import { EditableTableLayoutComponent } from 'components/layout/editable-table-layout/editable-table-layout.component';
import { ValidationMessagesComponent } from 'components/controls/validation-messages/validation-messages.component';
import { SelectInputComponent } from 'components/controls/select-input/select-input.component';
import { BroadcastService } from 'services/broadcast.service';
import { DetailsBasicLayoutComponent } from 'components/layout/details-basic-layout/details-basic-layout.component';
import { FormCommandsAreaDirective } from 'components/layout/details-basic-layout/form-commands-area.directive';
import { AllOptionWrapperComponent } from 'components/controls/all-option-wrapper/all-option-wrapper.component';
import { BoolToDisplayValuePipe, DateToDisplayValuePipe, CurrencyToDisplayValuePipe, PercentageToDisplayValuePipe, EmailToDisplayValuePipe, ListOptionToDisplayValuePipe, NumberToDisplayValuePipe, Date2Pipe } from 'helpers/pipes';
import { ErrorMessageComponent } from 'components/error-message/error-message.component';
import { ErrorService } from 'services/error.service';
import { FormControlContainerComponent } from 'components/controls/common/form-control-container/form-control-container.component';
import { CurrencyInputComponent } from 'components/controls/currency-input/currency-input.component';
import { BooleanInputComponent } from 'components/controls/boolean-input/boolean-input.component';
import { PhoneInputComponent } from 'components/controls/phone-input/phone-input.component';
import { NumberInputComponent } from 'components/controls/number-input/number-input.component';
import { PercentageInputComponent } from 'components/controls/percentage-input/percentage-input.component';
import { TextInputComponent } from 'components/controls/text-input/text-input.component';
import { EmailInputComponent } from 'components/controls/email-input/email-input.component';
import { TextMaskModule } from 'angular2-text-mask';
import { CentralBankExchangeRateSearchComponent } from 'components/actions/central-bank-exchange-rate-search/central-bank-exchange-rate-search.component';
import { CentralBankExchangeRateDetailsComponent } from 'components/actions/central-bank-exchange-rate-details/central-bank-exchange-rate-details.component';
import { CentralBankExchangeRateUpdateDetailsComponent } from 'components/actions/central-bank-exchange-rate-update-details/central-bank-exchange-rate-update-details.component';
import { CentralBankExchangeRateUpdateLinkedSearchComponent } from 'components/actions/central-bank-exchange-rate-update-linked-search/central-bank-exchange-rate-update-linked-search.component';
import { KioskStateDetailsComponent } from 'components/actions/kiosk-state-details/kiosk-state-details.component';
import { KioskStateSearchComponent } from 'components/actions/kiosk-state-search/kiosk-state-search.component';
import { LogRecordSearchComponent } from 'components/actions/log-record-search/log-record-search.component';
import { AdditionalDataCommonFormatterComponent } from 'components/actions/kiosk-state-search/additional-data-common-formatter/additional-data-common-formatter.component';
import { ConfirmMessageComponent } from 'components/actions/confirm-message/confirm-message.component';


@NgModule({
  declarations: [
    AdvancedSearchComponent,
    AllOptionWrapperComponent,
    AppBreadcrumbComponent,
    AppComponent,
    AppMenuComponent,
    BaseFrameComponent,
    BoolToDisplayValuePipe,
    BooleanInputComponent,
    CommandGroupComponent,
    CommandPanelComponent,
    CurrencyInputComponent,
    CurrencyToDisplayValuePipe,
    DashboardComponent,
    DateInputComponent,
    DateToDisplayValuePipe,
    Date2Pipe,
    DetailsBasicLayoutComponent,
    DynamicModalWrapper,
    EditableTableLayoutComponent,
    EmailInputComponent,
    EmailToDisplayValuePipe,
    ErrorMessageComponent,
    FormCommandsAreaDirective,
    FormControlContainerComponent,
    FormFieldGroupComponent,
    GlobalSearchInputComponent,
    ListOptionToDisplayValuePipe,
    LoaderComponent,
    LoginComponent,
    LoginStatusComponent,
    ModalOutletComponent,
    NotificationsComponent,
    NumberInputComponent,
    NumberToDisplayValuePipe,
    OpenSidebarButtonComponent,
    PagingComponent,
    PercentageInputComponent,
    PercentageToDisplayValuePipe,
    PhoneInputComponent,
    SearchBasicLayoutComponent,
    SearchLinkedLayoutComponent,
    SearchReportLayoutComponent,
    SelectInputComponent,
    TabComponent,
    TableHeaderComponent,
    TabsComponent,
    TextInputComponent,
    UnderConstructionComponent,
    ValidationMessagesComponent,
    KioskStateDetailsComponent,
    KioskStateSearchComponent,
    AdditionalDataCommonFormatterComponent,
    LogRecordSearchComponent,
    CentralBankExchangeRateSearchComponent,
    CentralBankExchangeRateDetailsComponent,
    CentralBankExchangeRateUpdateDetailsComponent,
    CentralBankExchangeRateUpdateLinkedSearchComponent,
    ConfirmMessageComponent
  ],
  entryComponents: [
    LoginComponent,
    ErrorMessageComponent,
    ConfirmMessageComponent
  ],
  imports: [
    BrowserModule,
    CommonModule,
    FormsModule,
    NguiOverlayModule,
    HttpModule,
    LocalStorageModule.withConfig({
      prefix: 'admin-portal-app',
      storageType: 'localStorage'
    }),
    AppRoutingModule,
    TextMaskModule
  ],
  providers: [
    DateToDisplayValuePipe,
    Date2Pipe,
    LoaderService,
    ErrorService,
    HttpService,
    SitemapService,
    ModalService,
    NavigationService,
    BroadcastService,
    LoginService
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
