export class SearchRequestMetadata {
    constructor()
    {
        this.pageSize = 20;
        this.start = 0;
    }

    public start: number;
    public pageSize: number;
    public orderBy: string;
    public orderDirection: number;
}