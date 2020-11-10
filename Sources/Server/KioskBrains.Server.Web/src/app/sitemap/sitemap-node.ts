import { SitemapNodeIcon } from './sitemap-node-icon';

export interface SitemapNode {
  title: string;
  path?: string;
  icon?: SitemapNodeIcon;
  children?: SitemapNode[];
  hideInMenu: boolean;
  containerOnly: boolean;
  // path name of parent sitemap node
  parentPath?: string;
}