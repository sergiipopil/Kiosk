export type ListOptionValueType = number | string;

export interface ListOption {
    value: ListOptionValueType;
    displayName: string;
}

export interface ListOptionInt extends ListOption {
    value: number;
}

export interface ListTypedOptionInt extends ListOption {
    value: number;
    type: number;
}

export interface ListOptionString extends ListOption {
    value: string;
}