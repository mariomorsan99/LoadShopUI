export interface Carrier {
    carrierId: string;
    carrierName: string;
    carrierScacs: string[];
}

export const defaultCarrier: Carrier = {
    carrierId: null,
    carrierName: null,
    carrierScacs: null
};

export function isCarrier(x: any): x is Carrier {
    return typeof x.carrierId === 'string' && typeof x.carrierName === 'string';
}

export function isCarrierArray(x: any): x is Carrier[] {
    return x.every(isCarrier);
}
