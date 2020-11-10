import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { SitemapService } from 'app/sitemap/sitemap.service';
import { SitemapNode } from 'app/sitemap/sitemap-node';
import { version } from 'models/api/version';

declare var jQuery: any;

@Component({
    selector: 'app-menu',
    templateUrl: './app-menu.component.html',
    styleUrls: ['./app-menu.component.scss']
})
export class AppMenuComponent implements AfterViewInit {

    constructor(sitemapService: SitemapService) {
        this.sitemapNodes = sitemapService.getSitemapNodes();
    }

    public sitemapNodes: SitemapNode[];

    public version = version;

    @ViewChild('pageSidebar')
    pageSidebar: ElementRef;

    ngAfterViewInit(): void {
        jQuery(this.pageSidebar.nativeElement).sidebar();
    }

    public filterMenuVisibleNodes(sitemapNodes: SitemapNode[]): SitemapNode[] {
        return sitemapNodes.filter(x => !x.hideInMenu);
    }

    public hasMenuVisibleChilds(sitemapNode: SitemapNode): boolean {
        return sitemapNode.children && this.filterMenuVisibleNodes(sitemapNode.children).length > 0;
    }
}
