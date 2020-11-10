import { Component, Input } from '@angular/core';

export const BaseFrameContainerClass = "base-frame-container";
@Component({
    selector: 'base-frame',
    templateUrl: 'base-frame.component.html',
    styleUrls: ['base-frame.scss']
})
export class BaseFrameComponent {

    @Input()
    isLoading: boolean;

    @Input()
    modalSize: string = 'default';

    @Input()
    hideHeader: boolean = false;
}
