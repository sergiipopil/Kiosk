import { Component, Input, Optional, Inject, ViewChild, OnInit } from '@angular/core';
import { NgModel, NG_VALUE_ACCESSOR, NG_VALIDATORS, NG_ASYNC_VALIDATORS } from '@angular/forms';
import { FormControlBase } from 'components/controls/common/form-control-base';
import createNumberMask from 'text-mask-addons/dist/createNumberMask'
import { NumberValidator } from "validators/numberValidator";

export let numberValidator = new NumberValidator();

@Component({
    selector: 'number-input',
    templateUrl: './number-input.component.html',
    styleUrls: ['./number-input.component.scss'],
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: NumberInputComponent, multi: true },
        { provide: NG_VALIDATORS, useValue: numberValidator, multi: true }
    ]
})
export class NumberInputComponent extends FormControlBase<string> implements OnInit {
    constructor(
        @Optional() @Inject(NG_VALIDATORS) validators: Array<any> = [],
        @Optional() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: Array<any>,
    ) {
        super('number-input', validators, asyncValidators);
    }

    @Input()
    public maxlength: number = 9;

    @Input()
    public allowDecimal: boolean = true;

    @Input()
    public allowNegative: boolean = true;

    @Input()
    public isReadonlyValue: boolean = false;

    @Input()
    public tabindex: number;

    @Input()
    public novalidate: boolean = false;

    @ViewChild("container")
    model: NgModel;

    ngOnInit(): void {
        this.numberMask = {
            mask: createNumberMask({
                prefix: '',
                suffix: '',
                includeThousandsSeparator: false,
                thousandsSeparatorSymbol: '',
                allowDecimal: this.allowDecimal,
                decimalSymbol: '.',
                // todo: should be an option
                decimalLimit: 6,
                integerLimit: this.maxlength,
                requireDecimal: false,
                allowNegative: this.allowNegative,
                allowLeadingZeroes: false
            }) }
    }

    public numberMask: any;
}
