import { Validator } from "@angular/forms";
import { AbstractControl, ValidationErrors } from '@angular/forms';

export class PhoneValidator implements Validator {
    constructor(public mask: Array<RegExp | string>) {
    }

    public validate(control: AbstractControl): ValidationErrors {
        if (control.value && this.mask && this.mask.length > 0) {
            let regExpStr = this.mask.map(v => {
                if (v instanceof RegExp) {
                    let item = v.toString();
                    return item.substr(item.indexOf("/") + 1, item.lastIndexOf("/") - 1) + "{1}";
                }
                else {
                    return ("[\\" + v + "]{1}")
                }
            }).join("");

            if (!new RegExp("^" + regExpStr + "$").test(control.value)) {
                return { "phoneformat": true };
            }
        }
    }

    public registerOnValidatorChange(fn: () => void): void {
    }
}