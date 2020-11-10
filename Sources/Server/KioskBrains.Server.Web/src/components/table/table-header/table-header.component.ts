import { Component, Input} from '@angular/core';
import { Field } from '../../../models/field';

@Component({
    selector: '[table-header-component]',
    templateUrl: 'table-header.component.html',
    styleUrls: ['table-header.scss']
})
export class TableHeaderComponent {
    @Input() public fields: Field[];
    @Input() public orderDirection: number;
    @Input() public orderBy: string;
    @Input() public onSortingChange: Function = () => { };
}