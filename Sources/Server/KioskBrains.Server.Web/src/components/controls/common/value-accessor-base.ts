// Based on http://blog.rangle.io/angular-2-ngmodel-and-custom-form-components/

import { ControlValueAccessor } from '@angular/forms';


export class ValueAccessorBase<TValue> implements ControlValueAccessor {
    private innerValue: TValue;

    private changed = new Array<(value: TValue) => void>();
    private touched = new Array<() => void>();

    get value(): TValue {
        return this.innerValue;
    }

    set value(value: TValue) {
        if (this.innerValue !== value) {
            this.innerValue = value;
            this.changed.forEach(f => f(value));
        }
    }

    touch() {
        this.touched.forEach(f => f());
    }

    writeValue(value: TValue) {
        let previousValue = this.innerValue;
        this.innerValue = value;
        this.onValueWritten(previousValue, value);
    }

    protected onValueWritten(previousValue: TValue, newValue: TValue) {
    }

    registerOnChange(fn: (value: TValue) => void) {
        this.changed.push(fn);
    }

    registerOnTouched(fn: () => void) {
        this.touched.push(fn);
    }
}
