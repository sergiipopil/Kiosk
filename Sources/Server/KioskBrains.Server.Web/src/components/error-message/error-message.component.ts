import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'error-message',
    templateUrl: 'error-message.component.html',
    styleUrls: ['error-message.component.scss']
})
export class ErrorMessageComponent {
    @Input() header: string;
    @Input() message: string;
    @Input() ok: Function;
    @Input() close: Function;

    okClick()
    {
        if (this.ok) {
            this.ok();
        }
    }
}
