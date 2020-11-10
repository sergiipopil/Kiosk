import { Component, Input, HostListener } from '@angular/core';

@Component({
    selector: 'advanced-search',
    templateUrl: 'advanced-search.component.html',
    styleUrls: ['advanced-search.scss']
})
export class AdvancedSearchComponent  {
    @Input() doSearch: Function;
    @Input() clearForm: Function;

    @HostListener('keydown.enter') onMouseEnter() {
        if (this.doSearch) {
            this.doSearch();
        }
    }
}