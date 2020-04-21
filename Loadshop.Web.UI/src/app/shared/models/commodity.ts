export interface Commodity {
    commodityId: number;
    commodityName: string;
}

export const defaultCommodity: Commodity = {
    commodityId: null,
    commodityName: null,
};

export function isCommodity(x: any): x is Commodity {
    return typeof x.commodityName === 'string'
        && typeof x.commodityId === 'number' ;
}

export function isCommodityArray(x: any): x is Commodity[] {
    return x.every(isCommodity);
}
