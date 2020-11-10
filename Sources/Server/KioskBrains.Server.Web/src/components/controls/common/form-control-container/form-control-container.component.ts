import { Component, Input } from '@angular/core';
import { FormControlBase } from 'components/controls/common/form-control-base';

@Component({
    selector: 'form-control-container',
    templateUrl: './form-control-container.component.html',
    styleUrls: ['./form-control-container.component.scss']
})
export class FormControlContainerComponent {
    @Input()
    control: FormControlBase<any>;
}
