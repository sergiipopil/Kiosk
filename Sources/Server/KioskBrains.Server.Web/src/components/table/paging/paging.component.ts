import { Component, Input, Output, EventEmitter } from '@angular/core';
import { SearchRequestMetadata } from 'models/base/search-request-metadata';

@Component({
    selector: 'paging-component',
    templateUrl: 'paging.component.html',
    styleUrls: ['paging.scss']
})
export class PagingComponent {

    @Input()
    public paging: SearchRequestMetadata;

    @Input()
    public total: number;

    @Output()
    public onStartChange = new EventEmitter<number>();

    getPageSize(): number {
        return +this.paging.pageSize;
    }

    getCurrentStart(): number {
        return this.paging.start > 0 ? this.paging.start : 1;
    }

    onPageSizeChanged(): void {
        this.onStartChange.emit(this.getCurrentStart());
    }

    onFirst(): void {
        this.onStartChange.emit(1);
    }

    onPrevious(): void {
        let newStart = this.getCurrentStart() - this.getPageSize();
        if (newStart <= 0) {
            newStart = 1;
        }
        this.onStartChange.emit(newStart);
    }

    onNext(): void {
        this.onStartChange.emit(this.getCurrentStart() + this.getPageSize());
    }

    onLast(): void {
        if (!this.getPageSize() || !this.total) {
            return;
        }
        let lastStart = Math.floor((this.total - 1) / this.getPageSize()) * this.getPageSize() + 1;
        this.onStartChange.emit(lastStart);
    }

    hasPrevious(): boolean {
        return this.getCurrentStart() > 1;
    }

    hasNext(): boolean {
        return this.total - (this.getCurrentStart() + this.getPageSize()) >= 0;
    }

    getStartLabel() {
        return this.paging.start ? this.paging.start : 1;
    }

    getEndLabel() {
        return this.getPageSize() + this.getCurrentStart() > this.total
            ? this.total
            : this.getPageSize() + this.getCurrentStart() - 1;
    }
}
