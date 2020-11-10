import { Component, Input, Optional, Inject, ViewChild, ElementRef, OnDestroy, Renderer, SimpleChanges, OnInit, OnChanges, HostListener, EventEmitter } from '@angular/core';
import { NgModel, NG_VALUE_ACCESSOR, NG_VALIDATORS, NG_ASYNC_VALIDATORS } from '@angular/forms';
import { FormControlBase } from 'components/controls/common/form-control-base';
import { ListOption } from 'models/base/list-option-int';
import { HttpService } from "services/http.service";

@Component({
    selector: 'select-input',
    templateUrl: './select-input.component.html',
    styleUrls: ['./select-input.component.scss'],
    providers: [
        { provide: NG_VALUE_ACCESSOR, useExisting: SelectInputComponent, multi: true }
    ]
})
export class SelectInputComponent extends FormControlBase<any> implements OnInit, OnDestroy, OnChanges {
    constructor(
        private http: HttpService,
        private _renderer: Renderer,
        @Optional() @Inject(NG_VALIDATORS) validators: Array<any>,
        @Optional() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: Array<any>
    ) {
        super('select-input', validators, asyncValidators);
    }

    @ViewChild("container")
    model: NgModel;

    @ViewChild('selectElement')
    _selectElement: ElementRef;

    @Input()
    public options: ListOption[];

    @Input()
    public useOptionsForInit: boolean = true;

    @Input()
    public isMultiple: boolean;

    @Input()
    public allowClear: boolean = true;

    @Input()
    public remoteAction: string;

    @Input()
    public link: string;

    @Input()
    public linkQueryParams: any;

    @Input()
    public remoteFilter: any;

    @Input()
    public minimumInputLength: number;

    //The minimum number of results required to display the search box.
    @Input()
    public minimumResultsForSearch: number = 20;

    @Input()
    public lazyComponentInit: boolean = true;

    ngOnInit(): void {
        if (!this.lazyComponentInit) {
            this.initPlugin();
        }
        //TODO: remove setTimeout
        //request initial options
        window.setTimeout(() => {
            if (this.remoteAction && !this.useOptionsForInit) {
                if (!this.value || this.value.length === 0) {
                    return;
                }
                let data: any = {
                    ids: this.isMultiple ? this.value : [this.value]
                }
                if (this.remoteFilter) {
                    data.filter = this.remoteFilter
                }
                this.http.get(this.remoteAction + "?request=" + encodeURIComponent(JSON.stringify(data))).subscribe(response => {
                    if (response.data && response.data.length > 0) {
                        this.options = response.data;
                    }
                });
            }
        }, 0);
    }

    ngOnDestroy(): void {
        this.select2Destroy();
    }

    ngOnChanges(changes: SimpleChanges): void {
        //on changes
        if (changes && changes.options && !changes.options.firstChange && !this.optionsAreSame(changes.options.previousValue, changes.options.currentValue)) {
            this.reloadPlugin();
        }
    }

    public isLoading: boolean;

    private showSelectElement: boolean = false;

    private showStaticHTML: boolean = true;

    private $select: any;

    //when isReadonlyValue sets true then destroy plugin
    private _isReadonlyValue: boolean = false;
    @Input()
    get isReadonlyValue(): boolean {
        return this._isReadonlyValue;
    };
    set isReadonlyValue(value: boolean) {
        this._isReadonlyValue = value;
        if (value) {
            this.select2Destroy();
        } else {
            this.initPlugin();
        }
    };

    //getters / setters
    get selectedOptions(): ListOption[] {
        let opts: ListOption[] = null;
        let value: any = this.isMultiple ? this.value : [this.value];
        if (value && this.options && value.length > 0 && this.options.length > 0) {
            switch (typeof value[0]) {
                case 'number':
                    this.options.forEach(x => x.value = +x.value);
                    value.forEach(x => x = +x);
                    opts = this.options.filter(x => value.indexOf(x.value) > -1);
                    break;
                case 'string':
                    this.options.forEach(x => x.value = '' + x.value);
                    value.forEach(x => x = '' + x);
                    opts = this.options.filter(x => value.indexOf(x.value) > -1);
                    break;
            }
        }

        return (opts && opts.length > 0) ? opts : null;
    }

    private initPlugin(needOpen: boolean = false) {
        this.showSelectElement = true;
        setTimeout(() => {
            this.$select = this.select2Init();
            if (needOpen) {
                this.$select.select2('open');
            }
            this.showStaticHTML = false;
        }, 0);
    }

    private reloadPlugin() {
        if (this.$select) {
            this.select2Destroy();
            this.$select = this.select2Init();
        }
    }

    //update element value written from outside
    public onValueWritten(pervVal, newVal) {
        if (newVal === 0 || newVal === "") {
            this.value = null;
        }
        this.setElementValue(newVal);
    }

    public initAndOpenOptions() {
        if (!this.disabled) {
            this.initPlugin(true);
        }
    }

