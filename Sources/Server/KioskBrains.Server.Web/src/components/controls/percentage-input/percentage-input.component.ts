import { Component, Input, Optional, Inject, ViewChild } from '@angular/core';
import { NgModel, NG_VALUE_ACCESSOR, NG_VALIDATORS, NG_ASYNC_VALIDATORS } from '@angular/forms';
import { FormControlBase } from 'components/controls/common/form-control-base';
import createNumberMask from 'text-mask-addons/dist/createNumberMask'

@Component({
    selector: 'percentage-input',
    templateUrl: './percentage-input.component.html',
    styleUrls: ['./percentage-input.component.scss'],
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: PercentageInputComponent, multi: true }
    ]
})
export class PercentageInputComponent extends FormControlBase<number> {
    constructor(
        @Optional() @Inject(NG_VALIDATORS) validators: Array<any>,
        @Optional() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: Array<any>,
    ) {
        super('percentage-input', validators, asyncValidators);
    }

    @Input()
    public maxlength: number;

    @Input()
    public tabindex: number;

    @Input()
    public isReadonlyValue: boolean = false;

    public numberMask = createNumberMask({
        prefix: '',
        suffix: '',
        includeThousandsSeparator: true,
        thousandsSeparatorSymbol: ',',
        allowDecimal: true,
        decimalSymbol: '.',
        decimalLimit: 2,
        integerLimit: 6,
        requireDecimal: false,
        allowNegative: false,
        allowLeadingZeroes: false
    });

    @ViewChild("container")
    model: NgModel;
}
