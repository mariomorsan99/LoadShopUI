export interface SmartSpotQuoteRequest {
    originCity: string;
    originState: string;
    originPostalCode: string;
    originCountry: string;
    destinationCity: string;
    destinationState: string;
    destinationPostalCode: string;
    destinationCountry: string;
    equipmentId: string;
    weight: number;
    pickupDate: Date;
}
