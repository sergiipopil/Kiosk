import { Component, Input, Optional, Inject, ViewChild, Output, EventEmitter } from '@angular/core';
import { NgModel, NG_VALUE_ACCESSOR, NG_VALIDATORS, NG_ASYNC_VALIDATORS } from '@angular/forms';
import { FormControlBase } from 'components/controls/common/form-control-base';

@Component({
    selector: 'boolean-input',
    templateUrl: './boolean-input.component.html',
    styleUrls: ['./boolean-input.component.scss'],
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: BooleanInputComponent, multi: true }
    ]
})
export class BooleanInputComponent extends FormControlBase<boolean> {
    constructor(
        @Optional() @Inject(NG_VALIDATORS) validators: Array<any>,
        @Optional() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: Array<any>,
    ) {
        super('boolean-input', validators, asyncValidators);
    }

    @Output()
    click: EventEmitter<boolean> = new EventEmitter<boolean>();

    @Input()
    public allowNull: boolean = false;

    @Input()
    public tabindex: number;

    @Input()
    public isReadonlyValue: boolean = false;

    @ViewChild("container")
    model: NgModel;

    checkBoxClicked() {
        this.click.emit(!this.value);
    }
}
