import { Component, Input, EventEmitter, Output } from '@angular/core';

@Component({
    selector: 'all-option-wrapper',
    templateUrl: './all-option-wrapper.component.html',
    styleUrls: ['./all-option-wrapper.component.scss']
})
export class AllOptionWrapperComponent {
    @Input()
    label: string;

    @Input()
    value: boolean;

    @Input()
    isRequiredLabel: boolean = false;

    @Output()
    valueChange = new EventEmitter<boolean>();

    @Input()
    isReadonlyValue: boolean;

    toggle(): void {
        this.value = !this.value;
        this.valueChange.emit(this.value);
    }
}
