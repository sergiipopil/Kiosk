import { Component, Input } from '@angular/core';

@Component({
    selector: 'validation-messages',
    templateUrl: './validation-messages.component.html',
    styleUrls: ['./validation-messages.component.scss']
})
export class ValidationMessagesComponent {
    @Input()
    messages: string[];
}
