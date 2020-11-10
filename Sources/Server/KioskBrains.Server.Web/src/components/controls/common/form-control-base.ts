// Based on http://blog.rangle.io/angular-2-ngmodel-and-custom-form-components/

import { Input } from '@angular/core';
import { NgModel, Validator } from '@angular/forms';
import { Observable } from 'rxjs';
import { ValueAccessorBase } from './value-accessor-base';
import {
    AsyncValidatorArray,
    ValidatorArray,
    ValidationResult,
    message,
    validate,
    } from './validate-utils';


let formControlIdGlobalCounter = 0;

export abstract class FormControlBase<TValue> extends ValueAccessorBase<TValue> {
    protected abstract model: NgModel;

    // we get these arguments from @Inject on the derived class
    constructor(public controlName: string = "", private validators: ValidatorArray = null, private asyncValidators: AsyncValidatorArray = null) {
        super();
    }

    protected validate(): Observable<ValidationResult> {
        return validate(this.validators, this.asyncValidators)(this.model.control);
    }

    public get invalid(): Observable<boolean> {
        return this.validate().map(v => Object.keys(v || {}).length > 0);
    }

    public get failures(): Observable<Array<string>> {
        return this.validate().map(v => Object.keys(v).map(k => message(v, k)));
    }

    public controlId = `${this.controlName}-${formControlIdGlobalCounter++}`;

    public get controlInstance(): FormControlBase<TValue> {
        return this;
    }

    @Input()
    public label: string;

    @Input()
    public disabled: boolean;

    @Input()
    public hideValidationMessages: boolean;

    @Input()
    public hint: string;

    @Input()
    public isReadonlyValue: boolean;
}
