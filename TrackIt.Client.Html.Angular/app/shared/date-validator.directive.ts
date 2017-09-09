import { Directive, Input, OnChanges, SimpleChanges } from '@angular/core';
import { AbstractControl, NG_VALIDATORS, Validator, ValidatorFn, Validators } from '@angular/forms';

export function dateValidator(): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } => {
        var valid: boolean = false;
        if (control && control.value) {
            var testDate = new Date(control.value);
            valid = !isNaN(testDate.valueOf());
            if (valid) {
                const dateRegex: RegExp = /^\d{1,2}\/\d{1,2}\/\d{4}$/;
                valid = dateRegex.test(control.value);
            }
        }
        return valid ? null : { 'date': 'Not a valid date' };
    };
}

@Directive({
    selector: '[date]',
    providers: [{ provide: NG_VALIDATORS, useExisting: DateValidatorDirective, multi: true }]
})
export class DateValidatorDirective implements Validator, OnChanges {
    @Input() dateString: string;
    private validationFunction = Validators.nullValidator;

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['date']) {
            this.validationFunction = dateValidator();
        } else {
            this.validationFunction = Validators.required;
        }
    }

    validate(control: AbstractControl): { [key: string]: any } {
        return this.validationFunction(control);
    }
}
