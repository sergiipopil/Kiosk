export class BaseSearchResponse<T> {
    public records: T[];
    public total: number;
}