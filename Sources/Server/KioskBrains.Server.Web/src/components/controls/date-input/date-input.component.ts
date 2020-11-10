import { Component, Input, Optional, Inject, ViewChild, AfterViewInit, ElementRef, OnDestroy, SimpleChanges, OnChanges } from '@angular/core';
import { NgModel, NG_VALUE_ACCESSOR, NG_VALIDATORS, NG_ASYNC_VALIDATORS } from '@angular/forms';
import { FormControlBase } from 'components/controls/common/form-control-base';
import { DateToDisplayValuePipe } from 'helpers/pipes';

declare var jQuery: any;

@Component({
  selector: 'date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.scss'],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: DateInputComponent, multi: true }
  ]
})
export class DateInputComponent extends FormControlBase<string> implements AfterViewInit, OnChanges, OnDestroy {
  constructor(
    @Optional() @Inject(NG_VALIDATORS) validators: Array<any> = [],
    @Optional() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: Array<any>,
    public datePipe: DateToDisplayValuePipe
  ) {
    super('date-input', validators, asyncValidators);
  }

  @Input()
  public isReadonlyValue: boolean = false;

  @Input()
  public format: string = 'yyyy-MM-dd';

  @Input()
  public tabindex: number;

  public $input: any;

  @ViewChild('container')
  model: NgModel;

  @ViewChild('inputElement')
  inputElement: ElementRef;

  ngAfterViewInit() {
    this.datepickerInit();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.isReadonlyValue) {
      if (changes.isReadonlyValue.currentValue === true) {
        this.datepickerDestroy();
      } else {
        this.datepickerInit();
      }
    }
  }

  ngOnDestroy(): void {
    this.datepickerDestroy();
  }

  datepickerInit() {
    window.setTimeout(() => {
      if (this.$input || !this.inputElement || !this.inputElement.nativeElement) {
        return;
      }
      this.$input = jQuery(this.inputElement.nativeElement);

      this.$input.datepicker({
          autoclose: true,
          // todo: this.format.toLower or replace M on m
          format: 'yyyy-mm-dd'
        })
        .on('changeDate',
          (args) => {
            this.value = this.datePipe.transform(args.date, this.format);
          })
        .on('hide',
          (args) => {
            this.value = this.datePipe.transform(args.date, this.format);
          });
    });
  }

  datepickerDestroy() {
    if (!this.$input || !this.inputElement || !this.inputElement.nativeElement) {
      return;
    }
    this.$input.datepicker('destroy');
    this.$input = null;
  }

  onValueWritten(previousValue, newValue) {
    let newDate = new Date(newValue);
    if (newValue == undefined ||
      newValue == null
      //date in wrong format
      ||
      newDate.getTime() == 978300000000) {
      this.value = newValue = null;
    }
    if (this.inputElement && this.inputElement.nativeElement) {
      let formated = this.datePipe.transform(newValue, this.format);
      this.value = formated;
      if (this.$input) {
        this.$input.datepicker('setDate', formated);
      }
    }
  }
}
