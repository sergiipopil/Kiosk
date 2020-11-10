import { SitemapNodeIcon } from './sitemap-node-icon';

export const ROUTING_SITEMAP_DATA_KEY = 'sitemap-node';

export interface RoutingSitemapData {
  // path name of parent sitemap node
  parentPath?: string;
  title: string;
  hideInMenu?: boolean;
  containerOnly?: boolean;
  icon?: SitemapNodeIcon;
}
