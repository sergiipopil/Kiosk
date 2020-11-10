import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import 'rxjs/add/operator/filter';
import { SitemapService } from 'app/sitemap/sitemap.service';
import { SitemapNode } from 'app/sitemap/sitemap-node';

// todo: move router/sitemap-related logic to BreadcrumbService

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './app-breadcrumb.component.html',
  styleUrls: ['./app-breadcrumb.component.scss']
})
export class AppBreadcrumbComponent implements OnInit {

  constructor(
    private _activatedRoute: ActivatedRoute,
    private _router: Router,
    private _sitemapService: SitemapService) {
  }

  ngOnInit(): void {
    this._router.events
      .filter(event => event instanceof NavigationEnd)
      .subscribe(event => {
        // todo: review ActivatedRoute usage
        let activatedRoute: ActivatedRoute = this._activatedRoute.root.firstChild;
        if (activatedRoute) {
          var routeConfig = activatedRoute.routeConfig;
          let breadcrumbNodes = this._sitemapService.getActiveSitemapPath(routeConfig.path, 3);
          if (breadcrumbNodes.length > 1) {
            this.parentBreadcrumbNodes = breadcrumbNodes.slice(0, breadcrumbNodes.length - 1);
            this.activeBreadcrumbNode = breadcrumbNodes[breadcrumbNodes.length - 1];
          } else {
            this.parentBreadcrumbNodes = null;
            this.activeBreadcrumbNode = breadcrumbNodes.length > 0 ? breadcrumbNodes[0] : null;
          }
        }
      });
  }

  public parentBreadcrumbNodes: SitemapNode[];

  public activeBreadcrumbNode: SitemapNode;
}
