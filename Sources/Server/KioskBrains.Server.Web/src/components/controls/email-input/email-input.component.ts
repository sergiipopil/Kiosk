import { Component, Input, Optional, Inject, ViewChild } from '@angular/core';
import { NgModel, NG_VALUE_ACCESSOR, NG_VALIDATORS, NG_ASYNC_VALIDATORS } from '@angular/forms';
import { FormControlBase } from 'components/controls/common/form-control-base';
import { CustomEmailValidator } from "../../../validators/customEmailValidator";

export let customEmailValidator = new CustomEmailValidator();

@Component({
    selector: 'email-input',
    templateUrl: './email-input.component.html',
    styleUrls: ['./email-input.component.scss'],
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: EmailInputComponent, multi: true },
        { provide: NG_VALIDATORS, useValue: customEmailValidator, multi: true }
    ]
})
export class EmailInputComponent extends FormControlBase<string> {
    constructor(
        @Optional() @Inject(NG_VALIDATORS) validators: Array<any>,
        @Optional() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: Array<any>
    ) {
        super('email-input', validators, asyncValidators);
    }

    @Input()
    public maxlength: number = 255;

    @Input()
    public tabindex: number;

    @Input()
    public isReadonlyValue: boolean = false;


    @ViewChild("container")
    model: NgModel;
}
