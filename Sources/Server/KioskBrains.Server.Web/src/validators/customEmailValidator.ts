import { Validator } from "@angular/forms";
import { AbstractControl, ValidationErrors } from '@angular/forms';

export class CustomEmailValidator implements Validator {
    public validate(control: AbstractControl): ValidationErrors {
        if (control.value) {
            if (!new RegExp(/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/g).test(control.value)) {
                return { "emailformat": true };
            }
        }
    }

    public registerOnValidatorChange(fn: () => void): void {
    }
}
