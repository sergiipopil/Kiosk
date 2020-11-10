import { Injectable, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { ROUTING_SITEMAP_DATA_KEY } from './routing-sitemap-data';
import { SitemapNode } from './sitemap-node';
import { RoutingSitemapData } from './routing-sitemap-data';

@Injectable()
export class SitemapService {
  constructor(private _router: Router) {
  }

  private _sitemap: Map<string, SitemapNode>;

  private _rootSitemapNode: SitemapNode;

  public rebuildSitemap(): void {
    this._sitemap = new Map();
    const routes = this._router.config;
    for (let route of routes) {
      let routingSitemapData: RoutingSitemapData = route.data
        ? route.data[ROUTING_SITEMAP_DATA_KEY]
        : undefined;

      // avoid node if no sitemap data presented
      if (!routingSitemapData) {
        continue;
      }

      let sitemapNode: SitemapNode = {
        title: routingSitemapData.title,
        path: route.path,
        icon: routingSitemapData.icon,
        hideInMenu: routingSitemapData.hideInMenu,
        containerOnly: routingSitemapData.containerOnly,
        parentPath: routingSitemapData.parentPath
      };

      this._sitemap.set(route.path, sitemapNode);
    }

    // build full sitemap tree
    this._rootSitemapNode = {
      title: null,
      hideInMenu: true,
      containerOnly: true,
      children: []
    };
    for (let sitemapNode of Array.from(this._sitemap.values())) {
      let parentSitemapNode: SitemapNode;
      if (sitemapNode.parentPath) {
        parentSitemapNode = this._sitemap.get(sitemapNode.parentPath);
        if (!parentSitemapNode) {
          parentSitemapNode = this._rootSitemapNode;
        }
      } else {
        parentSitemapNode = this._rootSitemapNode;
      }
      if (!parentSitemapNode.children) {
        parentSitemapNode.children = [];
      }
      parentSitemapNode.children.push(sitemapNode);
    }
  }

  private buildSitemapIfNotBuilt(): void {
    if (this._sitemap) {
      return;
    }

    // build flat sitemap nodes
    this.rebuildSitemap();
  }

  public getSitemapNodes(): SitemapNode[] {
    this.buildSitemapIfNotBuilt();
    return this._rootSitemapNode.children;
  }

  public getActiveSitemapPath(activeRoutePath: string, maxLength: number): SitemapNode[] {
    this.buildSitemapIfNotBuilt();
    let currentSitemapNode = this._sitemap.get(activeRoutePath);
    if (!currentSitemapNode) {
      return [];
    }

    let activePath: SitemapNode[] = [];
    do {
      activePath.push(currentSitemapNode);
      if (currentSitemapNode.parentPath) {
        currentSitemapNode = this._sitemap.get(currentSitemapNode.parentPath);
      } else {
        currentSitemapNode = null;
      }
    } while (currentSitemapNode && activePath.length < maxLength);

    return activePath.reverse();
  }
}
