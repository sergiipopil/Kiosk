import { Routes } from '@angular/router';
import { ModalGuard } from './routeGuards/modal.guard';
import { AuthGuard } from './routeGuards/auth.guard';
import { DashboardComponent } from 'components/actions/dashboard/dashboard.component';
import { UnderConstructionComponent } from 'components/under-construction/under-construction.component';
import { RoutingSitemapData } from 'app/sitemap/routing-sitemap-data';
import { CentralBankExchangeRateSearchComponent } from 'components/actions/central-bank-exchange-rate-search/central-bank-exchange-rate-search.component';
import { CentralBankExchangeRateDetailsComponent } from 'components/actions/central-bank-exchange-rate-details/central-bank-exchange-rate-details.component';
import { CentralBankExchangeRateUpdateDetailsComponent } from 'components/actions/central-bank-exchange-rate-update-details/central-bank-exchange-rate-update-details.component';
import { KioskStateSearchComponent } from 'components/actions/kiosk-state-search/kiosk-state-search.component';
import { KioskStateDetailsComponent } from 'components/actions/kiosk-state-details/kiosk-state-details.component';
import { LogRecordSearchComponent } from 'components/actions/log-record-search/log-record-search.component';
import { RoutingSitemapData as IRoutingSitemapData } from './sitemap/routing-sitemap-data';


// 'sitemap-node' is used instead of [ROUTING_SITEMAP_DATA_KEY] due to StaticReflector restrictions
// https://gist.github.com/chuckjaz/65dcc2fd5f4f5463e492ed0cb93bca60
// https://github.com/rangle/angular-2-aot-sandbox#aot-dos-and-donts

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full', data: { ignorePermissions: true } },
  // Dashboard
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthGuard, ModalGuard],
    data: {
      'sitemap-node': {
        title: 'Dashboard',
        icon: {
          className: 'pg-charts'
        }
      } as RoutingSitemapData,
      ignorePermissions: true
    }
  },
  // Monitoring
  {
    path: 'kiosk-state-search',
    component: KioskStateSearchComponent,
    canActivate: [AuthGuard, ModalGuard],
    data: {
      'sitemap-node': {
        title: 'Monitoring',
        icon: {
          className: 'fa fa-heartbeat'
        }
      } as RoutingSitemapData
    }
  },
  {
    path: 'kiosk-state-details',
    component: KioskStateDetailsComponent,
    canActivate: [AuthGuard, ModalGuard],
    data: {
      'sitemap-node': {
        parentPath: 'kiosk-state-search',
        title: 'Kiosk State Details',
        hideInMenu: true
      } as RoutingSitemapData
    }
  },
  // Transactions
  {
    path: 'transactions',
    redirectTo: 'ace-transaction-search',
    pathMatch: 'full',
    data: {
      'sitemap-node': {
        title: 'Transactions',
        containerOnly: true,
        icon: {
          className: 'fa fa-usd'
        }
      } as RoutingSitemapData,
    }
  },
  // Administration
  {
    path: 'administration',
    redirectTo: 'central-bank-exchange-rate-search',
    pathMatch: 'full',
    data: {
      'sitemap-node': {
        title: 'Administration',
        containerOnly: true,
        icon: {
          className: 'fa fa-cog'
        }
      } as RoutingSitemapData,
    }
  },
  // Central Bank Rates
  {
    path: 'central-bank-exchange-rate-search',
    component: CentralBankExchangeRateSearchComponent,
    canActivate: [AuthGuard, ModalGuard],
    data: {
      'sitemap-node': {
        parentPath: 'administration',
        title: 'Central Bank Rates',
        icon: {
          text: 'CB'
        }
      } as IRoutingSitemapData
    }
  },
  {
    path: 'central-bank-exchange-rate-details',
    component: CentralBankExchangeRateDetailsComponent,
    canActivate: [AuthGuard, ModalGuard],
    data: {
      'sitemap-node': {
        parentPath: 'central-bank-exchange-rate-search',
        title: 'Central Bank Rate Details',
        hideInMenu: true
      } as IRoutingSitemapData
    }
  },
  {
    path: 'central-bank-exchange-rate-update-details',
    component: CentralBankExchangeRateUpdateDetailsComponent,
    canActivate: [AuthGuard, ModalGuard],
    data: {
      'sitemap-node': {
        parentPath: 'central-bank-exchange-rate-search',
        title: 'Rate Update Details',
        hideInMenu: true
      } as RoutingSitemapData
    }
  },
  // Maintenance
  {
    path: 'maintenance',
    redirectTo: 'log-record-search',
    pathMatch: 'full',
    data: {
      'sitemap-node': {
        title: 'Maintenance',
        containerOnly: true,
        icon: {
          className: 'fa fa-bug'
        }
      } as RoutingSitemapData,
    }
  },
  // Kiosk Log
  {
    path: 'log-record-search',
    component: LogRecordSearchComponent,
    canActivate: [AuthGuard, ModalGuard],
    data: {
      'sitemap-node': {
        parentPath: 'maintenance',
        title: 'Kiosk Log',
        icon: {
          text: 'KL'
        }
      } as RoutingSitemapData
    }
  },
  // Default Route
  { path: '*', redirectTo: 'dashboard' }
];
