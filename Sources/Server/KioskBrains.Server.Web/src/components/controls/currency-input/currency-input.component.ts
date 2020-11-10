import { Component, Input, Optional, Inject, ViewChild , OnInit} from '@angular/core';
import { NgModel, NG_VALUE_ACCESSOR, NG_VALIDATORS, NG_ASYNC_VALIDATORS } from '@angular/forms';
import { FormControlBase } from 'components/controls/common/form-control-base';
import createNumberMask from "components/controls/currency-input/create-number-mask";

@Component({
    selector: 'currency-input',
    templateUrl: './currency-input.component.html',
    styleUrls: ['./currency-input.component.scss'],
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: CurrencyInputComponent, multi: true }
    ]
})
export class CurrencyInputComponent extends FormControlBase<number> implements OnInit {
    constructor(
        @Optional() @Inject(NG_VALIDATORS) validators: Array<any>,
        @Optional() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: Array<any>,
    ) {
        super('currency-input', validators, asyncValidators);
    }

    @Input()
    public sign: string;

    @Input()
    public isReadonlyValue: boolean = false;

    @Input()
    public allowNegative: boolean;

    public currencyMask: any;

    ngOnInit(): void {
        this.currencyMask = createNumberMask({
            prefix: '',
            suffix: '',
            includeThousandsSeparator: true,
            thousandsSeparatorSymbol: ',',
            allowDecimal: true,
            decimalSymbol: '.',
            decimalLimit: 2,
            integerLimit: 9,
            requireDecimal: true,
            allowNegative: !!this.allowNegative,
            allowLeadingZeroes: false
        });
    }

    @ViewChild("container")
    model: NgModel;
}
