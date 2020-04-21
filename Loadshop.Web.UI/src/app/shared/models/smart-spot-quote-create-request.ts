import { Equipment } from './equipment';
import { Place } from './place';

export interface SmartSpotQuoteCreateRequest {
    origin: Place;
    destination: Place;
    equipment: Equipment;
    weight: number;
    pickupDate: Date;
}
