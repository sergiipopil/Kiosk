import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import * as moment from 'moment';

@Pipe({ name: 'boolToDisplayValue' })
export class BoolToDisplayValuePipe implements PipeTransform {
  transform(value?: boolean, trueVal: string = 'Yes', falseVal: string = 'No'): string {
    if (value == null) {
      return '';
    }
    return value ? trueVal : falseVal;
  }
}

@Pipe({ name: 'dateToDisplayValue' })
export class DateToDisplayValuePipe extends DatePipe implements PipeTransform {
  transform(value: any, pattern?: string): string {
    if (value) {
      return super.transform(value, pattern || 'yyyy-MM-dd');
    }
    return '';
  }
}

@Pipe({ name: 'currencyToDisplayValue' })
export class CurrencyToDisplayValuePipe extends DecimalPipe implements PipeTransform {
  transform(value: any, sign?: string, digitInfo?: string): string {
    value = value ? value : 0;
    if (value < 0) {
      return '(' + (sign ? sign : '') + super.transform(0 - value, digitInfo || '1.2-2') + ')';
    } else {
      return (sign ? sign : '') + super.transform(value, digitInfo || '1.2-2');
    }
  }
}

@Pipe({ name: 'numberToDisplayValue' })
export class NumberToDisplayValuePipe extends DecimalPipe implements PipeTransform {
  transform(value: any, digitInfo?: string): string {
    value = value ? value : 0;
    if (value < 0) {
      return '(' + super.transform(0 - value, digitInfo || '1.0-5') + ')';
    } else {
      return super.transform(value, digitInfo || '1.0-5');
    }
  }
}

@Pipe({ name: 'percentageToDisplayValue' })
export class PercentageToDisplayValuePipe extends DecimalPipe implements PipeTransform {
  transform(value: any, digitInfo?: string): string {
    value = value ? value : 0;
    if (value < 0) {
      return '(' + super.transform(value, digitInfo || '1.0-2') + '%)';
    } else {
      return super.transform(value, digitInfo || '1.0-2') + '%';
    }
  }
}

@Pipe({ name: 'emailToDisplayValue' })
export class EmailToDisplayValuePipe implements PipeTransform {
  transform(value: any): string {
    if (value) {
      return `<a href="mailto:${value}">${value}</a>`;
    }
    return '';
  }
}

@Pipe({ name: 'listOptionToDisplayValue' })
export class ListOptionToDisplayValuePipe implements PipeTransform {
  transform(value: any, property: string = 'displayName'): string {
    if (value && value.length > 0) {
      return value.map(x => x[property]).join(', ');
    }
    return '';
  }
}

@Pipe({ name: 'date2' })
export class Date2Pipe extends DatePipe implements PipeTransform {
  transform(value: any, pattern?: string): string {
    if (!value) {
      return '';
    }
    var momentValue = moment(value);
    if (!momentValue.isValid()) {
      return '';
    }
    return super.transform(momentValue.toDate(), pattern);
  }
}
