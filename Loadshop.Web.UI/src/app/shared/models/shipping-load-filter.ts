import { Place, Equipment, CustomerLoadType } from '.';

export interface IShippingLoadFilter {
    quickFilter: string;
    origin: Place;
    dest: Place;
    equipment: Equipment[];
    customerLoadTypes: CustomerLoadType[];
}

export class ShippingLoadFilter implements IShippingLoadFilter {
    constructor(init?: Partial<ShippingLoadFilter>) {
        Object.assign(this, init);
    }

    quickFilter: string;
    origin: Place;
    dest: Place;
    equipment: Equipment[];
    customerLoadTypes: CustomerLoadType[];
}
