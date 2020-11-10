import { Component, Input, Optional, Inject, ViewChild } from '@angular/core';
import { NgModel, NG_VALUE_ACCESSOR, NG_VALIDATORS, NG_ASYNC_VALIDATORS } from '@angular/forms';
import { FormControlBase } from 'components/controls/common/form-control-base';
import { PhoneValidator } from "../../../validators/phoneValidator";

export let phoneValidator: PhoneValidator = new PhoneValidator([]);

@Component({
    selector: 'phone-input',
    templateUrl: './phone-input.component.html',
    styleUrls: ['./phone-input.component.scss'],
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: PhoneInputComponent, multi: true },
        { provide: NG_VALIDATORS, useValue: phoneValidator, multi: true }
    ]
})
export class PhoneInputComponent extends FormControlBase<string> {
    constructor(
        @Optional() @Inject(NG_VALIDATORS) validators: Array<any> = [],
        @Optional() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: Array<any>,
    ) {
        super('phone-input', validators, asyncValidators);
        phoneValidator.mask = this.mask;
    }

    @Input()
    public mask: Array<RegExp | string> = ['(', /\d/, /\d/, /\d/, ')', ' ', /\d/, /\d/, /\d/, '-', /\d/, /\d/, /\d/, /\d/];

    public textMask = {
        mask: this.mask,
        guide: false,
        keepCharPositions: false
    }

    @Input()
    public maxlength: number;

    @Input()
    public isReadonlyValue: boolean = false;

    @Input()
    public tabindex: number;

    @ViewChild("container")
    model: NgModel;
}
