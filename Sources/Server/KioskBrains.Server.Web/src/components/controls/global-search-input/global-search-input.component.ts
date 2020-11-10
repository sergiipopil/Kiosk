import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';

@Component({
    selector: 'global-search-input',
    templateUrl: './global-search-input.component.html',
    styleUrls: ['./global-search-input.component.scss']
})
export class GlobalSearchInputComponent {

    private _term: string;

    @HostListener('keydown.enter') onMouseEnter() {
        this.onSearch.emit();
    }

    @Input()
    get term(): string {
        return this._term;
    }

    set term(value: string) {
        this._term = value;
        this.termChange.emit(value);
    }

    @Output()
    termChange = new EventEmitter<string>();

    @Input()
    placeholder: string;

    @Output()
    onSearch = new EventEmitter();
}
