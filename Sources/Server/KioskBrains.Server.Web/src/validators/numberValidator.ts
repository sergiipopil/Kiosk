import { Validator } from "@angular/forms";
import { AbstractControl, ValidationErrors } from '@angular/forms';

export class NumberValidator implements Validator {
    public validate(control: AbstractControl): ValidationErrors {
        if (control.value) {
            if (!new RegExp(/^\d+(?:\.{1}\d+){0,}$/g).test(control.value)) {
                    return { "numberformat": true };
                }
        }
    }

    public registerOnValidatorChange(fn: () => void): void {
    }
}