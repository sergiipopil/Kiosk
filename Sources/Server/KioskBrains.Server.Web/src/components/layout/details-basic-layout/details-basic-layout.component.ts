import { Component, ContentChild, TemplateRef, Input, HostBinding } from '@angular/core';
import { FormCommandsAreaDirective } from 'components/layout/details-basic-layout/form-commands-area.directive';

@Component({
    selector: 'details-basic-layout',
    templateUrl: './details-basic-layout.component.html',
    styleUrls: ['./details-basic-layout.component.scss']
})
export class DetailsBasicLayoutComponent {

    @ContentChild(FormCommandsAreaDirective, { read: TemplateRef })
    _formCommandsTemplate: TemplateRef<any>;

    @Input()
    @HostBinding('class.fullWidth')
    isFullWidth: boolean;
}
