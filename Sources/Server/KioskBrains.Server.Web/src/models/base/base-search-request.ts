import { SearchRequestMetadata } from '../base/search-request-metadata';

export class BaseSearchRequest<T extends object> {
    constructor() {
        this.metadata = new SearchRequestMetadata();
        this.searchStruct = {} as T;
    }

    public searchTerm: string;
    public searchStruct: T;
    public isAdvancedSearch: boolean;
    public metadata: SearchRequestMetadata;
}