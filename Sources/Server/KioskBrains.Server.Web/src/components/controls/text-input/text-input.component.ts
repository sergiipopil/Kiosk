import { Component, Input, Optional, Inject, ViewChild } from '@angular/core';
import { NgModel, NG_VALUE_ACCESSOR, NG_VALIDATORS, NG_ASYNC_VALIDATORS } from '@angular/forms';
import { FormControlBase } from 'components/controls/common/form-control-base';

@Component({
    selector: 'text-input',
    templateUrl: './text-input.component.html',
    styleUrls: ['./text-input.component.scss'],
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: TextInputComponent, multi: true }
    ]
})
export class TextInputComponent extends FormControlBase<string> {
    constructor(
        @Optional() @Inject(NG_VALIDATORS) validators: Array<any>,
        @Optional() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: Array<any>,
    ) {
        super('text-input', validators, asyncValidators);
    }

    @Input()
    public multiline: boolean = false;

    @Input()
    public isReadonlyValue: boolean = false;

    @Input()
    public maxlength: number = 255;

    @Input()
    public placeholder: string;

    @Input()
    public tabindex: number;

    @ViewChild("container")
    model: NgModel;
}
