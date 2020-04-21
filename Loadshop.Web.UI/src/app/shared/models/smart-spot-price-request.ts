import { LoadCarrierScacData } from './load-carrier-scac-data';

export interface SmartSpotPriceRequest {
    loadId: string;
    weight: number;
    commodity: string;
    equipmentId: string;
    loadCarrierScacs: LoadCarrierScacData[];
}

export const defaultSmartSpotPriceRequest: SmartSpotPriceRequest = {
    loadId: null,
    weight: 0,
    commodity: null,
    equipmentId: null,
    loadCarrierScacs: null
};