    public loadOptions(term: string = null, ids: any[] = []) {
        if (this.remoteAction) {
            this.isLoading = true;
            let data: any = {};
            if (this.remoteFilter) {
                data.filter = this.remoteFilter;
            }
            if (term) {
                data.term = term;
            }
            if (ids && ids.length > 0) {
                data.ids = ids;
            }
            this.http.get(this.remoteAction + "?request=" + encodeURIComponent(JSON.stringify(data))).subscribe(response => {
                this.options = response.data || [];
                this.isLoading = false;
            });
        }
    }

    public clearOptionValue(optionValue: number) {
        if (optionValue && this.value && this.value.length > 0) {
            let valueIndex = this.value.indexOf(optionValue);
            let newVal = this.value.slice();
            newVal.splice(valueIndex, 1);
            this.value = newVal;
        }
    }

    public clearValue(withOptions: boolean = false) {
        this.value = this.isMultiple ? [] : null;
        if (withOptions) {
            this.options = null;
        }
        this.touch();
    }


    //----------------------------------------------------------------------------------------------------------------------------------------------------//
    //---------------------------------------------------------------   SELECT2   ------------------------------------------------------------------------//
    //----------------------------------------------------------------------------------------------------------------------------------------------------//
    private _select2Checked: boolean;

    public select2Init(): void {
        //already initialized
        if (this.$select) {
            return;
        }

        if (!this._selectElement || !this._selectElement.nativeElement) {
            throw new Error('Select element required for Select2');
        }

        this.$select = jQuery(this._selectElement.nativeElement);

        if (!this.$select.select2) {
            if (!this._select2Checked) {
                this._select2Checked = true;
                console.log('Please add Select2 library (js file) to the project.');
            }
            return;
        }

        // if select2 already initialized remove it and remove all tags inside
        if (this.$select.hasClass('select2-hidden-accessible') === true) {
            this.$select.select2('destroy');
            this._selectElement.nativeElement.innerHTML = '';
        }

        this.options = this.options || [];
        let select2Data: IdTextPair[] = this.options
            .map(x => ({
                id: x.value,
                text: x.displayName
            }));
        let select2Options: Select2Options = {
            data: select2Data,
            multiple: !!this.isMultiple,
            minimumInputLength: this.minimumInputLength,
            placeholder: " ",
            allowClear: this.allowClear,
            minimumResultsForSearch: this.remoteAction ? 0 : this.minimumResultsForSearch
        };
        if (this.remoteAction) {
            this.applyRemoteSettings(select2Options);
        }

        if (select2Options.matcher) {
            throw new Error('Select2 matcher is not supported at the moment.');
        } else {
            this.$select.select2(select2Options);
        }

        //subscribe
        this.$select.on('select2:select',
            () => {
                this.touch();
                this.value = this.$select.val() || (this.isMultiple ? [] : null);
            });
        this.$select.on('select2:unselect',
            () => {
                this.touch();
                if (this.isMultiple) {
                    this.value = this.$select.val() || [];
                }
                else {
                    this.value = null;
                }
            });

        this.setElementValue(this.value);

        return this.$select;
    }

    select2Destroy(): void {
        if (this.$select && this.$select.length > 0) {
            this.$select.off('select2:select');
            this.$select.off('select2:unselect');
            if (this.$select.hasClass('select2-hidden-accessible') === true) {
                this.$select.select2('destroy');
                this._selectElement.nativeElement.innerHTML = '';
            }

            this.$select = null;
        }
    }

    private applyRemoteSettings(select2Options: Select2Options) {
        if (select2Options && select2Options.minimumInputLength === null || select2Options.minimumInputLength === undefined) {
            select2Options.minimumInputLength = 2;
        }
        select2Options.ajax = {
            processResults: (response, params) => {
                if (response.data && response.data.length > 0) {
                    this.options = response.data || [];
                    response.data = response.data.map(v => {
                        return { id: v.value, text: v.displayName };
                    });
                }
                return {
                    results: response.data
                };
            },
            transport: function (params, success, failure) {
                this.isLoading = true;
                this.http.get(this.remoteAction + "?request=" + encodeURIComponent(JSON.stringify(params.data))).subscribe(s => {
                    if (success && success.call) {
                        success(s);
                    }
                    this.isLoading = false;
                }, e => {
                    if (failure && failure.call) {
                        failure();
                    }
                    this.isLoading = false;
                });
            }.bind(this),
            data: (params) => {
                let requestData: any = {
                    term: params.term,
                    callback: params.callback,
                    context: params.context
                };
                if (this.remoteFilter) {
                    requestData.filter = this.remoteFilter;
                }
                return requestData;
            },
            delay: 250,
            cache: true
        }
    }

    public setElementValue(newValue: any) {
        if (this.$select) {
            this.$select.val(newValue).trigger('change.select2');
        }
    }

    private optionsAreSame(x: any[], y: any[]) {
        if (x) {
            x = x.sort((a, b) => { return (a.value > b.value) ? -1 : 1; });
        }
        if (y) {
            y = y.sort((a, b) => { return (a.value > b.value) ? -1 : 1; });
        }

        return JSON.stringify(x) === JSON.stringify(y);
    }
}
