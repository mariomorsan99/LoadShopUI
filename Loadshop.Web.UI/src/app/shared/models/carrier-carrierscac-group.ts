import { Carrier } from './carrier';
import { CarrierScac } from './carrier-scac';

export interface CarrierCarrierScacGroup {
    carrier: Carrier;
    carrierScacs: CarrierScac[];
}
